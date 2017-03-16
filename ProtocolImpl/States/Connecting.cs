using Protocol.Transport;
using System;

namespace Protocol.Impl.States {
  class Connecting : ConnectionState {
    #region private data
    private Payloads.UserData _userData;
    #endregion

    #region ctor
    public Connecting(Connection connection, Payloads.UserData userData) : this(connection) {
      _userData = userData;
      _context.Transport.EndPoint = string.Format("wss://app.fontstore.com/socket/websocket?reuse_token={0}", _userData.AuthToken);
    }

    private Connecting(Connection connection) : base("Connecting", connection) {
      _context.Transport.Opened += _transport_Opened;
      _context.Transport.Error += _transport_Error;
      _userData = null;
    }
    #endregion

    #region methods
    public override void Stop() {
      _context.Transport.Opened -= _transport_Opened;
      _context.Transport.Error -= _transport_Error;
    }

    public override void Abort() {
      _context.Transport.Opened -= _transport_Opened;
      _context.Transport.Error -= _transport_Error;
      _context.Transport.Disconnect();
    }

    protected override void Start() {
      _context.Transport.Connect();
    }
    #endregion

    #region event handling
    private void _transport_Opened() {
      WillTransition = true;
      FSM.State = new Connected(_context, _userData);
    }

    private void _transport_Error(Exception exception) {
      WillTransition = true;
      FSM.State = new RetryConnecting(_context, _userData);
    }
    #endregion
  }
}
