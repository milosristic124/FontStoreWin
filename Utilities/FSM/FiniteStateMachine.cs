using System;

namespace Utilities.FSM {
  public class FiniteStateMachine<T> where T : class, IState<T> {
    #region data
    private T _defaultState;
    private bool _started;
    private T _state;
    private bool _startBeforeStop;
    #endregion

    #region properties
    public T State {
      get {
        return _state;
      }
      set {
        if (_started) {
          T oldState = _state;
          T newState = value;

          if (newState == null) {
            _state = _defaultState;
          } else {
            _state = newState;
          }

          if (_startBeforeStop) {
            StartState(newState, this);
            StopState(oldState);
          } else {
            StopState(oldState);
            StartState(newState, this);
          }
        }
      }
    }
    #endregion

    #region ctor
    public FiniteStateMachine(T defaultState, bool startBeforeStop = false) {
      _defaultState = defaultState;
      _state = _defaultState;
      _started = false;
      _startBeforeStop = startBeforeStop;
    }
    #endregion

    #region methods
    public void Start() {
      if (!_started) {
        State.Start(this);
        _started = true;
      }
    }

    public void Stop() {
      if (_started) {
        if (State.WillTransition)
          State.Stop();
        else
          State.Abort();
        _started = false;
      }
    }
    #endregion

    #region private methods
    private static void StopState(T state) {
      if (state.WillTransition)
        state.Stop();
      else // the state is not ready for transition
        state.Abort();
    }

    private static void StartState(T state, FiniteStateMachine<T> context) {
      state.Start(context);
    }
    #endregion
  }
}
