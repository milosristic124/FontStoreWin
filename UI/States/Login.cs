using Logging;
using System;
using System.Drawing;
using UI.Utilities;

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
      Logger.Log("Login state created");
      _view = new Views.Login();
      Application.SetDragHandle(_view.DragHandle);
      Application.MainWindow = _view;
      SetWindowPosition(_view, WindowPosition);
      _view.Hide();

      _view.OnConnect += _view_OnConnect;
      _view.OnExit += _view_OnExit;
      _view.OnAboutClicked += _view_OnAboutClicked;

      Application.Context.Connection.OnValidationFailure += Connection_OnValidationFailure;
      Application.Context.Connection.OnEstablished += Connection_OnEstablished;

      Application.Context.Connection.OnCatalogUpdateFinished += Connection_OnCatalogUpdateFinished;
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
      Logger.Log("Login state disposed");
      Application.Context.Connection.OnValidationFailure -= Connection_OnValidationFailure;
      Application.Context.Connection.OnEstablished -= Connection_OnEstablished;
      Application.Context.Connection.OnCatalogUpdateFinished -= Connection_OnCatalogUpdateFinished;

      _view.OnConnect -= _view_OnConnect;
      _view.OnExit -= _view_OnExit;
      _view.OnAboutClicked -= _view_OnAboutClicked;
    }
    #endregion

    #region private methods
    protected override async void Start() {
      base.Start();

      string savedCreds = await Application.Context.Storage.LoadCredentials();
      if (savedCreds != null) {
        Logger.Log("Credentials loaded");
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

    private void _view_OnAboutClicked() {
      _view.InvokeOnUIThread(delegate {
        Application.ShowAboutPopup(_view);
      });
    }
    #endregion

    #region connection event handling
    private async void Connection_OnEstablished(Protocol.Payloads.UserData userData) {
      if (_saveCredentials) {
        await Application.Context.Storage.SaveCredentials(userData.AuthToken);
        Logger.Log("Credentials saved");
      }

      try {
        await Application.Context.Storage.LoadFonts();
      }
      catch (Exception e) {
        Logger.Log("Catalog loading failed: {0}", e);
      }
      Application.Context.Connection.UpdateCatalog();
    }

    private void Connection_OnValidationFailure(string reason) {
      _view.InvokeOnUIThread(() => {
        _view.ConnectionRequestFailed(reason);
      });
    }

    private async void Connection_OnCatalogUpdateFinished(int newFontCount) {
      await Application.Context.Storage.SaveFonts();
      if (newFontCount > 0) {
        Application.ShowNotification($"{newFontCount} new fonts synchronized", System.Windows.Forms.ToolTipIcon.Info);
      }

      _view.InvokeOnUIThread(() => {
        WillTransition = true;
        FSM.State = new FontList(Application, WindowPosition.FromWindow(_view));
        FSM.State.Show();
        Dispose();
      });
    }
    #endregion
  }
}
