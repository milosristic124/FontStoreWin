using Protocol.Payloads;
using Storage.Data;

namespace Storage.Impl.Tests.Utilities {
  class Factory {
    public static Font CreateFont(FontDescription desc) {
      return new Font(
        uid: desc.UID,
        familyName: desc.FamilyName,
        name: desc.Name,
        downloadUrl: desc.DownloadUrl
      );
    }
  }
}
