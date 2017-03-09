using Protocol.Impl.States;
using Protocol.Payloads;
using Protocol.Transport;
using Storage;
using System;
using System.Threading.Tasks;
using Utilities.FSM;

namespace Protocol.Impl {
  public class Connection : AConnection {
    #region private data
    private FiniteStateMachine<ConnectionState> _fsm;
    #endregion

    #region properties
    public IFontStorage Storage { get; private set; }
    #endregion

    #region ctor
    public Connection(IConnectionTransport transport, IFontStorage storage): base(transport) {
      AuthenticationRetryInterval = TimeSpan.FromSeconds(10);
      ConnectionRetryInterval = TimeSpan.FromSeconds(10);

      Storage = storage;

      _fsm = new FiniteStateMachine<ConnectionState>(new Idle(this, Transport));
      _fsm.Start();
    }
    #endregion

    #region methods
    public override void Connect(string email, string password) {
      // All the FSM states lives in their Start method.
      // We must ensure that the calling thread is never blocked (most likely the UI thread)
      Task.Factory.StartNew(() => {
        _fsm.State = new Authenticating(this, Transport, email, password);
      });
    }

    public override void Disconnect() {
    }
    #endregion

    #region internal methods
    internal void TriggerConnectionEstablished(UserData userData) {
      Transport.AuthToken = userData.AuthToken;
      UserData = userData;
      OnEstablished?.Invoke(userData);
    }

    internal void TriggerValidationFailure(string error) {
      OnValidationFailure?.Invoke(error);
    }
    #endregion

    #region public events
    public override event ConnectionEstablishedHandler OnEstablished;
    public override event ConnectionValidationFailedHandler OnValidationFailure;
    #endregion
  }
}
