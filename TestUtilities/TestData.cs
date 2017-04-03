using Newtonsoft.Json;
using Protocol.Payloads;
using System;

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
      DownloadUrl = "http://localhost/downloads/font/test_font_uid",
      CreatedAt = TimeStamp_Now()
    };

    public static FontDescription Font1_Description2 = new FontDescription {
      UID = "test_font_uid",
      FamilyName = "TestFamilyName",
      Name = "TestFontName2",
      DownloadUrl = "http://localhost/downloads/font/test_font_uid",
      CreatedAt = TimeStamp_MinusDays(10)
    };

    public static FontId Font1_Id = new FontId {
      UID = Font1_Description.UID
    };

    public static FontDescription Font3_Description = new FontDescription {
      UID = "test_font_uid_3",
      FamilyName = "TestFamilyName",
      Name = "TestFontName3",
      DownloadUrl = "http://localhost/downloads/font/test_font_uid_3",
      CreatedAt = TimeStamp_MinusDays(10)
    };

    public static FontId Font3_Id = new FontId {
      UID = Font3_Description.UID
    };


    public static FontDescription Font2_Description = new FontDescription {
      UID = "test_font_uid_2",
      FamilyName = "TestFamilyName_2",
      Name = "TestFontName_2",
      DownloadUrl = "http://localhost/downloads/font/test_font_uid_2",
      CreatedAt = TimeStamp_Now()
    };

    public static FontId Font2_Id = new FontId {
      UID = Font2_Description.UID
    };

    private static int TimeStamp_Now() {
      return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    }

    private static int TimeStamp_MinusDays(int days) {
      return TimeStamp_Now() - (int)(new TimeSpan(days, 0, 0, 0).TotalSeconds);
    }
  }
}
