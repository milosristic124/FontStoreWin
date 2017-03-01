using Protocol.Payloads;
using System;

namespace Storage.Data {
  public class Font {
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
    public bool IsNew {
      get {
        return Description.IsNew;
      }
    }
    public Uri DownloadUrl {
      get {
        return new Uri(Description.DownloadUrl);
      }
    }

    public bool Activated { get; set; }

    public Font(FontDescription desc) {
      Description = desc;
      Activated = false;
    }

    public Font(string uid, string familyName, string name, Uri downloadUrl, bool isNew): this(new FontDescription {
      UID = uid,
      FamilyName = familyName,
      Name = name,
      DownloadUrl = downloadUrl.AbsoluteUri,
      IsNew = isNew
    }) {}
  }
}
