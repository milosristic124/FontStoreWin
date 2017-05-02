namespace Utilities {
  public static class Constants {
    public static class Urls {
      #region private constants
#if DEBUG
      private static readonly string WS = "ws";
      private static readonly string HTTP = "http";
      private static readonly string Host = "192.168.44.99:3000";
      //private static readonly string Host = "api.staging.fontstore.com";
#else
    private static readonly string WS = "wss://";
    private static readonly string HTTP = "https://";
    private static readonly string Host = "app.fontstore.com";
#endif
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
      public static readonly string FontCypherKey = "secret";
      #endregion
    }
  }
}
