﻿namespace Utilities.FSM {
  public interface IState<T> where T: class, IState<T> {
    FiniteStateMachine<T> FSM { get; }

    void Start(FiniteStateMachine<T> fsm);
    void Stop();
  }
}
