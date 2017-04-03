using FontInstaller;
using Protocol.Payloads;
using Storage;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Extensions;

namespace TestUtilities.Storage {
  public class MockedStorage : CallTracer, IFontStorage {
    #region properties
    public IFontInstaller Installer { get; private set; }

    public FamilyCollection FamilyCollection { get; private set; }
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
    public bool HasChanged { get; private set; }
    public bool Loaded { get; private set; }
    public DateTime? LastCatalogUpdate { get; set; }
    public DateTime? LastFontStatusUpdate { get; set; }
    #endregion

    #region events
    public event FontInstallationHandler OnFontInstall;
    public event FontUninstallationHandler OnFontUninstall;
    #endregion

    #region ctor
    public MockedStorage(IFontInstaller installer) {
      Installer = installer;
      LastCatalogUpdate = DateTime.Now;
      LastFontStatusUpdate = DateTime.Now;
      FamilyCollection = new FamilyCollection();
      RegisterCollectionEvents();
    }
    #endregion


    #region methods
    public Font AddFont(FontDescription description) {
      RegisterCall("AddFont");
      Font newFont = new Font(description);
      FamilyCollection.AddFont(newFont);
      return newFont;
    }

    public void RemoveFont(string uid) {
      RegisterCall("RemoveFont");
      
      FamilyCollection.RemoveFont(uid);
    }

    public void ActivateFont(string uid) {
      RegisterCall("ActivateFont");
      Font font = FamilyCollection.FindFont(uid);
      if (font != null) {
        font.Activated = true;
      }
    }

    public void DeactivateFont(string uid) {
      RegisterCall("DeactivateFont");
      Font font = FamilyCollection.FindFont(uid);
      if (font != null) {
        font.Activated = false;
      }
    }

    public Font FindFont(string uid) {
      RegisterCall("FindFont");
      return FamilyCollection.FindFont(uid);
    }

    public Task Load() {
      RegisterCall("Load");
      return Task.Run(() => {
        Loaded = true;
      });
    }

    public Task Save() {
      RegisterCall("Save");
      return Task.Run(() => {
        HasChanged = false;
      });
    }

    public void SynchronizeWithSystem(Action then = null) {
      RegisterCall("SynchronizeWithSystem");
      then?.Invoke();
    }

    public void BeginSynchronization() {
      RegisterCall("BeginSynchronization");
    }

    public void EndSynchronization() {
      RegisterCall("EndSynchronization");
    }

    public void AbortSynchronization() {
      RegisterCall("AbortSynchronization");
    }
    #endregion

    #region private methods
    private Family FindFamilyByName(string name) {
      return FamilyCollection.Families.FirstOrDefault(family => family.Name == name);
    }

    private Family FindFamilyByFontUID(string uid) {
      return FamilyCollection.Families.FirstOrDefault(family => family.FindFont(uid) != null);
    }

    private void RegisterCollectionEvents() {
      FamilyCollection.OnActivationChanged += FamilyCollection_OnActivationChanged;
      FamilyCollection.OnFontAdded += FamilyCollection_OnFontAdded;
      FamilyCollection.OnFontRemoved += FamilyCollection_OnFontRemoved;
    }
    #endregion

    #region event handling
    private void FamilyCollection_OnActivationChanged(FamilyCollection sender, Family fontFamily, Font target) {
      if (target.Activated) {
        FontAPIResult result = Installer.InstallFont(target.UID, InstallationScope.User, new MemoryStream()).Result;
        if (result != FontAPIResult.Noop)
          OnFontInstall?.Invoke(target, InstallationScope.User, result == FontAPIResult.Success);
      }
      else {
        FontAPIResult result = Installer.UninstallFont(target.UID, InstallationScope.User).Result;
        if (result != FontAPIResult.Noop)
          OnFontInstall?.Invoke(target, InstallationScope.User, result == FontAPIResult.Success);
      }
      HasChanged = true;
    }

    private void FamilyCollection_OnFontRemoved(FamilyCollection sender, Family target, Font oldFont) {
      FontAPIResult result = Installer.UninstallFont(oldFont.UID, InstallationScope.All).Result;
      if (result != FontAPIResult.Noop)
        OnFontUninstall?.Invoke(oldFont, InstallationScope.All, result == FontAPIResult.Success);
      HasChanged = true;
    }

    private void FamilyCollection_OnFontAdded(FamilyCollection sender, Family target, Font newFont) {
      FontAPIResult result = Installer.InstallFont(newFont.UID, InstallationScope.Process, new MemoryStream()).Result;
      if (result != FontAPIResult.Noop)
        OnFontInstall?.Invoke(newFont, InstallationScope.Process, result == FontAPIResult.Success);
      HasChanged = true;
    }
    #endregion
  }
}
