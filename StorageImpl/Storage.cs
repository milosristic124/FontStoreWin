using Encryption;
using FontInstaller;
using Logging;
using Protocol.Payloads;
using Protocol.Transport.Http;
using Storage.Data;
using Storage.Impl.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Extensions;

namespace Storage.Impl {
  public class Storage : IStorage {
    #region private data
    private FSStorage _HDDStorage;
    private FSSynchronizationAgent _fsAgent;
    private FontInstallerAgent _installAgent;

    private Action<int> _synchronizationCallback;
    #endregion

    #region properties
    public IFontInstaller Installer { get; private set; }
    public string SessionID {
      get {
        return _HDDStorage.SessionID;
      }
      set {
        _HDDStorage.SessionID = value;
      }
    }

    public int? LastCatalogUpdate {
      get {
        return _HDDStorage.LastCatalogUpdate;
      }
      set {
        _HDDStorage.LastCatalogUpdate = value;
      }
    }
    public int? LastFontStatusUpdate {
      get {
        return _HDDStorage.LastFontStatusUpdate;
      }
      set {
        _HDDStorage.LastFontStatusUpdate = value;
      }
    }

    public bool Loaded { get; private set; }
    public bool HasChanged { get; private set; }

    public IList<Family> ActivatedFamilies {
      get {
        return FamilyCollection.Filtered(family => {
          return family.HasActivatedFont;
        }).ToList();
      }
    }
    public IList<Family> NewFamilies {
      get {
        return FamilyCollection.Filtered((family) => {
          return family.HasNewFont;
        }).ToList();
      }
    }
    public FamilyCollection FamilyCollection { get; private set; }
    #endregion

    #region events
    public event FontInstallationHandler OnFontInstall;
    public event FontUninstallationHandler OnFontUninstall;
    public event FontActivationRequestHandler OnFontActivationRequest;
    public event FontDeactivationRequestHandler OnFontDeactivationRequest;
    #endregion

    #region ctor
    public Storage(IHttpTransport transport, IFontInstaller installer, ICypher cypher) :
      this(transport, installer, cypher, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Fontstore") {
    }

    public Storage(IHttpTransport transport, IFontInstaller installer, ICypher cypher, string rootPath) {
      Installer = installer;
      FamilyCollection = new FamilyCollection();
      RegisterCollectionEvents();

      _HDDStorage = new FSStorage(rootPath);
      _fsAgent = new FSSynchronizationAgent(transport, _HDDStorage);
      _installAgent = new FontInstallerAgent(_HDDStorage, Installer, cypher);

      Loaded = false;
      HasChanged = false;

      _synchronizationCallback = null;
    }
    #endregion

    #region methods
    public Task SaveCredentials(string token) {
      return _HDDStorage.WriteCredential(token);
    }

    public Task<string> LoadCredentials() {
      return _HDDStorage.ReadCredentials();
    }

    public Task CleanCredentials() {
      return _HDDStorage.RemoveCredentials();
    }

