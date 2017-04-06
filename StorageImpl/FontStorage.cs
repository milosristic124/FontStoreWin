using FontInstaller;
using Protocol.Payloads;
using Protocol.Transport;
using Protocol.Transport.Http;
using Storage.Data;
using Storage.Impl.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;

namespace Storage.Impl {
  public class FontStorage : IFontStorage {
    #region private data
    private FSStorage _HDDStorage;
    private FSSynchronizationAgent _fsAgent;
    private FontInstallerAgent _installAgent;

    private Action _synchronizationCallback;
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

    public DateTime? LastCatalogUpdate {
      get {
        return _HDDStorage.LastCatalogUpdate;
      }
    }
    public DateTime? LastFontStatusUpdate {
      get {
        return _HDDStorage.LastFontStatusUpdate;
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
    #endregion

    #region ctor
    public FontStorage(IHttpTransport transport, IFontInstaller installer) :
      this(transport, installer, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Fontstore") {
    }

    public FontStorage(IHttpTransport transport, IFontInstaller installer, string rootPath) {
      Installer = installer;
      FamilyCollection = new FamilyCollection();
      RegisterCollectionEvents();

      _HDDStorage = new FSStorage(rootPath);
      _fsAgent = new FSSynchronizationAgent(transport, _HDDStorage);
      _installAgent = new FontInstallerAgent(_HDDStorage, Installer);

      Loaded = false;
      HasChanged = false;

      _synchronizationCallback = null;
    }
    #endregion

    #region methods
    public Task Load() {
      if (Loaded) {
        return Task.Run(() => { });
      }

      UnregisterCollectionEvents();

      return Installer.UninstallAllFonts()
        .ContinueWith(t => {
          return _HDDStorage.Load().Result;
        }, TaskContinuationOptions.OnlyOnRanToCompletion)
        .ContinueWith(loadTask => {
          FamilyCollection = loadTask.Result;
          RegisterCollectionEvents();
          Loaded = true;
        }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    public Task Save() {
      if (!HasChanged) {
        return Task.Run(() => { });
      }

      return _HDDStorage.Save(FamilyCollection).ContinueWith(delegate {
        HasChanged = false;
      }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    public Font AddFont(FontDescription description) {
      Font newFont = new Font(
        uid: description.UID,
        familyName: description.FamilyName,
        name: description.Name,
        downloadUrl: description.DownloadUrl,
        timestamp: description.CreatedAt
      );
      FamilyCollection.AddFont(newFont);
      _HDDStorage.LastCatalogUpdate = DateTimeHelper.FromTimestamp(description.TransmittedAt);
      return newFont;
    }

    public void RemoveFont(FontId fid) {
      FamilyCollection.RemoveFont(fid.UID);
      _HDDStorage.LastCatalogUpdate = DateTimeHelper.FromTimestamp(fid.TransmittedAt);
    }

    public void ActivateFont(FontId fid) {
      Font font = FindFont(fid.UID);
      if (font != null) {
        font.Activated = true;
      }
      _HDDStorage.LastFontStatusUpdate = DateTimeHelper.FromTimestamp(fid.TransmittedAt);
    }

    public void DeactivateFont(FontId fid) {
      Font font = FindFont(fid.UID);
      if (font != null) {
        font.Activated = false;
      }
      _HDDStorage.LastFontStatusUpdate = DateTimeHelper.FromTimestamp(fid.TransmittedAt);
    }

    public async void DeactivateAllFonts(Action then = null) {
      await Installer.UninstallAllFonts().Then(delegate {
        then?.Invoke();
      });
    }

    public Font FindFont(string uid) {
      return FamilyCollection.FindFont(uid);
    }

    public void SynchronizeWithSystem(Action then = null) {
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
      _synchronizationCallback?.Invoke();
      _synchronizationCallback = null;
    }
    #endregion

    #region font event handling
    private void FamilyCollection_OnActivationChanged(FamilyCollection sender, Family fontFamily, Font target) {
      if (target.Activated) {
        _installAgent.QueueInstall(target, InstallationScope.User, result => {
          if (result != FontAPIResult.Noop)
            OnFontInstall?.Invoke(target, InstallationScope.User, result == FontAPIResult.Success);
        });
      } else {
        _installAgent.QueueUninstall(target, InstallationScope.User, result => {
          if (result != FontAPIResult.Noop)
            OnFontUninstall?.Invoke(target, InstallationScope.User, result == FontAPIResult.Success);
        });
      }
      HasChanged = true;
    }

    private void FamilyCollection_OnFontUpdated(FamilyCollection sender, Family target, Font removedFont, Font updatedFont) {
      _installAgent.QueueUninstall(removedFont, InstallationScope.User, userScopeResult => {
        if (userScopeResult == FontAPIResult.Failure) {
          OnFontUninstall?.Invoke(removedFont, InstallationScope.User, false);
        }
        else {
          _installAgent.QueueUninstall(removedFont, InstallationScope.Process, processScopeResult => {
            if (processScopeResult == FontAPIResult.Failure) {
              OnFontUninstall?.Invoke(removedFont, InstallationScope.Process, false);
            }
            else {
              _fsAgent.QueueDeletion(removedFont, delegate {
                if (userScopeResult != FontAPIResult.Noop) {
                  OnFontUninstall?.Invoke(removedFont, InstallationScope.User, userScopeResult == FontAPIResult.Success);
                }
                if (processScopeResult != FontAPIResult.Noop) {
                  OnFontUninstall?.Invoke(removedFont, InstallationScope.Process, processScopeResult == FontAPIResult.Success);
                }

                _fsAgent.QueueDownload(updatedFont, delegate {
                  _installAgent.QueueInstall(updatedFont, InstallationScope.Process, result => {
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
        if (userScopeResult == FontAPIResult.Failure) {
          OnFontUninstall?.Invoke(oldFont, InstallationScope.User, false);
        } else {
          _installAgent.QueueUninstall(oldFont, InstallationScope.Process, processScopeResult => {
            if (processScopeResult == FontAPIResult.Failure) {
              OnFontUninstall?.Invoke(oldFont, InstallationScope.Process, false);
            } else {
              _fsAgent.QueueDeletion(oldFont, delegate {
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
        _installAgent.QueueInstall(newFont, InstallationScope.Process, result => {
          if (result != FontAPIResult.Noop)
            OnFontInstall?.Invoke(newFont, InstallationScope.Process, result == FontAPIResult.Success);
        });
      });
      HasChanged = true;
    }
    #endregion

    #region private methods
    private void RegisterCollectionEvents() {
      FamilyCollection.OnActivationChanged += FamilyCollection_OnActivationChanged;
      FamilyCollection.OnFontAdded += FamilyCollection_OnFontAdded;
      FamilyCollection.OnFontRemoved += FamilyCollection_OnFontRemoved;
      FamilyCollection.OnFontUpdated += FamilyCollection_OnFontUpdated;
    }

    private void UnregisterCollectionEvents() {
      FamilyCollection.OnActivationChanged -= FamilyCollection_OnActivationChanged;
      FamilyCollection.OnFontAdded -= FamilyCollection_OnFontAdded;
      FamilyCollection.OnFontRemoved -= FamilyCollection_OnFontRemoved;
      FamilyCollection.OnFontUpdated -= FamilyCollection_OnFontUpdated;
    }
    #endregion
  }
}
