using Protocol.Transport;
using Utilities.FSM;

namespace Protocol.Impl.States {
  abstract class ConnectionState : AState<ConnectionState, Connection> {
    #region property
    public string Name { get; private set; }
    #endregion

    #region ctor
    public ConnectionState(string name, Connection connection) : base(connection) {
      Name = name;
    }
    #endregion

    #region methods
    public override void Start(FiniteStateMachine<ConnectionState> fsm) {
      FSM = fsm;
      Start();
    }

    // Test if an external actor can modify the FSM state
    // An FSM state can change the next state independetly of this function
    // Usage:
    //   - to allow transition to a specific state
    // public override bool CanTransitionTo<MyCustomState>()
    //   - to allow transition to any state
    // public override bool CanTransitionTo<T>()
    // Default:
    // no transition allowed
    public virtual bool CanTransitionTo<T>() where T: ConnectionState {
      return false;
    }
    #endregion

    #region abstract methods
    protected abstract void Start();
    #endregion
  }

  sealed class Idle : ConnectionState {
    public Idle(Connection connection) : base("Idle", connection) {
      WillTransition = true;
    }

    public override bool CanTransitionTo<T>() {
      return true;
    }

    public override void Stop() {
    }

    public override void Abort() {
    }

    protected override void Start() {
    }
  }
}
