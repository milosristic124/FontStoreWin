using Protocol;
using Protocol.Payloads;

namespace TestUtilities {
  public class TestData {
    public static UserData UserData = new UserData {
      UID = "test_user_uid",
      Name = "Name",
      LastName = "LastName",
      SettingsUrl = "http://localhost/test_uid/settings",
      AccountUrl = "http://localhost/test_uid/account",
      ReuseToken = "test_reuse_token"
    };

    public static FontDescription FontDescription1 = new FontDescription {
      UID = "test_font_uid",
      FamilyName = "TestFamilyName",
      Name = "TestFontName",
      DownloadUrl = "http://localhost/font/test_font_uid/download",
      IsNew = true
    };

    public static FontDescription FontDescription1_2 = new FontDescription {
      UID = "test_font_uid",
      FamilyName = "TestFamilyName",
      Name = "TestFontName2",
      DownloadUrl = "http://localhost/font/test_font_uid/download",
      IsNew = false
    };

    public static FontDescription FontDescription2 = new FontDescription {
      UID = "test_font_uid_2",
      FamilyName = "TestFamilyName_2",
      Name = "TestFontName_2",
      DownloadUrl = "http://localhost/font/test_font_uid_2/download",
      IsNew = true
    };
  }
}
