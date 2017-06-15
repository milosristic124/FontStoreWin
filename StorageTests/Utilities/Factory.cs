using Protocol.Payloads;
using Storage.Data;

namespace Storage.Impl.Tests.Utilities {
  class Factory {
    public static Font CreateFont(FontDescription desc) {
      return new Font(
        uid: desc.UID,
        familyName: desc.FamilyName,
        style: desc.Style,
        downloadUrl: desc.DownloadUrl,
        previewUrl: desc.PreviewUrl,
        sortRank: 0
      );
    }
  }
}
