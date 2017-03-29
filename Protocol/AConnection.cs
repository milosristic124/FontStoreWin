using Protocol.Payloads;
using Protocol.Transport;
using Storage;
using System;

namespace Protocol {
  public abstract class AConnection: IConnection {
    #region properties
    public IConnectionTransport Transport { get; private set; }
    public IFontStorage Storage { get; private set; }
    public UserData UserData { get; protected set; }

    public TimeSpan AuthenticationRetryInterval { get; protected set; }
    public TimeSpan ConnectionRetryInterval { get; protected set; }

    public int DownloadParallelism { get; protected set; }
    public TimeSpan DownloadTimeout { get; protected set; }
    #endregion

    #region ctor
    public AConnection(IConnectionTransport transport, IFontStorage storage) {
      Transport = transport;
      Storage = storage;
    }
    #endregion

    #region methods
    public abstract void Connect(string email, string password);
    public abstract void Disconnect(DisconnectReason reason, string error = null);
    public abstract void UpdateCatalog();
    #endregion

    #region IConnectionObservable
    public abstract event ConnectionEstablishedHandler OnEstablished;
    public abstract event ConnectionValidationFailedHandler OnValidationFailure;
    public abstract event CatalogUpdateFinishedHandler OnCatalogUpdateFinished;
    public abstract event ConnectionClosedHandler OnDisconnected;
    #endregion
  }
}
