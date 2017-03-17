using System;
using System.Windows;
using UI.Utilities;
using Utilities.FSM;

namespace UI.States {
  abstract class UIState : IState<UIState>, IDisposable {
    #region data
    protected App Application;
    #endregion

    #region properties
    public FiniteStateMachine<UIState> FSM { get; private set; }
    public bool WillTransition { get; protected set; }

    public abstract bool IsShown { get; }
    #endregion

    #region ctor
    public UIState(App application) {
      Application = application;
    }
    #endregion

    #region methods
    public virtual void Abort() {
      Stop();
    }

    public void Start(FiniteStateMachine<UIState> fsm) {
      FSM = fsm;
    }

    public virtual void Stop() {
      if (IsShown) {
        Hide();
      }
    }
    #endregion

    #region UIState methods
    public abstract void Hide();
    public abstract void Show();
    public abstract void Dispose();
    #endregion

    #region internal methods
    protected void SetWindowPosition(Window window, double bottomMargin = 10, double rightMargin = 10) {
      double left, top;
      TaskBarLocationProvider.CalculateWindowPositionByTaskbar(window.Width, window.Height, out left, out top);
      window.Top = top - bottomMargin;
      window.Left = left - rightMargin;
    }
    #endregion
  }

  class NoUI : UIState {
    public override bool IsShown {
      get {
        return false;
      }
    }

    public NoUI(App application) : base(application) {
    }

    public override void Show() {
    }
    public override void Hide() {
    }
    public override void Dispose() {
    }
  }
}
