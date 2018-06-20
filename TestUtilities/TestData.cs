using Newtonsoft.Json;
using Protocol.Payloads;
using System;

namespace TestUtilities {
  public class TestData {
    public static string Serialize(object obj) {
      return JsonConvert.SerializeObject(obj);
    }

    private static int TimeStamp_Now() {
      return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    }

    private static int TimeStamp_MinusDays(int days) {
      return TimeStamp_Now() - (int)(new TimeSpan(days, 0, 0, 0).TotalSeconds);
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

    public static Disconnect DisconnectReason = new Disconnect {
      Reason = "I don't like you"
    };

    public static FontDescription Font1_Description = new FontDescription {
      UID = "test_font_uid",
      FamilyName = "TestFamilyName",
      Style = "TestFontName",
      DownloadUrl = "http://localhost/downloads/font/test_font_uid",
      TransmittedAt = 0
    };

    public static FontDescription Font1_Description2 = new FontDescription {
      UID = "test_font_uid",
      FamilyName = "TestFamilyName",
      Style = "TestFontName2",
      DownloadUrl = "http://localhost/downloads/font/test_font_uid",
      TransmittedAt = 0
    };

    public static TimestampedFontId Font1_Id = new TimestampedFontId {
      UID = Font1_Description.UID,
      TransmittedAt = 0
    };

    public static FontDescription Font3_Description = new FontDescription {
      UID = "test_font_uid_3",
      FamilyName = "TestFamilyName",
      Style = "TestFontName3",
      DownloadUrl = "http://localhost/downloads/font/test_font_uid_3",
      TransmittedAt = 0
    };

    public static TimestampedFontId Font3_Id = new TimestampedFontId {
      UID = Font3_Description.UID,
      TransmittedAt = 0
    };


    public static FontDescription Font2_Description = new FontDescription {
      UID = "test_font_uid_2",
      FamilyName = "TestFamilyName_2",
      Style = "TestFontName_2",
      DownloadUrl = "http://localhost/downloads/font/test_font_uid_2",
      TransmittedAt = 0
    };

    public static TimestampedFontId Font2_Id = new TimestampedFontId {
      UID = Font2_Description.UID,
      TransmittedAt = 0
    };
  }
}
