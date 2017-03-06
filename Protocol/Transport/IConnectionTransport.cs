using System;

namespace Protocol.Transport {
  public delegate void TransportClosedHandler();
  public delegate void TransportOpenedHandler();
  public delegate void TransportErrorHandler(Exception exception);

  public interface IConnectionTransport {
    event TransportClosedHandler Closed;
    event TransportErrorHandler Error;
    event TransportOpenedHandler Opened;

    void Connect();
    void Disconnect(Action callback = null);

    IBroadcastChannel Channel(string name);
  }
}
