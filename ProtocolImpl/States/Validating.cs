using Protocol.Payloads;
using Protocol.Transport;
using System;

namespace Protocol.Impl.States {
  class Validating : ConnectionState {
    #region private data
    private IBroadcastChannel _validationChan;
    private Authentication _authPayload;
    #endregion

    #region ctor
    private Validating(AConnectionTransport context) : base(context) {
      _validationChan = _context.Channel("validation");
    }

    public Validating(AConnectionTransport context, string email, string password) : this(context) {
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
          .Send("authenticate", _authPayload) // send the authentication message
          .Receive("ok", data => { // authentication succeed
            UserData userData = data as UserData;
            _validationChan.Leave(); // we don't need the validation channel anymore
            //_context.TriggerConnectionEstablished(userData); // trigger the connection established event
            //FSM.State = new Setup(_context); // push the connection setup state
          })
          .Receive("ko", delegate { // authentication failed
            throw new NotImplementedException(string.Format("[{0}] Authentication failed", _authPayload.Login));
          });
      });
    }
    #endregion
  }
}
