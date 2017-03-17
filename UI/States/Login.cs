namespace UI.States {
  class Login : UIState {
    #region data
    private Views.Login _view = null;
    private bool _saveCredentials = false;
    #endregion

    #region properties
    public override bool IsShown {
      get {
        return _view.IsVisible;
      }
    }
    #endregion

    #region ctor
    public Login(App application) : base(application) {
      _view = new Views.Login();
      Application.SetDragHandle(_view.DragHandle);
      Application.MainWindow = _view;
      SetWindowPosition(_view);
      _view.Hide();

      _view.OnConnect += _view_OnConnect;
      _view.OnExit += _view_OnExit;

      Application.Connection.OnValidationFailure += Connection_OnValidationFailure;
      Application.Connection.OnEstablished += Connection_OnEstablished;
    }
    #endregion

    #region methods
    public override void Show() {
      if (!IsShown) {
        SetWindowPosition(_view);
        _view.Show();
        _view.Activate();
      }
    }

    public override void Hide() {
      if (IsShown) {
        _view.Hide();
      }
    }

    public override void Dispose() {
      Application.Connection.OnValidationFailure -= Connection_OnValidationFailure;
      Application.Connection.OnEstablished -= Connection_OnEstablished;

      _view.OnConnect -= _view_OnConnect;
      _view.OnExit -= _view_OnExit;
    }
    #endregion

    #region action handling
    private void _view_OnConnect(string email, string password, bool saveCredentials) {
      _saveCredentials = saveCredentials;
      _view.ConnectionRequestStarted();
      Application.Connection.Connect(email, password);
    }

    private void _view_OnExit() {
      Application.Connection.Disconnect();
      Application.Shutdown();
    }
    #endregion

    #region connection event handling
    private void Connection_OnEstablished(Protocol.Payloads.UserData userData) {
      _view.InvokeOnUIThread(() => {
        WillTransition = true;
        FSM.State = new FontList(Application);
        FSM.State.Show();
        Dispose();
      });
    }

    private void Connection_OnValidationFailure(string reason) {
      _view.InvokeOnUIThread(() => {
        _view.ConnectionRequestFailed(reason);
      });
    }
    #endregion
  }
}
