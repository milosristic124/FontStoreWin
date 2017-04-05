using System;
using UI.Utilities;
using Utilities.Extensions;

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
    public FontList(App application, WindowPosition prevPos = null) : base(application, prevPos) {
      _view = new Views.FontList(Application.Context.Connection.UserData);
      Application.SetDragHandle(_view.DragHandle);
      Application.MainWindow = _view;
      SetWindowPosition(_view, WindowPosition);

      _view.Storage = Application.Context.Storage;

      _view.OnExit += _view_OnExit;
      _view.OnLogout += _view_OnLogout;

      Application.Context.Connection.OnCatalogUpdateFinished += Connection_OnCatalogUpdateFinished;
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

        if (!Application.Context.Storage.Loaded) {
          ShowLoadingState();
          await Application.Context.Storage.Load()
            .Then(() => {
              Application.Context.Connection.UpdateCatalog();
            })
            .Recover(e => {
              Console.WriteLine(string.Format("Catalog loading failed: {0}", e.Message));
            });
        } else {
          ShowLoadedState();
        }
      }
    }

    public override void Dispose() {
      _view.OnExit -= _view_OnExit;
      _view.OnLogout -= _view_OnLogout;
      Application.Context.Connection.OnCatalogUpdateFinished -= Connection_OnCatalogUpdateFinished;
      Application.Context.Storage.OnFontInstall -= Storage_OnFontInstall;
      Application.Context.Storage.OnFontUninstall -= Storage_OnFontUninstall;
    }
    #endregion

    #region private methods
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
      Application.Context.Connection.Disconnect(Protocol.DisconnectReason.Logout);
      _view.InvokeOnUIThread(() => {
        WillTransition = true;
        FSM.State = new Login(Application, WindowPosition.FromWindow(_view));
        FSM.State.Show();
        Dispose();
      });
    }

    private void _view_OnExit() {
      Application.Context.Connection.Disconnect(Protocol.DisconnectReason.Quit);
      Application.Shutdown();
    }
    #endregion

    #region event handling
    private async void Connection_OnCatalogUpdateFinished() {
      await Application.Context.Storage.Save();
      ShowLoadedState();
    }

    private void Storage_OnFontUninstall(Storage.Data.Font font, FontInstaller.InstallationScope scope, bool succeed) {
      _view.InvokeOnUIThread(() => {
        _view.UpdateCounters();
      });
    }

    private void Storage_OnFontInstall(Storage.Data.Font font, FontInstaller.InstallationScope scope, bool succeed) {
      _view.InvokeOnUIThread(() => {
        _view.UpdateCounters();
      });
    }
    #endregion
  }
}
