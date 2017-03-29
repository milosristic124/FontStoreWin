using System;
using System.Collections.Generic;

namespace Protocol.Transport {
  public delegate void TransportClosedHandler();
  public delegate void TransportOpenedHandler();
  public delegate void TransportErrorHandler(Exception exception);

  public interface IHttpTransport {
    IHttpRequest CreateHttpRequest(string endpoint);
  }

  public interface IConnectionTransport: IHttpTransport {
    #region properties
    string EndPoint { get; set; }
    Dictionary<string, string> UrlParams { get; set; }
    string AuthToken { get; set; }
    #endregion

    #region methods
    void Connect();
    void Disconnect(Action callback = null);
    IBroadcastChannel Channel(string name);
    #endregion

    #region events
    event TransportClosedHandler Closed;
    event TransportErrorHandler Error;
    event TransportOpenedHandler Opened;
    #endregion
  }
}
