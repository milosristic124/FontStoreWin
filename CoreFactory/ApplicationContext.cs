using FontInstaller;
using Protocol;
using Protocol.Transport;
using Protocol.Transport.Http;
using Storage;

namespace Core {
  public class ApplicationContext {
    #region properties
    public IFontInstaller FontInstaller { get; private set; }
    public IHttpTransport HttpTransport { get; private set; }
    public IConnectionTransport Transport { get; private set; }
    public IStorage Storage { get; private set; }
    public IConnection Connection { get; private set; }
    #endregion

    #region ctor
    internal ApplicationContext(IConnection connection) {
      Connection = connection;
      Transport = Connection.Transport;
      HttpTransport = Connection.HttpTransport;
      Storage = Connection.Storage;
      FontInstaller = Storage.Installer;
    }
    #endregion
  }
}
