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

    public bool Activated { get; set; }

    public Font(FontDescription desc) {
      Description = desc;
      Activated = false;
    }

    public Font(string uid, string familyName, string name, Uri downloadUrl, int createdAt): this(new FontDescription {
      UID = uid,
      FamilyName = familyName,
      Name = name,
      DownloadUrl = downloadUrl.AbsoluteUri,
      CreatedAt = createdAt
    }) {}
  }
}
