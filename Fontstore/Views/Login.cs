using System;
using System.Windows.Forms;
using UI.Utilities;

namespace Fontstore {
  public partial class Login : Form {
    #region ctor
    public Login() {
      InitializeComponent();

      double top, left;
      TaskBarLocationProvider.CalculateWindowPositionByTaskbar(Width, Height, out left, out top);
      Top = (int)Math.Floor(top);
      Left = (int)Math.Floor(left);

      ViewsUtility.SetDragHandle(this, Header);
    }
    #endregion

    #region Menu actions
    private void quitToolStripMenuItem_Click(object sender, EventArgs e) {
      Application.Exit();
    }

    private void visitFontstoreToolStripMenuItem_Click(object sender, EventArgs e) {
      ViewsUtility.NavigateToUri(new Uri("https://www.fontstore.com"));
    }

    private void helpToolStripMenuItem_Click(object sender, EventArgs e) {
      ViewsUtility.NavigateToUri(new Uri("https://www.fontstore.com/help"));
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
      ViewsUtility.ShowAboutPopup();
    }
    #endregion
  }
}
