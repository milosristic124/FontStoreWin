using System;

namespace Protocol.Transport {
  public delegate void TransportClosedHandler();
  public delegate void TransportOpenedHandler();
  public delegate void TransportErrorHandler(Exception exception);

  public interface IConnectionTransport {
    #region properties
    string EndPoint { get; set; }
    string AuthToken { get; set; }
    #endregion

    #region methods
    void Connect();
    void Disconnect(Action callback = null);
    IBroadcastChannel Channel(string name);

    IHttpRequest CreateHttpRequest(string endpoint);
    #endregion

    #region events
    event TransportClosedHandler Closed;
    event TransportErrorHandler Error;
    event TransportOpenedHandler Opened;
    #endregion
  }
}
