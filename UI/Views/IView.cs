using System;
using System.Windows;

namespace UI.Views {

  public delegate void OnExitHandler();
  public delegate void OnLogoutHandler();
  public delegate void OnAboutClickedHandler();

  interface IView {
    UIElement DragHandle { get; }
    void InvokeOnUIThread(Action action);
  }
}
