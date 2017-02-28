using PhoenixSocket;
using Protocol.Payloads;
using System;
using WebSocket4Net;

namespace Protocol.States {
  class Validating : ConnectionState {
    #region private data
    private Channel _validationChan;
    private Authentication _authPayload;
    #endregion

    #region ctor
    private Validating(ConnectionContext context) : base(context) {
      _validationChan = _context.Socket.Channel("validation");
    }

    public Validating(ConnectionContext context, string email, string password) : this(context) {
      _authPayload = new Authentication {
        Login = email,
        Password = password,
        ApplicationVersion = "0.1",
        ProtocolVersion = 1,
        Os = "Win",
        OsVersion = "Unknown"
      };
    }
    #endregion

      #region methods
    public override void Stop() {
    }

    protected override void Start() {
      // join the validation channel
      _validationChan.Join().Receive("ok", delegate {
        _validationChan
        .Push("authenticate", _authPayload) // send the authentication message
        .Receive("ok", data => { // authentication succeed
          UserData userData = data as UserData;
          // trigger the connection established event
          _context.TriggerConnectionEstablished(userData);
          // push the connection setup state (do nothing on start)
        })
        .Receive("ko", delegate { // authentication failed
          throw new NotImplementedException(string.Format("[{0}] Authentication failed", _authPayload.Login));
        });
      });
    }
    #endregion
  }
}
