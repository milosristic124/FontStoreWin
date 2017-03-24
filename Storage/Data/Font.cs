using Protocol.Payloads;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Storage.Data {
  public class Font: INotifyPropertyChanged {
    #region private data
    private bool _activated;
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
    #endregion

    #region obeservable properties
    public bool Activated {
      get {
        return _activated;
      }
      set {
        if (value != _activated) {
          _activated = value;
          NotifyPropertyChanged();
        }
      }
    }
    #endregion

    #region events
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region ctor
    public Font(FontDescription desc) {
      Description = desc;
      Activated = false;
    }

    public Font(string uid, string familyName, string name, Uri downloadUrl, int createdAt) : this(new FontDescription {
      UID = uid,
      FamilyName = familyName,
      Name = name,
      DownloadUrl = downloadUrl.AbsoluteUri,
      CreatedAt = createdAt
    }) { }
    #endregion

    #region private methods
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
      Console.WriteLine("[{0}] Property changed: {1}", Name, propertyName);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
  }
}
