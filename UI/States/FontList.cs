using System;
using System.Threading.Tasks;
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
      Application.Context.Connection.OnDisconnected += Connection_Disconnected;
      Application.Context.Connection.OnEstablished += Connection_OnEstablished; ;
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

        await LoadContent();
      }
    }

    public override void Dispose() {
      _view.OnExit -= _view_OnExit;
      _view.OnLogout -= _view_OnLogout;
      Application.Context.Connection.OnCatalogUpdateFinished -= Connection_OnCatalogUpdateFinished;
      Application.Context.Connection.OnDisconnected -= Connection_Disconnected;
      Application.Context.Storage.OnFontInstall -= Storage_OnFontInstall;
      Application.Context.Storage.OnFontUninstall -= Storage_OnFontUninstall;
    }
    #endregion

    #region private methods
    private async Task LoadContent(bool reload = false) {
      if (reload || !Application.Context.Storage.Loaded) {
        ShowLoadingState();
        try {
          await Application.Context.Storage.Load();
        } catch (Exception e) {
          Console.WriteLine(string.Format("Catalog loading failed: {0}", e.Message));
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

    private void Connection_Disconnected(string reason) {
      _view.InvokeOnUIThread(delegate {
        _view.Disconnected(reason);
      });
    }

    private async void Connection_OnEstablished(Protocol.Payloads.UserData userData) {
      await LoadContent(true);
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
