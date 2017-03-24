using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Storage.Data {
  public class Family: INotifyPropertyChanged {
    #region private data
    private bool _fullyActivated;
    #endregion

    #region properties
    public string Name { get; private set; }
    public bool HasNewFont {
      get {
        return Fonts.Any(font => font.IsNew);
      }
    }
    public bool HasActivatedFont {
      get {
        return Fonts.Any(font => font.Activated);
      }
    }
    #endregion

    #region observable properties
    public ObservableCollection<Font> Fonts { get; private set; }

    public bool FullyActivated {
      get {
        return _fullyActivated;
      }
      set {
        foreach (Font font in Fonts) {
          font.Activated = value;
        }
      }
    }
    #endregion

    #region events
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region ctor
    public Family(string name, List<Font> fonts = null) {
      Name = name;
      _fullyActivated = false;

      if (fonts == null) {
        Fonts = new ObservableCollection<Font>();
      } else {
        Fonts = new ObservableCollection<Font>(fonts);
        _fullyActivated = !Fonts.Any(font => !font.Activated);

        foreach (Font font in Fonts) {
          font.PropertyChanged += Font_PropertyChanged;
        }
      }
    }
    #endregion

    #region methods
    public void Add(Font font) {
      Remove(font.UID);

      font.PropertyChanged += Font_PropertyChanged;
      Fonts.Add(font);
      UpdateFamilyActivationStatus();
    }

    public void Remove(string uid) {
      Font font = FindFont(uid);
      if (font != null) {
        font.PropertyChanged -= Font_PropertyChanged;
        Fonts.Remove(font);
        UpdateFamilyActivationStatus();
      }
    }

    public Font FindFont(string uid) {
      return Fonts.FirstOrDefault(font => font.UID == uid);
    }
    #endregion

    #region private methods
    private void Font_PropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == "Activated") {
        UpdateFamilyActivationStatus();
      }
    }

    private void UpdateFamilyActivationStatus() {
      bool prevValue = _fullyActivated;
      _fullyActivated = !Fonts.Any(font => !font.Activated);
      if (_fullyActivated != prevValue)
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FullyActivated"));
    }
    #endregion
  }
}
