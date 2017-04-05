using System;

namespace Utilities {
  public static class DateTimeHelper {
    public static DateTime FromTimestamp(int timestamp) {
      return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestamp);
    }

    public static int ToTimestamp(this DateTime self) {
      return (int)self.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
  }
}
