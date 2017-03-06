using System;

namespace Utilities.FSM {
  public class FiniteStateMachine<T> where T : class, IState<T> {
    private T _defaultState;
    private bool _started;
    private T _state;

    public T State {
      get {
        return _state;
      }
      set {
        if (_started) {
          _state.Stop();

          _state = value;
          if (_state == null) {
            _state = _defaultState;
          }

          _state.Start(this);
        }
      }
    }

    public FiniteStateMachine(T defaultState) {
      _defaultState = defaultState;
      _state = _defaultState;
      _started = false;
    }

    public void Start() {
      if (!_started) {
        State.Start(this);
        _started = true;
      }
    }

    public void Stop() {
      if (_started) {
        State.Stop();
        _started = false;
      }
    }

    public void Reset() {
      if (_started) {
        _state.Stop();
      }

      _state = _defaultState;

      if (_started) {
        _state.Start(this);
      }
    }
  }
}
