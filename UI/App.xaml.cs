using Protocol;
using Storage;
using System.Windows;
using Utilities.FSM;
using System;

namespace UI {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {
    #region private data
    private FiniteStateMachine<States.UIState> _ui;
    #endregion

    #region properies
    public IFontStorage Storage { get; private set; }
    public IConnection Connection { get; private set; }
    public System.Windows.Forms.NotifyIcon NotifyIcon { get; private set; }
    #endregion

    #region ctor
    public App() {
      _ui = new FiniteStateMachine<States.UIState>(new States.NoUI(this));
      _ui.Start();

      Storage = Core.Factory.InitializeStorage();
      Connection = Core.Factory.InitializeServerConnection(Storage);

      InitializeNotificationIcon();

      Deactivated += App_Deactivated;

      _ui.State = new States.Login(this);
    }
    #endregion

    #region event handling
    private void App_Deactivated(object sender, EventArgs e) {
      _ui.State.Hide();
    }

    private void NotifyIcon_Click(object sender, EventArgs e) {
      if (!_ui.State.IsShown) {
        _ui.State.Show();
        MainWindow.Activate();
      }
    }
    #endregion

    #region private methods
    private void InitializeNotificationIcon() {
      NotifyIcon = new System.Windows.Forms.NotifyIcon();
      NotifyIcon.Text = "Fontstore";
      NotifyIcon.Icon = UI.Properties.Resources.NotifIcon;
      NotifyIcon.Visible = true;

      NotifyIcon.Click += NotifyIcon_Click;
    }
    #endregion
  }
}
