using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI.Controls {
  /// <summary>
  /// Interaction logic for RadioCheckbox.xaml
  /// </summary>
  public partial class RememberMeButton : UserControl {
    public RememberMeButton() {
      InitializeComponent();
    }

    public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
            "RememberMeButton.IsChecked",
            typeof(bool),
            typeof(RememberMeButton));

    public bool IsChecked {
      get { return (bool)GetValue(IsCheckedProperty); }
      set { if (value != IsChecked) SetValue(IsCheckedProperty, value); }
    }
  }
}
