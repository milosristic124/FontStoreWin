using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI.Controls {
  /// <summary>
  /// Interaction logic for SearchButton.xaml
  /// </summary>
  public partial class SearchButton : UserControl {
    public SearchButton() {
      InitializeComponent();
    }

    public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
            "SearchButton.IsChecked",
            typeof(bool),
            typeof(RememberMeButton));

    public bool IsChecked {
      get { return (bool)GetValue(IsCheckedProperty); }
      set { if (value != IsChecked) SetValue(IsCheckedProperty, value); }
    }
  }
}
