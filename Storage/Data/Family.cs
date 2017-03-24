using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Storage.Data {
  public class Family {
    #region private data
    private bool _fullyActivated;
    private bool _batchActivation;
    #endregion

    #region properties
    public string Name { get; private set; }
    public IList<Font> Fonts { get; private set; }

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

    public bool FullyActivated {
      get {
        return _fullyActivated;
      }
      set {
        ToggleFullFamilyActivation(value);
      }
    }
    #endregion

    #region delegates
    public delegate void FontAddedHandler(Family sender, Font newFont);
    public delegate void FontRemovedHandler(Family sender, Font removedFont);
    public delegate void FontActivationChangedHandler(Family sender, Font target);
    public delegate void FullyActivatedChangedHandler(Family sender);
    #endregion

    #region events
    public event FontAddedHandler OnFontAdded;
    public event FontRemovedHandler OnFontRemoved;
    public event FontActivationChangedHandler OnFontActivationChanged;
    public event FullyActivatedChangedHandler OnFullyActivatedChanged;
    #endregion

    #region ctor
    public Family(string name, IList<Font> fonts = null) {
      Name = name;
      _batchActivation = false;
      _fullyActivated = false;

      if (fonts == null) {
        Fonts = new List<Font>();
      } else {
        Fonts = fonts;
        UpdateFamilyActivationStatus(false);

        foreach (Font font in Fonts) {
          font.OnActivationChanged += FontActivationChanged;
        }
      }
    }
    #endregion

    #region methods
    public void Add(Font font) {
      Font removedFont = RemoveAndReturnFont(font.UID);
      if (removedFont != null) {
        OnFontRemoved?.Invoke(this, removedFont);
      }

      font.OnActivationChanged += FontActivationChanged;
      Fonts.Add(font);
      OnFontAdded?.Invoke(this, font);

      UpdateFamilyActivationStatus();
    }

    public void Remove(string uid) {
      Font removedFont = RemoveAndReturnFont(uid);
      if (removedFont != null) {
        OnFontRemoved?.Invoke(this, removedFont);
        UpdateFamilyActivationStatus();
      }
    }

    public Font FindFont(string uid) {
      return Fonts.FirstOrDefault(font => font.UID == uid);
    }
    #endregion

    #region private methods
    private Font RemoveAndReturnFont(string uid) {
      Font font = FindFont(uid);
      if (font != null) {
        font.OnActivationChanged -= FontActivationChanged;
        Fonts.Remove(font);
      }
      return font;
    }

    private void ToggleFullFamilyActivation(bool newValue) {
      _batchActivation = true;
      foreach (Font font in Fonts) {
        font.Activated = newValue;
      }
      _batchActivation = false;
      UpdateFamilyActivationStatus();
    }

    private void FontActivationChanged(Font sender) {
      // there is only 4 cases:
      // 1. family fully activated & font activated
      // 2. family fully activated & font deactivated
      // 3. family not fully activated & font activated
      // 4. family not fully activated & font deactivate
      // the reactions are:
      // 1. not supposed to happen
      // 2. family is no longer fully activated
      // 3. family might become fully activated, check it.
      // 4. don't care, nothing change

      // whatever happens we transmit the font event
      OnFontActivationChanged?.Invoke(this, sender);

      // if we are doing mass act/deactivations we don't want to do repetitive work
      if (_batchActivation) return;

      if (FullyActivated && !sender.Activated) { // case 2
        _fullyActivated = false;
        TriggerFullyActivatedEvent();
      } else if (!FullyActivated && sender.Activated) { // case 3
        UpdateFamilyActivationStatus();
      }
    }

    private void UpdateFamilyActivationStatus(bool trigger = true) {
      bool prevValue = _fullyActivated;
      _fullyActivated = !Fonts.Any(font => !font.Activated);
      if (trigger && _fullyActivated != prevValue) {
        TriggerFullyActivatedEvent();
      }
    }

    private void TriggerFullyActivatedEvent() {
      OnFullyActivatedChanged?.Invoke(this);
    }
    #endregion
  }
}
