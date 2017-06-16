﻿using Protocol.Payloads;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Utilities;

namespace Storage.Data {
  public class Font {
    #region private data
    private bool _activated;
    private bool _isNew;
    #endregion

    #region properties
    public string UID { get; private set; }
    public string Style { get; private set; }
    public string FamilyName { get; private set; }
    public bool IsNew {
      get {
        return _isNew;
      }
      set {
        if (_isNew != value) {
          _isNew = value;
          OnNewChanged?.Invoke(this);
        }
      }
    }
    public Uri DownloadUrl { get; private set; }
    public Uri PreviewUrl { get; private set; }
    public bool Activated {
      get {
        return _activated;
      }
      set {
        if (_activated != value) {
          _activated = value;
          OnActivationChanged?.Invoke(this);
        }
      }
    }
    public int SortRank { get; private set; }

    public string PreviewPath { get; set; }
    #endregion

    #region delegates
    public delegate void FontActivationEventHandler(Font sender);
    public delegate void FontNewEventHandler(Font sender);
    public delegate void FontActivationRequestedHandler(Font sender);
    public delegate void FontDeactivationRequestedHandler(Font sender);
    #endregion

    #region events
    public event FontActivationEventHandler OnActivationChanged;
    public event FontNewEventHandler OnNewChanged;
    public event FontActivationRequestedHandler OnActivationRequest;
    public event FontDeactivationRequestedHandler OnDeactivationRequest;
    #endregion

    #region ctor
    public Font(string uid, string familyName, string style, int sortRank, string downloadUrl, string previewUrl) {
      UID = uid;
      FamilyName = familyName;
      Style = style;
      DownloadUrl = new Uri(downloadUrl);
      PreviewUrl = new Uri(previewUrl);
      _activated = false;
      _isNew = true;
      SortRank = sortRank;
    }
    #endregion

    #region methods
    public void RequestDeactivation() {
      OnDeactivationRequest?.Invoke(this);
    }

    public void RequestActivation() {
      OnActivationRequest?.Invoke(this);
    }
    #endregion
  }
}
