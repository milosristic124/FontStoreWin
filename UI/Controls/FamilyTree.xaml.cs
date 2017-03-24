using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
  /// Interaction logic for FamilyTree.xaml
  /// </summary>
  public partial class FamilyTree : UserControl {
    #region properties
    public IEnumerable ItemsSource {
      get { return (IEnumerable)GetValue(FamiliesProperty); }
      set { SetValue(FamiliesProperty, value); }
    }
    #endregion

    #region dependency properties
    public static readonly DependencyProperty FamiliesProperty = DependencyProperty.Register(
      "Families",
      typeof(IEnumerable),
      typeof(FamilyTree),
      new PropertyMetadata(OnFamiliesPropertyChanged)
    );
    #endregion

    #region ctor
    public FamilyTree() {
      InitializeComponent();
    }
    #endregion

    #region private methods
    private static void OnFamiliesPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
      Console.WriteLine("OnFamiliesPropertyChanged");
      FamilyTree tree = sender as FamilyTree;
      if (tree != null) {
        Console.WriteLine("Calling [OnItemSourceChanged]");
        tree.OnItemSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
      }
    }

    private void OnItemSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
      Console.WriteLine("OnItemSourceChanged");
      INotifyCollectionChanged oldNotifyingValue = oldValue as INotifyCollectionChanged;
      if (oldNotifyingValue != null) {
        Console.WriteLine("Unsubscribe old value");
        oldNotifyingValue.CollectionChanged -= OnCollectionChanged;
      }

      INotifyCollectionChanged newNotifyingValue = newValue as INotifyCollectionChanged;
      if (newNotifyingValue != null) {
        Console.WriteLine("Subscribe new value");
        newNotifyingValue.CollectionChanged += OnCollectionChanged;
      }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    }
    #endregion
  }
}
