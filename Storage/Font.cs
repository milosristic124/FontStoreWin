using System;

namespace Storage {
  public class Font
  {
    public string Uid { get; private set; }
    public string Name { get; private set; }
    public string FamilyName { get; private set; }
    public bool IsNew { get; private set; }

    public Uri DownloadUrl { get; private set; }
    public Uri FileUri { get; private set; }

    public Font(string uid, string name, string familyName, bool isNew, Uri downloadUrl)
    {
      this.Uid = uid;
      this.Name = name;
      this.FamilyName = familyName;
      this.IsNew = isNew;
      this.DownloadUrl = downloadUrl;
      this.FileUri = null;
    }
  }
}
