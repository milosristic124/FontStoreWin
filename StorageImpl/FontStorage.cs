using FontInstaller;
using Protocol.Payloads;
using Protocol.Transport;
using Storage.Data;
using Storage.Impl.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public DateTime? LastCatalogUpdate {
      get {
        return _HDDStorage.LastCatalogUpdate;
      }
      set {
        _HDDStorage.LastCatalogUpdate = value;
      }
    }
    public DateTime? LastFontStatusUpdate {
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
      return _HDDStorage.Load().ContinueWith(loadTask => {
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
      Font newFont = new Font(description);
      FamilyCollection.AddFont(newFont);
      return newFont;
    }

    public void RemoveFont(string uid) {
      FamilyCollection.RemoveFont(uid);
    }

    public void ActivateFont(string uid) {
      Font font = FindFont(uid);
      if (font != null) {
        font.Activated = true;
      }
    }

    public void DeactivateFont(string uid) {
      Font font = FindFont(uid);
      if (font != null) {
        font.Activated = false;
      }
    }

    public Font FindFont(string uid) {
      return FamilyCollection.FindFont(uid);
    }

    public void SynchronizeWithSystem(Action then = null) {
      _synchronizationCallback = then;

      _fsAgent.OnDownloadsFinished += Synchronization_DownloadsFinished;
      _fsAgent.BeginDownloads();
    }

    public void BeginSynchronization() {
      _fsAgent.BeginDownloads();
      _installAgent.BeginInstalls();
    }

    public void EndSynchronization() {
      _fsAgent.PauseDownloads();
      _installAgent.PauseInstalls();
    }

    public void AbortSynchronization() {
      _fsAgent.AbortDownloads();
      _installAgent.AbortInstalls();
    }

    #endregion

    #region synchronization event handling
    private void Synchronization_DownloadsFinished() {
      _fsAgent.PauseDownloads();
      _fsAgent.OnDownloadsFinished -= Synchronization_DownloadsFinished;

      _installAgent.OnInstallsFinished += Synchronization_InstallsFinished;
      _installAgent.BeginInstalls();
    }

    private void Synchronization_InstallsFinished() {
      _installAgent.PauseInstalls();
      _installAgent.OnInstallsFinished -= Synchronization_InstallsFinished;

      _synchronizationCallback?.Invoke();
    }
    #endregion

    #region font event handling
    private void FamilyCollection_OnActivationChanged(FamilyCollection sender, Family fontFamily, Font target) {
      if (target.Activated) {
        _installAgent.QueueInstall(target, InstallationScope.User);
      } else {
        _installAgent.QueueUninstall(target, InstallationScope.User);
      }
      HasChanged = true;
    }

    private void FamilyCollection_OnFontRemoved(FamilyCollection sender, Family target, Font oldFont) {
      if (oldFont.Activated) {
        _installAgent.QueueUninstall(oldFont, InstallationScope.All, delegate {
          _fsAgent.QueueDeletion(oldFont);
        });
      } else {
        _fsAgent.QueueDeletion(oldFont);
      }
      HasChanged = true;
    }

    private void FamilyCollection_OnFontAdded(FamilyCollection sender, Family target, Font newFont) {
      _fsAgent.QueueDownload(newFont, delegate {
        _installAgent.QueueInstall(newFont, InstallationScope.Process);
      });
      HasChanged = true;
    }
    #endregion

    #region private methods
    private void RegisterCollectionEvents() {
      FamilyCollection.OnActivationChanged += FamilyCollection_OnActivationChanged;
      FamilyCollection.OnFontAdded += FamilyCollection_OnFontAdded;
      FamilyCollection.OnFontRemoved += FamilyCollection_OnFontRemoved;
    }

    private void UnregisterCollectionEvents() {
      FamilyCollection.OnActivationChanged -= FamilyCollection_OnActivationChanged;
      FamilyCollection.OnFontAdded -= FamilyCollection_OnFontAdded;
      FamilyCollection.OnFontRemoved -= FamilyCollection_OnFontRemoved;
    }
    #endregion
  }
}
