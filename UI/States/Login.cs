using Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using UI.Utilities;

namespace UI.States {
  class Login : UIState {
    #region data
    private Views.Login _view = null;
    private bool _saveCredentials = false;
    private string _tmpFile;
    private AppDirs _dirs;
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
      _dirs = null;
      _tmpFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Fontstore/data";

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

      _dirs = null;
      if (File.Exists(_tmpFile)) {
        _dirs = JsonConvert.DeserializeObject<AppDirs>(File.ReadAllText(_tmpFile));
        File.Delete(_tmpFile);

        Application.Context.FontInstaller.UserFontDir = _dirs.UserPath;
        Application.Context.FontInstaller.PrivateFontDir = _dirs.PrivatePath;

        _view.InvokeOnUIThread(() => {
          _view.ConnectionRequestStarted();
        });
        Application.Context.Connection.AutoConnect(_dirs.Token);
      } else {
        string savedCreds = await Application.Context.Storage.LoadCredentials();
        if (savedCreds != null) {
          Logger.Log("Credentials loaded");
          _view.InvokeOnUIThread(() => {
            _view.ConnectionRequestStarted();
          });
          Application.Context.Connection.AutoConnect(savedCreds);
        }
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
      Application.ApplicationActive();

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

      if (_dirs != null) {
        if (newFontCount > 0) {
          Application.ShowNotification($"{newFontCount} new fonts synchronized", System.Windows.Forms.ToolTipIcon.Info);
        }

        _view.InvokeOnUIThread(() => {
          WillTransition = true;
          FSM.State = new FontList(Application, WindowPosition.FromWindow(_view));
          FSM.State.Show();
          Dispose();
        });
      } else {
        AppDirs dirs = new AppDirs() {
          UserPath = Application.Context.FontInstaller.UserFontDir,
          PrivatePath = Application.Context.FontInstaller.PrivateFontDir,
          Token = Application.Context.Connection.UserData.AuthToken
        };
        File.WriteAllText(_tmpFile, JsonConvert.SerializeObject(dirs));

        string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        Process.Start(path);

        _view.InvokeOnUIThread(delegate {
          Application.Shutdown();
        });
      }
    }
    #endregion

    private class AppDirs {
      [JsonProperty("private_path")]
      public string PrivatePath { get; set; }
      [JsonProperty("user_path")]
      public string UserPath { get; set; }
      [JsonProperty("auth_token")]
      public string Token { get; set; }
    }
  }
}
