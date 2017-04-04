using Protocol;
using Protocol.Transport;
using Storage;
using FontInstaller;
using Protocol.Transport.Http;

namespace Core {
  public static class Factory {
    public static ApplicationContext InitializeApplicationContext() {
      IConnectionTransport transport = new Protocol.Transport.Phoenix.ConnectionTransport();
      IHttpTransport http = new Protocol.Transport.Http.Impl.HttpTransport();
      IFontInstaller installer = new FontInstaller.Impl.FontInstaller();
      IFontStorage storage = new Storage.Impl.FontStorage(http, installer);
      IConnection connection = new Protocol.Impl.Connection(transport, http, storage);

      return new ApplicationContext(connection);
    }
  }
}
