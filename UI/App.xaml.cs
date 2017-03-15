using Protocol;
using Storage;
using System.Windows;

namespace UI {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {
    #region properies
    public IFontStorage Storage { get; private set; }
    public IConnection Connection { get; private set; }
    #endregion

    #region ctor
    public App() {
      Storage = Core.Factory.InitializeStorage();
      Connection = Core.Factory.InitializeServerConnection(Storage);
    }
    #endregion
  }
}
