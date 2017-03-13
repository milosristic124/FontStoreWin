using Protocol.Impl.States;
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

    #region internal properties
    internal Channels.Catalog CatalogChannel { get; private set; }
    internal Channels.User UserChannel { get; private set; }
    #endregion

    #region ctor
    public Connection(IConnectionTransport transport, IFontStorage storage): base(transport) {
      AuthenticationRetryInterval = TimeSpan.FromSeconds(10);
      ConnectionRetryInterval = TimeSpan.FromSeconds(10);

      Storage = storage;

      _fsm = new FiniteStateMachine<ConnectionState>(new Idle(this));
      _fsm.Start();
    }
    #endregion

    #region methods
    public override void Connect(string email, string password) {
      AssertTransition<Authenticating>("connect");

      // All the FSM states lives in their Start method.
      // We must ensure that the calling thread is never blocked (most likely the UI thread)
      Task.Factory.StartNew(() => {
        _fsm.State = new Authenticating(this, email, password);
      });
    }

    public override void Disconnect() {
    }

    public override void UpdateCatalog() {
      AssertTransition<UpdatingCatalog>("update the catalog");

      Task.Factory.StartNew(() => {
        _fsm.State = new UpdatingCatalog(this);
      });
    }
    #endregion

    #region internal methods
    internal void TriggerConnectionEstablished(Payloads.UserData userData) {
      Transport.AuthToken = userData.AuthToken;
      UserData = userData;

      CatalogChannel = new Channels.Catalog(this);
      UserChannel = new Channels.User(this);

      OnEstablished?.Invoke(userData);
    }

    internal void TriggerValidationFailure(string error) {
      OnValidationFailure?.Invoke(error);
    }
    #endregion

    #region private methods
    private void AssertTransition<T>(string action) where T: ConnectionState {
      if (!_fsm.State.CanTransitionTo<T>()) {
        throw new Exception(string.Format("The protocol can't {0} when the connection is in the {1} state.", action, _fsm.State.Name));
      }
    }
    #endregion

    #region public events
    public override event ConnectionEstablishedHandler OnEstablished;
    public override event ConnectionValidationFailedHandler OnValidationFailure;
    public override event CatalogUpdateFinished OnCatalogUpdateFinished;
    #endregion
  }
}
