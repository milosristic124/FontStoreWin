using Protocol.Payloads;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Utilities;

namespace Storage.Data {
  public class Font {
    #region private data
    private bool _activated;
    #endregion

    #region properties
    public string UID { get; private set; }
    public string Name { get; private set; }
    public string FamilyName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsNew {
      get {
        return (DateTime.UtcNow - CreatedAt).TotalDays < 5;
      }
    }
    public Uri DownloadUrl { get; private set; }
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
    #endregion

    #region delegates
    public delegate void FontActivationEventHandler(Font sender);
    public delegate void FontActivationRequestedHandler(Font sender);
    public delegate void FontDeactivationRequestedHandler(Font sender);
    #endregion

    #region events
    public event FontActivationEventHandler OnActivationChanged;
    public event FontActivationRequestedHandler OnActivationRequest;
    public event FontDeactivationRequestedHandler OnDeactivationRequest;
    #endregion

    #region ctor
    public Font(string uid, string familyName, string name, string downloadUrl, int timestamp) {
      UID = uid;
      FamilyName = familyName;
      Name = name;
      DownloadUrl = new Uri(downloadUrl);
      CreatedAt = DateTimeHelper.FromTimestamp(timestamp);
      _activated = false;
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
