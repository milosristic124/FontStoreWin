using Logging;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using UI.Utilities;

namespace UI.States {
  class FontList : UIState {
    #region data
    private Views.FontList _view = null;
    private bool _reconnecting = false;
    private bool _loading = false;
    private bool _disconnecting = false;
    #endregion

    #region properties
    public override bool IsShown {
      get {
        return _view.IsVisible;
      }
    }
    #endregion

    #region ctor
    public FontList(App application, WindowPosition prevPos = null) : base(application, prevPos) {
      Logger.Log("FontList state created");
      _view = new Views.FontList(Application.Context.Connection.UserData, Application.Context.FontInstaller);
      Application.SetDragHandle(_view.DragHandle);
      Application.MainWindow = _view;
      SetWindowPosition(_view, WindowPosition);

      _view.Storage = Application.Context.Storage;

      _view.OnExit += _view_OnExit;
      _view.OnLogout += _view_OnLogout;
      _view.OnAboutClicked += _view_OnAboutClicked;

      Application.Context.Connection.OnCatalogUpdateFinished += Connection_OnCatalogUpdateFinished;
      Application.Context.Connection.OnDisconnected += Connection_Disconnected;
      Application.Context.Connection.OnEstablished += Connection_OnEstablished;
      Application.Context.Connection.OnConnectionTerminated += Connection_Terminated;

      Application.Context.Storage.OnFontInstall += Storage_OnFontInstall;
      Application.Context.Storage.OnFontUninstall += Storage_OnFontUninstall;
    }
    #endregion

    #region methods
    public override void Hide() {
      if (IsShown) {
        _view.Hide();
      }
    }

    public override async void Show() {
      if (!IsShown) {
        SetWindowPosition(_view, WindowPosition);
        _view.Activate();
        _view.Show();

        if (_reconnecting) {
          ShowLoadingState();
        }
        else {
          await LoadContent();
        }
      }
    }

    public override void Dispose() {
      Logger.Log("FontList state disposed");
      _view.OnExit -= _view_OnExit;
      _view.OnLogout -= _view_OnLogout;
      _view.OnAboutClicked -= _view_OnAboutClicked;
      Application.Context.Connection.OnCatalogUpdateFinished -= Connection_OnCatalogUpdateFinished;
      Application.Context.Connection.OnDisconnected -= Connection_Disconnected;
      Application.Context.Connection.OnConnectionTerminated -= Connection_Terminated;

      Application.Context.Storage.OnFontInstall -= Storage_OnFontInstall;
      Application.Context.Storage.OnFontUninstall -= Storage_OnFontUninstall;
    }
    #endregion

    #region private methods
    private async Task LoadContent(bool force = false) {
      if (_disconnecting || _loading) {
        ShowLoadingState();
      } else if (force || !Application.Context.Storage.Loaded) {
        _loading = true;
        ShowLoadingState();
        try {
          await Application.Context.Storage.LoadFonts();
        } catch (Exception e) {
          Logger.Log("Catalog loading failed: {0}", e);
        }
        Application.Context.Connection.UpdateCatalog();
      }
      else {
        ShowLoadedState();
      }
    }

    private void ShowLoadingState() {
      _view.InvokeOnUIThread(() => {
        _view.LoadingState(true);
      });
    }

    private void ShowLoadedState() {
      _view.InvokeOnUIThread(() => {
        _view.LoadingState(false);
      });
    }
    #endregion

    #region action handling
    private void _view_OnLogout() {
      _disconnecting = true;
      Application.Context.Connection.OnConnectionClosed += Connection_OnConnectionClosed_logout;
      Application.Context.Connection.Disconnect(Protocol.DisconnectReason.Logout);
    }

    private void _view_OnExit() {
      _disconnecting = true;
      Application.Context.Connection.OnConnectionClosed += Connection_OnConnectionClosed_exit;
      Application.Context.Connection.Disconnect(Protocol.DisconnectReason.Quit);
    }

    private void _view_OnAboutClicked() {
      _view.InvokeOnUIThread(delegate {
        Application.ShowAboutPopup(_view);
      });
    }
    #endregion

    #region event handling
    private void Connection_Terminated(string reason) {
      _view.InvokeOnUIThread(() => {
        _view.Terminated(reason);
        WillTransition = true;
        FSM.State = new Login(Application, WindowPosition.FromWindow(_view));
        FSM.State.Show();
      });
    }

    private async void Connection_OnCatalogUpdateFinished(int newFontCount) {
      await Application.Context.Storage.SaveFonts();

      ShowLoadedState();
      _loading = false;
      if (newFontCount > 0) {
        Application.ShowNotification($"{newFontCount} new fonts synchronized", System.Windows.Forms.ToolTipIcon.Info);
      }
    }

    private bool Connection_Disconnected() {
      _reconnecting = true;
      return _view.InvokeOnUIThread(delegate {
        return _view.Disconnected();
      });
    }

    private async void Connection_OnEstablished(Protocol.Payloads.UserData userData) {
      if (_reconnecting) { // we should only be called if we reconnect after having lost the connection
        _reconnecting = false;
        await LoadContent(true);
      }
    }

    private async void Connection_OnConnectionClosed_logout() {
      await Application.Context.Storage.SaveFonts();
      await Application.Context.Storage.CleanCredentials();

      string appPath = Assembly.GetEntryAssembly().Location;
      _view.InvokeOnUIThread(delegate {
        Process.Start(appPath);
        Application.Shutdown();
      });
    }

    private async void Connection_OnConnectionClosed_exit() {
      await Application.Context.Storage.SaveFonts();
      _view.InvokeOnUIThread(delegate {
        Application.Shutdown();
      });
    }

    private void Storage_OnFontUninstall(Storage.Data.Font font, bool succeed) {
    }

    private void Storage_OnFontInstall(Storage.Data.Font font, bool succeed) {
    }
    #endregion
  }
}
