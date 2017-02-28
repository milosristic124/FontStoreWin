using Utilities.FSM;

namespace Protocol.States {
  abstract class ConnectionState : AState<ConnectionState, ConnectionContext> {
    public ConnectionState(ConnectionContext context) : base(context) {
    }

    public override void Start(FiniteStateMachine<ConnectionState> fsm) {
      FSM = fsm;
      Start();
    }

    protected abstract void Start();
  }
}
