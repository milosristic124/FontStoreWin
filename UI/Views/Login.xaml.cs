using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using UI.Utilities;

namespace UI.Views {
  /// <summary>
  /// Interaction logic for Login.xaml
  /// </summary>
  public partial class Login : Window, IView {
    #region private data
    private static readonly string EmailPlaceholder = "Email Address";

    //private App _application;
    private bool _connecting;

    private bool LoginEnabled {
      get {
        return LoginButton.IsEnabled;
      }
      set {
        if (IsInitialized) {
          LoginButton.IsEnabled = value;
        }
      }
    }

    private string Email {
      get {
        if (!IsInitialized) return "";

        string _email = LoginInput.Text.Trim();
        return _email == EmailPlaceholder ? "" : _email;
      }
    }

    private string Password {
      get {
        if (!IsInitialized) return "";

        return PasswordInput.Password.Trim();
      }
    }
    #endregion

    #region delegates
    public delegate void OnConnectHandler(string email, string password, bool saveCredentials);
    #endregion

    #region events
    public event OnExitHandler OnExit;
    public event OnConnectHandler OnConnect;
    #endregion

    #region properties
    public UIElement DragHandle {
      get {
        if (IsInitialized) {
          return HeaderGrid;
        }
        return null;
      }
    }
    #endregion

    #region ctor
    public Login() {
      InitializeComponent();
      _connecting = false;

      // Set the login input placeholder
      LoginInput.Text = EmailPlaceholder;
    }
    #endregion

    #region methods
    public void ConnectionRequestStarted() {
      _connecting = true;
      DisableUserInputs();
      ConnectingBar.Visibility = Visibility.Visible;
    }

    public void ConnectionRequestFailed(string error) {
      _connecting = false;
      ConnectingBar.Visibility = Visibility.Hidden;
      MessageBox.Show(error, "Fontstore - Connection failed", MessageBoxButton.OK);
      EnableUserInputs();
    }

    public void InvokeOnUIThread(Action action) {
      Dispatcher.Invoke(action);
    }
    #endregion

    #region UI event handling
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
      ViewsUtility.NavigateToUri(e.Uri);
      e.Handled = true;
    }

    private void LoginInput_GotFocus(object sender, RoutedEventArgs e) {
      if (LoginInput.Text == EmailPlaceholder) {
        LoginInput.Text = "";
      }
      LoginInput.Foreground = new SolidColorBrush((Color)FindResource("FSBlack"));
    }

    private void LoginInput_LostFocus(object sender, RoutedEventArgs e) {
      if (string.IsNullOrWhiteSpace(LoginInput.Text)) {
        LoginInput.Text = EmailPlaceholder;
        LoginInput.Foreground = new SolidColorBrush((Color)FindResource("FSGrey"));
      }
    }

    private void PasswordInput_LostFocus(object sender, RoutedEventArgs e) {
      if (string.IsNullOrWhiteSpace(PasswordInput.Password)) {
        PasswordLabel.Visibility = Visibility.Visible;
        PasswordInput.Opacity = 0;
      }
    }

    private void PasswordLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
      PasswordInput.Focus();
    }

    private void PasswordInput_GotFocus(object sender, RoutedEventArgs e) {
      PasswordLabel.Visibility = Visibility.Hidden;
      PasswordInput.Opacity = 1;
    }

    private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e) {
      LoginEnabled = LoginPossible();
    }

    private void LoginInput_TextChanged(object sender, TextChangedEventArgs e) {
      LoginEnabled = LoginPossible();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e) {
      Connect();
    }

    private void MenuButton_Click(object sender, RoutedEventArgs e) {
      MenuButton.ContextMenu.IsEnabled = true;
      MenuButton.ContextMenu.PlacementTarget = MenuButton;
      MenuButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
      MenuButton.ContextMenu.IsOpen = true;
    }

    private void Visit_Click(object sender, RoutedEventArgs e) {
      ViewsUtility.NavigateToUri(new Uri("http://fontstore.com"));
      e.Handled = true;
    }

    private void Help_Click(object sender, RoutedEventArgs e) {
      ViewsUtility.NavigateToUri(new Uri("http://fontstore.com/faq"));
      e.Handled = true;
    }

    private void Quit_Click(object sender, RoutedEventArgs e) {
      OnExit?.Invoke();
    }

    private void About_Click(object sender, RoutedEventArgs e) {
    }
    #endregion

    #region private methods
    private bool LoginPossible() {
      bool emailValid = !string.IsNullOrWhiteSpace(Email);
      bool passwordValid = !string.IsNullOrWhiteSpace(Password);

      return emailValid && passwordValid;
    }

    private void Connect() {
      if (LoginEnabled && !_connecting) {
        OnConnect?.Invoke(Email, Password, RememberCheck.Checked);
      }
    }

    private void DisableUserInputs() {
      LoginEnabled = false;
      LoginInput.IsEnabled = false;
      PasswordInput.IsEnabled = false;
      RememberCheck.IsEnabled = false;
    }

    private void EnableUserInputs() {
      LoginEnabled = true;
      LoginInput.IsEnabled = true;
      PasswordInput.IsEnabled = true;
      RememberCheck.IsEnabled = true;
    }
    #endregion
  }
}
