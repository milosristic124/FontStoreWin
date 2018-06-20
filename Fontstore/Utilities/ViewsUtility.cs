using System;
using System.Diagnostics;
using System.Windows.Forms;
using Utilities;

namespace UI.Utilities {
  static class ViewsUtility {
    public static void NavigateToUri(Uri uri) {
      Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
    }

    public static void ShowAboutPopup() {
      string aboutTxt = $"Fontstore Installer (v{Constants.App.ApplicationVersion})\n" +
        $"Copyright © 2017 - Fontstore Pte Ltd";
      MessageBox.Show(aboutTxt, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    #region drag & drop
    private static Control _window;

    public static void SetDragHandle(Control window, Control dragHandle) {
      _window = window;
      dragHandle.MouseDown += DragHandle_MouseDown;
    }

    private static void DragHandle_MouseDown(object sender, MouseEventArgs e) {
      if (e.Button == MouseButtons.Left) {
        ReleaseCapture();
        SendMessage(_window.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
      }
    }

    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HT_CAPTION = 0x2;

    [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
    private static extern bool ReleaseCapture();
    #endregion
  }
}
