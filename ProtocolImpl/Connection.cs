using Protocol.Impl.States;
using Protocol.Payloads;
using Protocol.Transport;
using System;
using Utilities.FSM;

namespace Protocol.Impl {
  public class Connection : AConnection {
    #region private
    private FiniteStateMachine<ConnectionState> _fsm;
    #endregion

    #region ctor
    public Connection(AConnectionTransport transport): base(transport) {
      _fsm = new FiniteStateMachine<ConnectionState>(new Idle(Transport));
      _fsm.Start();
    }
    #endregion

    #region methods
    public override void Connect(string email, string password) {
      _fsm.State = new Connecting(Transport, email, password);
    }

    public override void Disconnect() {
    }

    public override void UpdateCatalog(DateTime? lastUpdate) {
    }

    public override void UpdateFontsStatus(DateTime? lastUpdate) {
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

    #region private methods
    #endregion
  }
}
