namespace Utilities {
  public static class Constants {
    public static class Urls {
      #region private constants
      private static readonly string WS = "wss";
      private static readonly string HTTP = "https";
      // staging
      //private static readonly string Host = "api.staging.fontstore.com";
      // prod
      private static readonly string Host = "api.fontstore.com";
      #endregion

      #region constants
      public static readonly string Authentication = $"{HTTP}://{Host}/session/desktop";
      public static readonly string Connection = $"{WS}://{Host}/socket";
      public static readonly string Download = $"{HTTP}://{Host}";
      #endregion
    }

    public static class App {
      #region contants
      public static readonly string OSType = "Win";
      public static readonly string ProtocolVersion = "0.8.1";
      public static readonly string ApplicationVersion = "1.0.0";
      #endregion
    }

    public static class Security {
      #region constants
      // staging
      //public static readonly string FontCypherKey = "lvcypbhupbdmg";
      // prod
      public static readonly string FontCypherKey = "ugpmfjbtlzpdgrut";
      #endregion
    }
  }
}
