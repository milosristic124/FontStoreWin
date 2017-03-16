using Utilities.FSM;

namespace UI.States {
  abstract class UIState : IState<UIState> {
    #region data
    protected App Application;
    #endregion

    #region properties
    public FiniteStateMachine<UIState> FSM { get; private set; }
    public bool WillTransition { get; private set; }

    public abstract bool IsShown { get; }
    #endregion

    #region ctor
    public UIState(App application) {
      Application = application;
    }
    #endregion

    #region methods
    public virtual void Abort() {
    }

    public void Start(FiniteStateMachine<UIState> fsm) {
      FSM = fsm;
    }

    public virtual void Stop() {
    }
    #endregion

    #region UIState methods
    public abstract void Hide();
    public abstract void Show();
    #endregion
  }

  class NoUI : UIState {
    public override bool IsShown {
      get {
        return true;
      }
    }

    public NoUI(App application) : base(application) {
    }

    public override void Show() {
    }

    public override void Hide() {
    }
  }
}
