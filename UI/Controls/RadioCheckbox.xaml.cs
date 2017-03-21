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

namespace UI.Controls {
  /// <summary>
  /// Interaction logic for RadioCheckbox.xaml
  /// </summary>
  public partial class RadioCheckbox : UserControl {
    public RadioCheckbox() {
      InitializeComponent();
    }

    public bool Checked { get; set; }

    private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
      Checked = !Checked;
      if (Checked) {
        //Tick.Visibility = Visibility.Visible;
        (Tick.Fill as SolidColorBrush).Color = (Color)FindResource("FSBlack");
      } else {
        //Tick.Visibility = Visibility.Hidden;
        (Tick.Fill as SolidColorBrush).Color = (Color)FindResource("FSGrey");
      }
    }
  }
}
