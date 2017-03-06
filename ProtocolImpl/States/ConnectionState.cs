using System;
using Protocol.Transport;
using Utilities.FSM;

namespace Protocol.Impl.States {
  abstract class ConnectionState : AState<ConnectionState, AConnectionTransport> {
    public ConnectionState(AConnectionTransport context) : base(context) {
    }

    public override void Start(FiniteStateMachine<ConnectionState> fsm) {
      FSM = fsm;
      Start();
    }

    protected abstract void Start();
  }

  sealed class Idle : ConnectionState {
    public Idle(AConnectionTransport context) : base(context) {
    }

    public override void Stop() {
    }

    protected override void Start() {
    }
  }
}
