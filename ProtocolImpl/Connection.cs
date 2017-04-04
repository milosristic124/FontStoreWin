using Protocol.Impl.States;
using Protocol.Transport;
using Protocol.Transport.Http;
using Storage;
using System;
using System.Threading.Tasks;
using Utilities.FSM;

namespace Protocol.Impl {
  public class Connection : AConnection {
    #region private data
    private FiniteStateMachine<ConnectionState> _fsm;
    #endregion

    #region internal properties
    internal Channels.Catalog CatalogChannel { get; private set; }
    internal Channels.User UserChannel { get; private set; }
    #endregion

    #region public events
    public override event ConnectionEstablishedHandler OnEstablished;
    public override event ConnectionValidationFailedHandler OnValidationFailure;
    public override event CatalogUpdateFinishedHandler OnCatalogUpdateFinished;
    public override event ConnectionClosedHandler OnConnectionClosed;
    public override event DisconnectionHandler OnDisconnected;
    #endregion

    #region ctor
    public Connection(IConnectionTransport transport, IHttpTransport http, IFontStorage storage): base(transport, http, storage) {
      AuthenticationRetryInterval = TimeSpan.FromSeconds(10);
      ConnectionRetryInterval = TimeSpan.FromSeconds(10);
      HttpTransport.DownloadParallelism = 3;

      _fsm = new FiniteStateMachine<ConnectionState>(new Idle(this));
      _fsm.Start();
    }
    #endregion

    #region methods
    public override void Connect(string email, string password) {
      if (CanTransition<Authenticating>()) {
        Transport.Closed += Transport_Closed;

        // All the FSM states lives in their Start method.
        // We must ensure that the calling thread is never blocked (most likely the UI thread)
        Task.Run(() => {
          _fsm.State = new Authenticating(this, email, password);
        });
      }
    }

    public override void Disconnect(DisconnectReason reason, string error = null) {
      Transport.Closed -= Transport_Closed;

      Task.Run(() => {
        _fsm.State = new Disconnecting(this, reason, error);
      });
    }

    public override void UpdateCatalog() {
      AssertTransition<UpdatingCatalog>("update the catalog");

      Task.Run(() => {
        _fsm.State = new UpdatingCatalog(this);
      });
    }
    #endregion

    #region internal methods
    internal void TriggerConnectionEstablished(Payloads.UserData userData) {
      Transport.AuthToken = userData.AuthToken;
      UserData = userData;
      Storage.SessionID = UserData.UID;

      CatalogChannel = new Channels.Catalog(this);
      UserChannel = new Channels.User(this);

      UserChannel.OnDisconnection += UserChannel_OnDisconnection;

      OnEstablished?.Invoke(userData);
    }

    internal void TriggerValidationFailure(string error) {
      OnValidationFailure?.Invoke(error);
    }

    internal void TriggerUpdateFinished() {
      OnCatalogUpdateFinished?.Invoke();
    }

    internal void TriggerConnectionClosed() {
      UserChannel.OnDisconnection -= UserChannel_OnDisconnection;

      CatalogChannel = null;
      UserChannel = null;
      _fsm.State = new Idle(this);

      OnConnectionClosed?.Invoke();
    }
    #endregion

    #region private methods
    private void AssertTransition<T>(string action) where T: ConnectionState {
      if (!CanTransition<T>()) {
        throw new Exception(string.Format("The protocol can't {0} when the connection is in the {1} state.", action, _fsm.State.Name));
      }
    }

    private bool CanTransition<T>() where T: ConnectionState {
      return _fsm.State.CanTransitionTo<T>();
    }
    #endregion

    #region event handling
    private void UserChannel_OnDisconnection(string reason) {
      OnDisconnected?.Invoke(reason);
    }

    private void Transport_Closed() {
      OnDisconnected?.Invoke("");
    }
    #endregion
  }
}
