using System.Windows;
using System.Windows.Controls;

namespace UI.Controls {
  /// <summary>
  /// Interaction logic for RadioCheckbox.xaml
  /// </summary>
  public partial class RadioCheckbox : UserControl {
    public RadioCheckbox() {
      InitializeComponent();
    }

    public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
            "IsChecked",
            typeof(bool),
            typeof(RadioCheckbox));

    public bool IsChecked {
      get { return (bool)GetValue(IsCheckedProperty); }
      set { if (value != IsChecked) SetValue(IsCheckedProperty, value); }
    }
  }
}
