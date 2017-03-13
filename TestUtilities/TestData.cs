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

    public static FontDescription Font1_Description = new FontDescription {
      UID = "test_font_uid",
      FamilyName = "TestFamilyName",
      Name = "TestFontName",
      DownloadUrl = "http://localhost/font/test_font_uid/download",
      IsNew = true
    };

    public static FontDescription Font1_Description2 = new FontDescription {
      UID = "test_font_uid",
      FamilyName = "TestFamilyName",
      Name = "TestFontName2",
      DownloadUrl = "http://localhost/font/test_font_uid/download",
      IsNew = false
    };

    public static FontId Font1_Id = new FontId {
      UID = Font1_Description.UID
    };


    public static FontDescription Font2_Description = new FontDescription {
      UID = "test_font_uid_2",
      FamilyName = "TestFamilyName_2",
      Name = "TestFontName_2",
      DownloadUrl = "http://localhost/font/test_font_uid_2/download",
      IsNew = true
    };

    public static FontId Font2_Id = new FontId {
      UID = Font2_Description.UID
    };
  }
}
