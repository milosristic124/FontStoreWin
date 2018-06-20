using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UILib {
  /// <summary>
  /// Interaction logic for CustomTextBox.xaml
  /// </summary>
  public partial class CustomTextBox : UserControl {
    public string Placeholder { get; set; }
    public Brush PlaceholderBrush { get; set; }
    public Brush TextBrush { get; set; }

    public CustomTextBox() {
      InitializeComponent();

      textBox.GotFocus += TextBox_GotFocus;
      textBox.LostFocus += TextBox_LostFocus;
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (string.IsNullOrWhiteSpace(textBox.Text)) {
        textBox.Text = Placeholder;
        textBox.Foreground = PlaceholderBrush;
      }
    }

    private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
      if (textBox.Text == Placeholder) {
        textBox.Text = "";
        textBox.Foreground = TextBrush;
      }
    }
  }
}
