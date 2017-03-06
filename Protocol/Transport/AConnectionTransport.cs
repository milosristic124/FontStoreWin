using System;

namespace Protocol.Transport {
  public abstract class AConnectionTransport: IConnectionTransport {
    #region properties
    public string AuthToken { get; set; }
    public string EndPoint { get; protected set; }
    #endregion

    #region ctor
    public AConnectionTransport(string endpoint) {
      EndPoint = endpoint;
      AuthToken = null;
    }
    #endregion

    #region events
    public abstract event TransportClosedHandler Closed;
    public abstract event TransportErrorHandler Error;
    public abstract event TransportOpenedHandler Opened;
    #endregion

    #region methods
    public abstract void Connect();
    public abstract void Disconnect(Action callback = null);

    public abstract IBroadcastChannel Channel(string name);
    #endregion
  }
}
