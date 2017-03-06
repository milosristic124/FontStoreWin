using PhoenixSocket;
using SuperSocket.ClientEngine;
using System;

namespace Protocol.Transport.Phoenix {
  public class ConnectionTransport : AConnectionTransport {
    #region private data
    private Socket _socket;
    #endregion

    #region ctor
    public ConnectionTransport(string endpoint): base(endpoint) {
      _socket = new Socket(EndPoint);
      _socket.Opened += _socket_Opened;
      _socket.Closed += _socket_Closed;
      _socket.Error += _socket_Error;
    }
    #endregion

    #region methods
    public override void Connect() {
      _socket.Connect();
    }

    public override void Disconnect(Action callback = null) {
      if (_socket != null) {
        _socket.Disconnect(() => {
          _socket = null;
          callback?.Invoke();
        });
      }
    }

    public override IBroadcastChannel Channel(string name) {
      Channel chan = _socket?.Channel(name);
      return new BroadcastChannel(chan);
    }
    #endregion

    #region events
    public override event TransportClosedHandler Closed;
    public override event TransportErrorHandler Error;
    public override event TransportOpenedHandler Opened;
    #endregion

    #region private events management
    private void _socket_Error(object sender, ErrorEventArgs e) {
      Error?.Invoke(e.Exception);
    }

    private void _socket_Closed(object sender, EventArgs e) {
      Closed?.Invoke();
    }

    private void _socket_Opened(object sender, EventArgs e) {
      Opened?.Invoke();
    }
    #endregion
  }
}
