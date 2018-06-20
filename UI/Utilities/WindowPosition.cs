using System.Windows;

namespace UI.Utilities {
  class WindowPosition {
    #region private data
    private Window _win;
    #endregion

    #region properties
    public double Top {
      get {
        return _win.Top;
      }
    }
    public double Left {
      get {
        return _win.Left;
      }
    }
    #endregion

    #region ctor
    private WindowPosition(Window win) {
      _win = win;
    }
    #endregion

    #region factory
    public static WindowPosition FromWindow(Window win) {
      return new WindowPosition(win);
    }
    #endregion

    #region methods
    public void ApplyToWindow(Window win) {
      win.Left = Left;
      win.Top = Top;
    }
    #endregion
  }
}
