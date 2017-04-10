using Protocol;
using Protocol.Transport;
using Storage;
using FontInstaller;
using Protocol.Transport.Http;
using Encryption;
using Encryption.Impl;
using Utilities;

namespace Core {
  public static class Factory {
    public static ApplicationContext InitializeApplicationContext() {
      IConnectionTransport transport = new Protocol.Transport.Phoenix.ConnectionTransport();
      IHttpTransport http = new Protocol.Transport.Http.Impl.HttpTransport();
      IFontInstaller installer = new FontInstaller.Impl.FontInstaller();
      ICypher cypher = new XORCypher(Constants.Security.FontCypherKey);
      IStorage storage = new Storage.Impl.Storage(http, installer, cypher);
      IConnection connection = new Protocol.Impl.Connection(transport, http, storage);

      return new ApplicationContext(connection);
    }
  }
}
