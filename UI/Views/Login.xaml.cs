using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace UI.Views {
  /// <summary>
  /// Interaction logic for Login.xaml
  /// </summary>
  public partial class Login : Window {
    #region private data
    private static readonly string EmailPlaceholder = "Email Address";

    private App _application;
    private bool _connecting;
    private Point _dragStartPos;

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

    #region ctor
    public Login() {
      InitializeComponent();

      _application = Application.Current as App;
      _connecting = false;

      // Set the login input placeholder
      LoginInput.Text = EmailPlaceholder;

      _application.SetDragHandle(HeaderGrid);
      _application.Connection.OnValidationFailure += Connection_OnValidationFailure;
      _application.Connection.OnEstablished += Connection_OnEstablished;
    }
    #endregion

    #region UI event handling
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
      URINavigate(e.Uri);
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
      URINavigate(new Uri("http://fontstore.com"));
      e.Handled = true;
    }

    private void Help_Click(object sender, RoutedEventArgs e) {
      URINavigate(new Uri("http://fontstore.com/faq"));
      e.Handled = true;
    }

    private void Quit_Click(object sender, RoutedEventArgs e) {
      _application.Connection.Disconnect();
      _application.Shutdown();
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
        _connecting = true;
        DisableUserInputs();
        ConnectingBar.Visibility = Visibility.Visible;
        _application.Connection.Connect(Email, Password);
      }
    }

    private void ShowConnectingUI() {
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

    private void URINavigate(Uri uri) {
      Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
    }
    #endregion

    #region connection event handling
    private void Connection_OnEstablished(Protocol.Payloads.UserData userData) {
      _connecting = false;
      bool saveCredentials = RememberCheck.Checked;
      // no need to enable user inputs, he won't need it were he is going (main screen)
      MessageBox.Show(string.Format("Welcome {0} {1}", userData.FirstName, userData.LastName), "Connection succeed", MessageBoxButton.OK);
    }

    private void Connection_OnValidationFailure(string reason) {
      Dispatcher.Invoke(() => {
        _connecting = false;
        ConnectingBar.Visibility = Visibility.Hidden;
        MessageBox.Show(reason, "Fontstore - Connection failed", MessageBoxButton.OK);
        EnableUserInputs();
      });
    }
    #endregion
  }
}
