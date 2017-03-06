using Protocol.Transport;
using System;

namespace Protocol.Impl.States {
  class Connecting : ConnectionState {
    #region private data
    private bool _connected;
    private string _email;
    private string _password;
    #endregion

    #region ctor
    public Connecting(AConnectionTransport context, string email, string password) : this(context) {
      _email = email;
      _password = password;
      _connected = false;
    }

    private Connecting(AConnectionTransport context) : base(context) {
      _context.Opened += _transport_Opened;
      _context.Error += _transport_Error;
    }
    #endregion

    #region state management
    protected override void Start() {
      _connected = false;
      _context.Connect();
    }

    public override void Stop() {
      _context.Opened -= _transport_Opened;
      _context.Error -= _transport_Error;

      // if we leave the connecting state before the connection is achieved
      if (!_connected) {
        // then we need to stop the connection and do some cleanup
        _context.Disconnect();
      }
    }
    #endregion

    #region private event handling
    private void _transport_Opened() {
      _connected = true;
      FSM.State = new Validating(_context, _email, _password);
    }

    private void _transport_Error(Exception exception) {
      _connected = true;
      throw new NotImplementedException(string.Format("[{0}] Connection failed", _email), exception);
    }
    #endregion
  }
}
