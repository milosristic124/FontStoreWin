using Protocol.Impl.States;
using Protocol.Payloads;
using Protocol.Transport;
using System;
using System.Threading.Tasks;
using Utilities.FSM;

namespace Protocol.Impl {
  public class Connection : AConnection {
    #region private data
    private FiniteStateMachine<ConnectionState> _fsm;
    #endregion

    #region ctor
    public Connection(IConnectionTransport transport): base(transport) {
      AuthenticationRetryInterval = TimeSpan.FromSeconds(10);
      ConnectionRetryInterval = TimeSpan.FromSeconds(10);

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
    public override event FontDescriptionHandler OnFontDesctiptionReceived;
    public override event FontDeletedHandler OnFontDeleted;
    public override event FontActivationHandler OnFontActivated;
    public override event FontDeactivationHandler OnFontDeactivated;
    public override event UpdateFinishedHandler OnUpdateFinished;
    #endregion
  }
}
