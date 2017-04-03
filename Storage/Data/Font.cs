using Protocol.Payloads;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Storage.Data {
  public class Font {
    #region private data
    private bool _activated;
    private bool _installed;
    #endregion

    #region properties
    public FontDescription Description { get; private set; }
    public string UID {
      get {
        return Description.UID;
      }
    }
    public string Name {
      get {
        return Description.Name;
      }
    }
    public string FamilyName {
      get {
        return Description.FamilyName;
      }
    }
    public DateTime CreatedAt {
      get {
        DateTime tmp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return tmp.AddMilliseconds(Description.CreatedAt);
      }
    }
    public bool IsNew {
      get {
        return (DateTime.UtcNow - CreatedAt).TotalDays < 5;
      }
    }
    public Uri DownloadUrl {
      get {
        return new Uri(Description.DownloadUrl);
      }
    }
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
    public bool IsInstalled {
      get {
        return _installed;
      }
      set {
        if (_installed != value) {
          _installed = value;
          OnInstallationChanged?.Invoke(this);
        }
      }
    }
    #endregion

    #region delegates
    public delegate void FontActivationEventHandler(Font sender);
    public delegate void FontInstallationEventhandler(Font sender);
    #endregion

    #region events
    public event FontActivationEventHandler OnActivationChanged;
    public event FontInstallationEventhandler OnInstallationChanged;
    #endregion

    #region ctor
    public Font(FontDescription desc) {
      Description = desc;
      _activated = false;
      _installed = false;
    }

    public Font(string uid, string familyName, string name, Uri downloadUrl, int createdAt) : this(new FontDescription {
      UID = uid,
      FamilyName = familyName,
      Name = name,
      DownloadUrl = downloadUrl.AbsoluteUri,
      CreatedAt = createdAt
    }) { }
    #endregion
  }
}
