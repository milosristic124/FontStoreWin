using System;
using UI.Utilities;
using Utilities.Extensions;

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
    public Login(App application, WindowPosition prevPos = null) : base(application, prevPos) {
      _view = new Views.Login();
      Application.SetDragHandle(_view.DragHandle);
      Application.MainWindow = _view;
      SetWindowPosition(_view, WindowPosition);
      _view.Hide();

      _view.OnConnect += _view_OnConnect;
      _view.OnExit += _view_OnExit;

      Application.Context.Connection.OnValidationFailure += Connection_OnValidationFailure;
      Application.Context.Connection.OnEstablished += Connection_OnEstablished;
    }
    #endregion

    #region methods
    public override void Show() {
      if (!IsShown) {
        SetWindowPosition(_view, WindowPosition);
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
      Application.Context.Connection.OnValidationFailure -= Connection_OnValidationFailure;
      Application.Context.Connection.OnEstablished -= Connection_OnEstablished;

      _view.OnConnect -= _view_OnConnect;
      _view.OnExit -= _view_OnExit;
    }
    #endregion

    #region private methods
    protected override async void Start() {
      base.Start();

      string savedCreds = await Application.Context.Storage.LoadCredentials();
      if (savedCreds != null) {
        Console.WriteLine("Credentials loaded");
        _view.InvokeOnUIThread(() => {
          _view.ConnectionRequestStarted();
        });
        Application.Context.Connection.AutoConnect(savedCreds);
      }
    }
    #endregion

    #region action handling
    private void _view_OnConnect(string email, string password, bool saveCredentials) {
      _saveCredentials = saveCredentials;
      _view.ConnectionRequestStarted();
      Application.Context.Connection.Connect(email, password);
    }

    private void _view_OnExit() {
      Application.Context.Connection.Disconnect(Protocol.DisconnectReason.Quit);
      Application.Shutdown();
    }
    #endregion

    #region connection event handling
    private async void Connection_OnEstablished(Protocol.Payloads.UserData userData) {
      if (_saveCredentials) {
        await Application.Context.Storage.SaveCredentials(userData.AuthToken);
        Console.WriteLine("Credentials saved");
      }
      _view.InvokeOnUIThread(() => {
        WillTransition = true;
        FSM.State = new FontList(Application, WindowPosition.FromWindow(_view));
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
