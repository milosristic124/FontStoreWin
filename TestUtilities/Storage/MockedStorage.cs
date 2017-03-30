using FontInstaller;
using Protocol.Payloads;
using Storage;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestUtilities.Storage {
  public class MockedStorage : CallTracer, IFontStorage {
    #region properties
    public IFontInstaller Installer { get; set; }

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

    #region ctor
    public MockedStorage() {
      LastCatalogUpdate = DateTime.Now;
      LastFontStatusUpdate = DateTime.Now;
      FamilyCollection = new FamilyCollection();
    }
    #endregion

    #region methods
    public Font AddFont(FontDescription description) {
      RegisterCall("AddFont");
      Font newFont = new Font(description);
      FamilyCollection.AddFont(newFont);

      HasChanged = true;
      return newFont;
    }

    public void RemoveFont(string uid) {
      RegisterCall("RemoveFont");
      FamilyCollection.RemoveFont(uid);
      HasChanged = true;
    }

    public void ActivateFont(string uid) {
      RegisterCall("ActivateFont");
      Font font = FamilyCollection.FindFont(uid);
      if (font != null) {
        font.Activated = true;
        HasChanged = true;
      }
    }

    public void DeactivateFont(string uid) {
      RegisterCall("DeactivateFont");
      Font font = FamilyCollection.FindFont(uid);
      if (font != null) {
        font.Activated = false;
        HasChanged = true;
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
    #endregion
  }
}
