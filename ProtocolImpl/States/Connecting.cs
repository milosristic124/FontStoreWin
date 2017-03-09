using Protocol.Payloads;
using Protocol.Transport;
using System;

namespace Protocol.Impl.States {
  class Connecting : ConnectionState {
    #region private data
    private UserData _userData;
    #endregion

    #region ctor
    public Connecting(Connection connection, IConnectionTransport transport, UserData userData) : this(connection, transport) {
      _userData = userData;
      _context.EndPoint = string.Format("wss://app.fontstore.com/connect?token={0}", _userData.AuthToken);
    }

    private Connecting(Connection connection, IConnectionTransport transport) : base(connection, transport) {
      _context.Opened += _transport_Opened;
      _context.Error += _transport_Error;
      _userData = null;
    }
    #endregion

    #region methods
    public override void Stop() {
      _context.Opened -= _transport_Opened;
      _context.Error -= _transport_Error;
    }

    public override void Abort() {
      _context.Opened -= _transport_Opened;
      _context.Error -= _transport_Error;
      _context.Disconnect();
    }

    protected override void Start() {
      _context.Connect();
    }
    #endregion

    #region event handling
    private void _transport_Opened() {
      _connection.TriggerConnectionEstablished(_userData);
      WillTransition = true;
      //FSM.State = new Setup(_connection, _context);
    }

    private void _transport_Error(Exception exception) {
      WillTransition = true;
      FSM.State = new RetryConnecting(_connection, _context, _userData);
    }
    #endregion
  }
}