    public Task LoadFonts() {
      if (Loaded) {
        return Task.Run(() => { });
      }

      UnregisterCollectionEvents();

      return _HDDStorage.Load(loadedFont => {
        // font loaded, download it if necessary
        Logger.Log("Font loaded, download queued: {0}", loadedFont.UID);
        _fsAgent.QueueDownload(loadedFont, delegate {
          // install the font for the application
          Logger.Log("Font loaded, install queued (process): {0}", loadedFont.UID);
          _installAgent.QueueInstall(loadedFont, InstallationScope.Process, delegate {
            // if the font was activated, install it for the user
            if (loadedFont.Activated) {
              Logger.Log("Font loaded, install queued (user): {0}", loadedFont.UID);
              _installAgent.QueueInstall(loadedFont, InstallationScope.User);
            }
          });
        });
      }).ContinueWith(loadTask => {
        FamilyCollection = loadTask.Result;
        RegisterCollectionEvents();
        Loaded = true;
      }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    public Task SaveFonts() {
      if (!HasChanged) {
        return Task.Run(() => { });
      }

      return _HDDStorage.Save(FamilyCollection).ContinueWith(delegate {
        HasChanged = false;
      }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    public void Clear() {
      UnregisterCollectionEvents();
      FamilyCollection = new FamilyCollection();
      RegisterCollectionEvents();

      _HDDStorage = new FSStorage(_HDDStorage);
      _fsAgent = new FSSynchronizationAgent(_fsAgent);
      _installAgent = new FontInstallerAgent(_installAgent);

      Loaded = false;
      HasChanged = false;

      _synchronizationCallback = null;
    }

    public void ResetNewStatus() {
      foreach (Family family in FamilyCollection.Families) {
        if (family.HasNewFont) {
          foreach (Font font in family.Fonts) {
            font.IsNew = false;
          }
        }
      }
    }

    public Font AddFont(FontDescription description) {
      Font newFont = new Font(
        uid: description.UID,
        familyName: description.FamilyName,
        style: description.Style,
        downloadUrl: description.DownloadUrl,
        sortRank: description.SortRank
      );
      FamilyCollection.AddFont(newFont);
      _HDDStorage.LastCatalogUpdate = description.TransmittedAt;
      return newFont;
    }

    public void RemoveFont(TimestampedFontId fid) {
      FamilyCollection.RemoveFont(fid.UID);
      _HDDStorage.LastCatalogUpdate = fid.TransmittedAt;
    }

    public void ActivateFont(TimestampedFontId fid) {
      Font font = FindFont(fid.UID);
      if (font != null) {
        font.Activated = true;
      }
      _HDDStorage.LastFontStatusUpdate = fid.TransmittedAt;
    }

    public void DeactivateFont(TimestampedFontId fid) {
      Font font = FindFont(fid.UID);
      if (font != null) {
        font.Activated = false;
      }
      _HDDStorage.LastFontStatusUpdate = fid.TransmittedAt;
    }

    public async void DeactivateAllFonts(Action then = null) {
      await Installer.UninstallAllFonts().Then(delegate {
        then?.Invoke();
      });
    }

    public Font FindFont(string uid) {
      return FamilyCollection.FindFont(uid);
    }

    public void SynchronizeWithSystem(Action<int> then = null) {
      _synchronizationCallback = then;
      Synchronization_ExecuteFSCommands();
    }

    public void BeginSynchronization() {
      _fsAgent.StartProcessing();
      _installAgent.StartProcessing();
    }

    public void EndSynchronization() {
      _fsAgent.PauseProcessing();
      _installAgent.PauseProcessing();
    }

    public void AbortSynchronization() {
      _fsAgent.AbortProcessing();
      _installAgent.AbortProcessing();
    }
    #endregion

    #region private methods
    private void RegisterCollectionEvents() {
      FamilyCollection.OnActivationChanged += FamilyCollection_OnActivationChanged;
      FamilyCollection.OnFontAdded += FamilyCollection_OnFontAdded;
      FamilyCollection.OnFontRemoved += FamilyCollection_OnFontRemoved;
      FamilyCollection.OnFontUpdated += FamilyCollection_OnFontUpdated;
      FamilyCollection.OnActivationRequest += FamilyCollection_OnActivationRequest;
      FamilyCollection.OnDeactivationRequest += FamilyCollection_OnDeactivationRequest;
    }

    private void UnregisterCollectionEvents() {
      FamilyCollection.OnActivationChanged -= FamilyCollection_OnActivationChanged;
      FamilyCollection.OnFontAdded -= FamilyCollection_OnFontAdded;
      FamilyCollection.OnFontRemoved -= FamilyCollection_OnFontRemoved;
      FamilyCollection.OnFontUpdated -= FamilyCollection_OnFontUpdated;
      FamilyCollection.OnActivationRequest -= FamilyCollection_OnActivationRequest;
      FamilyCollection.OnDeactivationRequest -= FamilyCollection_OnDeactivationRequest;
    }
    #endregion

    #region synchronization event handling
    private void Synchronization_ExecuteFSCommands() {
      if (_fsAgent.CommandCount > 0) {
        _fsAgent.OnProcessingFinished += Synchronization_OnFSProcessingFinished;
        _fsAgent.StartProcessing();
      } else {
        Synchronization_OnFSProcessingFinished(0);
      }
    }

    private void Synchronization_OnFSProcessingFinished(int processedCommands) {
      _fsAgent.OnProcessingFinished -= Synchronization_OnFSProcessingFinished;
      _fsAgent.PauseProcessing();

      Synchronization_ExecuteAPICommands(processedCommands > 0);
    }

    private void Synchronization_ExecuteAPICommands(bool fsCommandsProcessed) {
      if (_installAgent.CommandCount > 0) {
        _installAgent.OnProcessingFinished += Synchronization_OnAPIProcessingFinished;
        _installAgent.StartProcessing();
      } else if (fsCommandsProcessed) {
        Synchronization_OnAPIProcessingFinished(0);
      } else {
        // no FS commands processed and no API commands to process => no commands can be generated. Synchro is done.
        Synchronization_TriggerSynchroFinished();
      }
    }

    private void Synchronization_OnAPIProcessingFinished(int processedCommands) {
      _installAgent.OnProcessingFinished -= Synchronization_OnAPIProcessingFinished;
      _installAgent.PauseProcessing();

      Synchronization_ExecuteFSCommands();
    }

    private void Synchronization_TriggerSynchroFinished() {
      _synchronizationCallback?.Invoke(_fsAgent.DownloadCount);
      _synchronizationCallback = null;
    }
    #endregion

    #region font event handling
    private void FamilyCollection_OnActivationChanged(FamilyCollection sender, Family fontFamily, Font target) {
      if (target.Activated) {
        _installAgent.QueueInstall(target, InstallationScope.User, result => {
          Logger.Log("Font installed (user): {0} ({1})", target.UID, result);
          if (result != FontAPIResult.Noop)
            OnFontInstall?.Invoke(target, InstallationScope.User, result == FontAPIResult.Success);
        });
      } else {
        _installAgent.QueueUninstall(target, InstallationScope.User, result => {
          Logger.Log("Font uninstalled (user): {0} ({1})", target.UID, result);
          if (result != FontAPIResult.Noop)
            OnFontUninstall?.Invoke(target, InstallationScope.User, result == FontAPIResult.Success);
        });
      }
      HasChanged = true;
    }

    private void FamilyCollection_OnFontUpdated(FamilyCollection sender, Family target, Font removedFont, Font updatedFont) {
      _installAgent.QueueUninstall(removedFont, InstallationScope.User, userScopeResult => {
        Logger.Log("Font uninstalled (user): {0} ({1})", removedFont.UID, userScopeResult);
        if (userScopeResult == FontAPIResult.Failure) {
          OnFontUninstall?.Invoke(removedFont, InstallationScope.User, false);
        }
        else {
          _installAgent.QueueUninstall(removedFont, InstallationScope.Process, processScopeResult => {
            Logger.Log("Font uninstalled (process): {0} ({1})", removedFont.UID, processScopeResult);
            if (processScopeResult == FontAPIResult.Failure) {
              OnFontUninstall?.Invoke(removedFont, InstallationScope.Process, false);
            }
            else {
              _fsAgent.QueueDeletion(removedFont, delegate {
                Logger.Log("Font deleted: {0}", removedFont.UID);
                if (userScopeResult != FontAPIResult.Noop) {
                  OnFontUninstall?.Invoke(removedFont, InstallationScope.User, userScopeResult == FontAPIResult.Success);
                }
                if (processScopeResult != FontAPIResult.Noop) {
                  OnFontUninstall?.Invoke(removedFont, InstallationScope.Process, processScopeResult == FontAPIResult.Success);
                }

                _fsAgent.QueueDownload(updatedFont, delegate {
                  Logger.Log("Font downloaded: {0}", updatedFont.UID);
                  _installAgent.QueueInstall(updatedFont, InstallationScope.Process, result => {
                    Logger.Log("Font installed (process): {0} ({1})", updatedFont.UID, result);
                    if (result != FontAPIResult.Noop)
                      OnFontInstall?.Invoke(updatedFont, InstallationScope.Process, result == FontAPIResult.Success);
                  });
                });
              });
            }
          });
        }
      });
      HasChanged = true;
    }

    private void FamilyCollection_OnFontRemoved(FamilyCollection sender, Family target, Font oldFont) {
      _installAgent.QueueUninstall(oldFont, InstallationScope.User, userScopeResult => {
        Logger.Log("Font uninstalled (user): {0} ({1})", oldFont.UID, userScopeResult);
        if (userScopeResult == FontAPIResult.Failure) {
          OnFontUninstall?.Invoke(oldFont, InstallationScope.User, false);
        } else {
          _installAgent.QueueUninstall(oldFont, InstallationScope.Process, processScopeResult => {
            Logger.Log("Font uninstalled (process): {0} ({1})", oldFont.UID, processScopeResult);
            if (processScopeResult == FontAPIResult.Failure) {
              OnFontUninstall?.Invoke(oldFont, InstallationScope.Process, false);
            } else {
              _fsAgent.QueueDeletion(oldFont, delegate {
                Logger.Log("Font deleted: {0}", oldFont.UID);
                if (userScopeResult != FontAPIResult.Noop) {
                  OnFontUninstall?.Invoke(oldFont, InstallationScope.User, userScopeResult == FontAPIResult.Success);
                }
                if (processScopeResult != FontAPIResult.Noop) {
                  OnFontUninstall?.Invoke(oldFont, InstallationScope.Process, processScopeResult == FontAPIResult.Success);
                }
              });
            }
          });
        }
      });
      HasChanged = true;
    }

    private void FamilyCollection_OnFontAdded(FamilyCollection sender, Family target, Font newFont) {
      _fsAgent.QueueDownload(newFont, delegate {
        Logger.Log("Font downloaded: {0}", newFont.UID);
        _installAgent.QueueInstall(newFont, InstallationScope.Process, result => {
          Logger.Log("Font installed (process): {0} ({1})", newFont.UID, result);
          if (result != FontAPIResult.Noop)
            OnFontInstall?.Invoke(newFont, InstallationScope.Process, result == FontAPIResult.Success);
        });
      });
      HasChanged = true;
    }

    private void FamilyCollection_OnActivationRequest(FamilyCollection sender, Family family, Font target) {
      OnFontActivationRequest?.Invoke(target);
    }

    private void FamilyCollection_OnDeactivationRequest(FamilyCollection sender, Family family, Font target) {
      OnFontDeactivationRequest?.Invoke(target);
    }
    #endregion
  }
}
