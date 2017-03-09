using Protocol.Transport;
using Utilities.FSM;

namespace Protocol.Impl.States {
  abstract class ConnectionState : AState<ConnectionState, IConnectionTransport> {
    #region private data
    protected Connection _connection;
    #endregion

    #region ctor
    public ConnectionState(Connection connection, IConnectionTransport transport) : base(transport) {
      _connection = connection;
    }
    #endregion

    #region methods
    public override void Start(FiniteStateMachine<ConnectionState> fsm) {
      FSM = fsm;
      Start();
    }
    #endregion

    #region abstract methods
    protected abstract void Start();
    #endregion
  }

  sealed class Idle : ConnectionState {
    public Idle(Connection connection, IConnectionTransport transport) : base(connection, transport) {
    }

    public override void Stop() {
    }

    public override void Abort() {
    }

    protected override void Start() {
    }
  }
}
