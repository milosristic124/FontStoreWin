namespace Utilities {
  public static class Urls {
    #region private constants
#if DEBUG
    private static readonly string WS = "ws";
    private static readonly string HTTP = "http";
    private static readonly string Host = "localhost:3000";
#else
    private static readonly string WS = "wss://";
    private static readonly string HTTP = "https://";
    private static readonly string HostRoot = "app.fontstore.com";
#endif
    #endregion

    #region constants
    public static readonly string Authentication = $"{HTTP}://{Host}/session/desktop";
    public static readonly string Connection = $"{WS}://{Host}/socket";
    public static readonly string Download = $"{HTTP}://{Host}";
    #endregion
  }
}
