using System;
using System.Collections.Generic;

namespace Protocol.Transport {
  public abstract class AConnectionTransport : IConnectionTransport {
    #region properties
    public string EndPoint { get; set; }
    public string AuthToken { get; set; }
    public Dictionary<string, string> UrlParams { get; set; }
    #endregion

    #region ctor
    public AConnectionTransport() {
      EndPoint = null;
      AuthToken = null;
      UrlParams = new Dictionary<string, string>();
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
