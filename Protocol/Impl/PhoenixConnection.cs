using Protocol.Payloads;
using Protocol.States;
using System;
using Utilities.FSM;

namespace Protocol.Impl {
  class PhoenixConnection : IConnection {
    #region private
    private FiniteStateMachine<ConnectionState> _fsm;
    private ConnectionContext _stateContext;
    #endregion

    #region properties
    public string EndPoint {
      get {
        return _stateContext.EndPoint;
      }
    }
    #endregion

    #region public
    public PhoenixConnection(string endpoint) {
      _fsm = new FiniteStateMachine<ConnectionState>();

      _stateContext = new ConnectionContext {
        Socket = null,
        EndPoint = endpoint
      };

      _stateContext.OnEstablished += _stateContext_OnEstablished;
    }

    public void Connect(string email, string password) {
      _fsm.State = new Connecting(_stateContext, email, password);
    }

    public void Disconnect() {
    }

    public void UpdateCatalog(DateTime? lastUpdate) {
    }

    public void UpdateFontsStatus(DateTime? lastUpdate) {
    }
    #endregion

    #region events
    public event ConnectionEstablishedHandler OnEstablished;
    public event ConnectionValidationFailedHandler OnValidationFailure;

    public event FontDescriptionHandler OnFontDesctiptionReceived;
    public event FontDeletedHandler OnFontDeleted;

    public event FontActivationHandler OnFontActivated;
    public event FontDeactivationHandler OnFontDeactivated;

    public event UpdateFinishedHandler OnUpdateFinished;
    #endregion

    #region private methods
    private void _stateContext_OnEstablished(UserData userData) {
      OnEstablished?.Invoke(userData);
    }
    #endregion
  }
}
