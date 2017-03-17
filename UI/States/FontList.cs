using System;

namespace UI.States {
  class FontList : UIState {
    #region data
    private Views.FontList _view = null;
    #endregion

    #region properties
    public override bool IsShown {
      get {
        return _view.IsVisible;
      }
    }
    #endregion

    #region ctor
    public FontList(App application) : base(application) {
      _view = new Views.FontList(Application.Connection.UserData);
      Application.SetDragHandle(_view.DragHandle);
      Application.MainWindow = _view;
      SetWindowPosition(_view);

      _view.OnExit += _view_OnExit;
      _view.OnLogout += _view_OnLogout;
    }
    #endregion

    #region methods
    public override void Hide() {
      if (IsShown) {
        _view.Hide();
      }
    }

    public override void Show() {
      if (!IsShown) {
        SetWindowPosition(_view);
        _view.Activate();
        _view.Show();
      }
    }

    public override void Dispose() {
      _view.OnExit -= _view_OnExit;
      _view.OnLogout -= _view_OnLogout;
    }
    #endregion

    #region action handling
    private void _view_OnLogout() {
      Application.Connection.Disconnect();
      _view.InvokeOnUIThread(() => {
        WillTransition = true;
        FSM.State = new Login(Application);
        FSM.State.Show();
        Dispose();
      });
    }

    private void _view_OnExit() {
      Application.Connection.Disconnect();
      Application.Shutdown();
    }
    #endregion
  }
}
