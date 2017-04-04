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
    public delegate void FontUpdatedHandler(Family sender, Font removedFont, Font updatedFont);

    public delegate void FontActivationChangedHandler(Family sender, Font target);
    public delegate void FontInstallationChangedHandler(Family sender, Font target);
    public delegate void FullyActivatedChangedHandler(Family sender);
    #endregion

    #region events
    public event FontAddedHandler OnFontAdded;
    public event FontRemovedHandler OnFontRemoved;
    public event FontUpdatedHandler OnFontUpdated;

    public event FontActivationChangedHandler OnActivationChanged;
    public event FontInstallationChangedHandler OnInstallationChanged;
    public event FullyActivatedChangedHandler OnFullyActivatedChanged;
    #endregion

    #region ctor
    public Family(string name, params Font[] fonts): this(name, new List<Font>(fonts)) {
    }

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
          RegisterFontEvents(font);
        }
      }
    }
    #endregion

    #region methods
    public Family Add(Font font) {
      Font removedFont = RemoveAndReturnFont(font.UID);

      RegisterFontEvents(font);
      Fonts.Add(font);

      if (removedFont == null) {
        OnFontAdded?.Invoke(this, font);
      } else {
        OnFontUpdated?.Invoke(this, removedFont, font);
      }

      // will trigger all the correct events to install the font if it was previously installed
      font.Activated = removedFont?.Activated ?? font.Activated;
      UpdateFamilyActivationStatus();
      return this;
    }

    public Family Remove(string uid) {
      Font removedFont = RemoveAndReturnFont(uid);
      if (removedFont != null) {
        OnFontRemoved?.Invoke(this, removedFont);
        UpdateFamilyActivationStatus();
      }
      return this;
    }

    public Font FindFont(string uid) {
      return Fonts.FirstOrDefault(font => font.UID == uid);
    }
    #endregion

    #region private methods
    private Font RemoveAndReturnFont(string uid) {
      Font font = FindFont(uid);
      if (font != null) {
        UnregisterFontEvents(font);
        Fonts.Remove(font);
      }
      return font;
    }

    private void RegisterFontEvents(Font font) {
      font.OnActivationChanged += FontActivationChanged;
      font.OnInstallationChanged += FontInstallationChanged;
    }

    private void UnregisterFontEvents(Font font) {
      font.OnActivationChanged -= FontActivationChanged;
      font.OnInstallationChanged -= FontInstallationChanged;
    }

    private void ToggleFullFamilyActivation(bool newValue) {
      _batchActivation = true;
      foreach (Font font in Fonts) {
        font.Activated = newValue;
      }
      _batchActivation = false;
      UpdateFamilyActivationStatus();
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

    #region event handling
    private void FontInstallationChanged(Font sender) {
      OnInstallationChanged?.Invoke(this, sender);
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
      OnActivationChanged?.Invoke(this, sender);

      // if we are doing mass act/deactivations we don't want to do repetitive work
      if (_batchActivation) return;

      if (FullyActivated && !sender.Activated) { // case 2
        _fullyActivated = false;
        TriggerFullyActivatedEvent();
      }
      else if (!FullyActivated && sender.Activated) { // case 3
        UpdateFamilyActivationStatus();
      }
    }
    #endregion
  }
}
