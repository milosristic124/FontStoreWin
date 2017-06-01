using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using Utilities;
using Utilities.FSM;

namespace UI {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {
    #region private data
    private FiniteStateMachine<States.UIState> _ui;

    private static readonly string RegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
    private static readonly string RegistryValue = "Fontstore";
    #endregion

    #region properies
    public Core.ApplicationContext Context { get; private set; }
    public System.Windows.Forms.NotifyIcon NotifyIcon { get; private set; }
    #endregion

    #region ctor
    public App() {
      _ui = new FiniteStateMachine<States.UIState>(new States.NoUI(this), true);
      _ui.Start();

      InitializeNotificationIcon();

      _wasDragged = false;
      _dragStart = new Point(0, 0);

      Deactivated += App_Deactivated;
#if DEBUG
      Activated += delegate {
        Console.WriteLine("[{0}] App activated", DateTime.Now.ToString("hh:mm:ss.fff"));
      };
#endif

      Context = Core.Factory.InitializeApplicationContext();
      _ui.State = new States.Login(this);
      _ui.State.Show();
    }
    #endregion

    #region about popup
    public void ShowAboutPopup(Window parent) {
      string aboutTxt = $"Fontstore v{Constants.App.ApplicationVersion}\n" +
        $"Fontstore all rights reserved - © 2017";
      MessageBox.Show(parent, aboutTxt, "About Fontstore", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
    }
    #endregion

    #region drag & drop
    public void SetDragHandle(UIElement dragHandle) {
      dragHandle.PreviewMouseLeftButtonDown += DragClickHandler;
      dragHandle.PreviewMouseMove += DragHandler;
    }

    private Point _dragStart;
    private void DragClickHandler(object sender, MouseButtonEventArgs e) {
      _dragStart = e.GetPosition(null);
    }

    private bool _wasDragged = false;
    private void DragHandler(object sender, MouseEventArgs e) {
      if (e.LeftButton == MouseButtonState.Pressed) {
        Point mousePos = e.GetPosition(null);
        Vector diff = _dragStart - mousePos;

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
            || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) {
          _wasDragged = true;
          MainWindow.DragMove();
        }
      }
    }
    #endregion

    #region event handling
    private void App_Deactivated(object sender, EventArgs e) {
#if DEBUG
      Console.WriteLine("[{0}] App deactivated", DateTime.Now.ToString("hh:mm:ss.fff"));
#endif
      if (!_wasDragged) { // don't close the window when clicking outside if it was dragged elsewhere
        _ui.State.Hide();
      }
    }

    private void NotifyIcon_Click(object sender, EventArgs e) {
      if (_wasDragged && _ui.State.IsShown) {
        _ui.State.Hide();
        _ui.State.ResetWindowPosition();
        _wasDragged = false;
      }
      else if (!_ui.State.IsShown) {
        _ui.State.Show();
        MainWindow.Activate();
      }
    }
    #endregion

    #region private methods
    private void InitializeNotificationIcon() {
      NotifyIcon = new System.Windows.Forms.NotifyIcon();
      NotifyIcon.Text = "Fontstore";
      NotifyIcon.Icon = UI.Properties.Resources.NotifIcon_54;
      NotifyIcon.Visible = true;

      NotifyIcon.Click += NotifyIcon_Click;
    }

    private void RemoveNotificationIcon() {
      NotifyIcon.Click -= NotifyIcon_Click;
      NotifyIcon.Visible = false;
      NotifyIcon.Icon = null;
      NotifyIcon = null;
    }
    #endregion

    #region private static methods
    private static void SetAsStartupItem() {
      RegistryKey rk = Registry.CurrentUser.OpenSubKey(RegistryKey, true);

      if (rk.GetValue(RegistryValue) == null) {
        rk.SetValue(RegistryValue, System.Reflection.Assembly.GetExecutingAssembly().Location);
      }
    }
    #endregion

    private void Application_Exit(object sender, ExitEventArgs e) {
      RemoveNotificationIcon();
    }
  }
}
