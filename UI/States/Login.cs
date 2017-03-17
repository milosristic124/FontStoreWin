namespace UI.States {
  class Login : UIState {
    private Views.Login _view = null;

    public override bool IsShown {
      get {
        return _view != null;
      }
    }

    public Login(App application) : base(application) {
    }

    public override void Show() {
      if (_view == null) {
        _view = new Views.Login();
        Application.MainWindow = _view;
        SetWindowPosition(_view);
      }
    }

    public override void Hide() {
      _view?.Close();
      _view = null;
    }
  }
}
