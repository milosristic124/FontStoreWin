using System;
using System.Collections.Generic;
using System.Linq;

namespace Storage.Data {
  public class FamilyCollection {
    #region properties
    public IList<Family> Families { get; private set; }
    #endregion

    #region delegates
    public delegate void CollectionClearedHandler(FamilyCollection sender);

    public delegate void FamilyAddedHandler(FamilyCollection sender, Family newFamily);
    public delegate void FamilyRemovedHandler(FamilyCollection sender, Family removedFamily);

    public delegate void FontAddedHandler(FamilyCollection sender, Family target, Font newFont);
    public delegate void FontRemovedHandler(FamilyCollection sender, Family target, Font oldFont);
    public delegate void FontUpdatedHandler(FamilyCollection sender, Family target, Font removedFont, Font updatedFont);

    public delegate void FontActivationChangedHandler(FamilyCollection sender, Family fontFamily, Font target);
    public delegate void FontInstallationChangedHandler(FamilyCollection sender, Family fontFamily, Font target);
    #endregion

    #region events
    public event CollectionClearedHandler OnCollectionCleared;
    public event FamilyAddedHandler OnFamilyAdded;
    public event FamilyRemovedHandler OnFamilyRemoved;
    public event FontAddedHandler OnFontAdded;
    public event FontRemovedHandler OnFontRemoved;
    public event FontUpdatedHandler OnFontUpdated;
    public event FontActivationChangedHandler OnActivationChanged;
    public event FontInstallationChangedHandler OnInstallationChanged;
    #endregion

    #region ctor
    public FamilyCollection() {
      Families = new List<Family>();
    }
    #endregion

    #region methods
    public FamilyCollection Clear() {
      foreach (Family family in Families) {
        UnregisterFamilyEvents(family);
      }
      Families.Clear();
      OnCollectionCleared?.Invoke(this);
      return this;
    }

    public FamilyCollection AddFont(Font newFont) {
      Family family = FindFamilyByName(newFont.FamilyName);
      if (family != null) {
        family.Add(newFont);
      } else {
        family = new Family(newFont.FamilyName);
        RegisterFamilyEvents(family);

        Families.Add(family);
        family.Add(newFont); // adding fonts here ensure the triggering of font added events
        OnFamilyAdded?.Invoke(this, family);
      }
      return this;
    }

    public FamilyCollection RemoveFont(string uid) {
      Family family = FindFamilyByFontUID(uid);
      if (family != null) {
        family.Remove(uid);
        if (family.Fonts.Count <= 0) {
          UnregisterFamilyEvents(family);
          Families.Remove(family);
          OnFamilyRemoved?.Invoke(this, family);
        }
      }
      return this;
    }

    public IEnumerable<Family> Filtered(Func<Family, bool> predicate) {
      return Families.Where(family => {
        return predicate(family);
      });
    }

    public Font FindFont(string uid) {
      return FindFamilyByFontUID(uid)?.FindFont(uid);
    }
    #endregion

    #region private methods
    private Family FindFamilyByFontUID(string uid) {
      return Families.FirstOrDefault(family => family.FindFont(uid) != null);
    }

    private Family FindFamilyByName(string name) {
      return Families.FirstOrDefault(family => family.Name == name);
    }

    private void RegisterFamilyEvents(Family family) {
      family.OnFontAdded += Family_OnFontAdded;
      family.OnFontRemoved += Family_OnFontRemoved;
      family.OnFontUpdated += Family_OnFontUpdated;
      family.OnActivationChanged += Family_OnFontActivationChanged;
      family.OnInstallationChanged += Family_OnFontInstallationChanged;
    }

    private void UnregisterFamilyEvents(Family family) {
      family.OnFontAdded -= Family_OnFontAdded;
      family.OnFontRemoved -= Family_OnFontRemoved;
      family.OnFontUpdated -= Family_OnFontUpdated;
      family.OnActivationChanged -= Family_OnFontActivationChanged;
      family.OnInstallationChanged -= Family_OnFontInstallationChanged;
    }
    #endregion

    #region event handling
    private void Family_OnFontUpdated(Family sender, Font removedFont, Font updatedFont) {
      OnFontUpdated?.Invoke(this, sender, removedFont, updatedFont);
    }

    private void Family_OnFontInstallationChanged(Family sender, Font target) {
      OnInstallationChanged?.Invoke(this, sender, target);
    }

    private void Family_OnFontActivationChanged(Family sender, Font target) {
      OnActivationChanged?.Invoke(this, sender, target);
    }

    private void Family_OnFontAdded(Family sender, Font newFont) {
      OnFontAdded?.Invoke(this, sender, newFont);
    }

    private void Family_OnFontRemoved(Family sender, Font removedFont) {
      OnFontRemoved?.Invoke(this, sender, removedFont);
    }
    #endregion
  }
}
