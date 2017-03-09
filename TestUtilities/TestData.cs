using Newtonsoft.Json;
using Protocol.Payloads;

namespace TestUtilities {
  public class TestData {
    public static string Serialize(object obj) {
      return JsonConvert.SerializeObject(obj);
    }
    public static T Deserialize<T>(string json) {
      return JsonConvert.DeserializeObject<T>(json);
    }

    public static string AuthenticationErrorReason = "authentication error";

    public static UserData UserData = new UserData {
      UID = "test_user_uid",
      FirstName = "Name",
      LastName = "LastName",
      AuthToken = "test_reuse_token",

      Urls = new UserUrls {
        Account = "http://localhost/test_uid/account",
        Settings = "http://localhost/test_uid/settings",
        Visit = "http://localhost"
      }
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
