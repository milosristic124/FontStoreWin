using System;
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
    public FontList(App application) : base(application) {
      _view = new Views.FontList(Application.Connection.UserData);
      Application.SetDragHandle(_view.DragHandle);
      Application.MainWindow = _view;
      SetWindowPosition(_view);

      _view.OnExit += _view_OnExit;
      _view.OnLogout += _view_OnLogout;

      Application.Connection.OnCatalogUpdateFinished += Connection_OnCatalogUpdateFinished;
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
        SetWindowPosition(_view);
        _view.Activate();
        _view.Show();

        if (!Application.Storage.Loaded) {
          ShowLoadingState();
          await Application.Storage.Load()
            .Then(() => {
              Application.Connection.UpdateCatalog();
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
    }
    #endregion

    #region private methods
    private void ShowLoadingState() {
      _view.InvokeOnUIThread(() => {
        Console.WriteLine("Loading catalog");
        _view.LoadingState(true);
      });
    }

    private void ShowLoadedState() {
      _view.InvokeOnUIThread(() => {
        Console.WriteLine("Catalog loaded");
        _view.AllCount = Application.Storage.Families.Count;
        _view.NewCount = Application.Storage.NewFamilies.Count;
        _view.InstalledCount = Application.Storage.ActivatedFamilies.Count;
        _view.LoadingState(false);
      });
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

    #region event handling
    private async void Connection_OnCatalogUpdateFinished() {
      Console.WriteLine("Catalog update finished");
      await Application.Storage.Save();
      ShowLoadedState();
    }
    #endregion
  }
}
