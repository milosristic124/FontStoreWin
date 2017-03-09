using System;

namespace Utilities.FSM {
  public abstract class AState<T, U> : IState<T> where T : class, IState<T> {
    protected U _context;

    public FiniteStateMachine<T> FSM { get; protected set; }
    public bool WillTransition { get; protected set; }

    public AState(U context) {
      _context = context;
    }

    public abstract void Start(FiniteStateMachine<T> fsm);
    public abstract void Stop();
    public abstract void Abort();
  }
}
