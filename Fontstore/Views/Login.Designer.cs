namespace Fontstore {
  partial class Login {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.Header = new System.Windows.Forms.Panel();
      this.Logo = new System.Windows.Forms.PictureBox();
      this.Menu = new System.Windows.Forms.MenuStrip();
      this.Welcome = new System.Windows.Forms.ToolStripMenuItem();
      this.visitFontstoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.Header.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.Logo)).BeginInit();
      this.Menu.SuspendLayout();
      this.SuspendLayout();
      // 
      // Header
      // 
      this.Header.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.Header.AutoSize = true;
      this.Header.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.Header.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(255)))));
      this.Header.Controls.Add(this.Logo);
      this.Header.Controls.Add(this.Menu);
      this.Header.Location = new System.Drawing.Point(0, 0);
      this.Header.MinimumSize = new System.Drawing.Size(330, 70);
      this.Header.Name = "Header";
      this.Header.Size = new System.Drawing.Size(330, 70);
      this.Header.TabIndex = 0;
      // 
      // Logo
      // 
      this.Logo.Image = global::Fontstore.Properties.Resources.Logo;
      this.Logo.Location = new System.Drawing.Point(14, 42);
      this.Logo.Name = "Logo";
      this.Logo.Size = new System.Drawing.Size(110, 20);
      this.Logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.Logo.TabIndex = 0;
      this.Logo.TabStop = false;
      // 
      // Menu
      // 
      this.Menu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.Menu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(255)))));
      this.Menu.Dock = System.Windows.Forms.DockStyle.None;
      this.Menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Welcome});
      this.Menu.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
      this.Menu.Location = new System.Drawing.Point(242, 38);
      this.Menu.Name = "Menu";
      this.Menu.Padding = new System.Windows.Forms.Padding(0);
      this.Menu.Size = new System.Drawing.Size(73, 24);
      this.Menu.Stretch = false;
      this.Menu.TabIndex = 1;
      this.Menu.Text = "Welcome";
      // 
      // Welcome
      // 
      this.Welcome.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.Welcome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(255)))));
      this.Welcome.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.visitFontstoreToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.toolStripSeparator1,
            this.quitToolStripMenuItem});
      this.Welcome.Image = global::Fontstore.Properties.Resources.WhiteTriangle;
      this.Welcome.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.Welcome.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.Welcome.Name = "Welcome";
      this.Welcome.Padding = new System.Windows.Forms.Padding(0);
      this.Welcome.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.Welcome.Size = new System.Drawing.Size(71, 24);
      this.Welcome.Text = "Welcome";
      // 
      // visitFontstoreToolStripMenuItem
      // 
      this.visitFontstoreToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
      this.visitFontstoreToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.visitFontstoreToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
      this.visitFontstoreToolStripMenuItem.Name = "visitFontstoreToolStripMenuItem";
      this.visitFontstoreToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
      this.visitFontstoreToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.visitFontstoreToolStripMenuItem.Size = new System.Drawing.Size(152, 20);
      this.visitFontstoreToolStripMenuItem.Text = "Visit Fontstore";
      this.visitFontstoreToolStripMenuItem.Click += new System.EventHandler(this.visitFontstoreToolStripMenuItem_Click);
      // 
      // helpToolStripMenuItem
      // 
      this.helpToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
      this.helpToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.helpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
      this.helpToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.helpToolStripMenuItem.Size = new System.Drawing.Size(152, 20);
      this.helpToolStripMenuItem.Text = "Help";
      this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
      // 
      // aboutToolStripMenuItem
      // 
      this.aboutToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
      this.aboutToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.aboutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
      this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
      this.aboutToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
      this.aboutToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 20);
      this.aboutToolStripMenuItem.Text = "About";
      this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.BackColor = System.Drawing.SystemColors.Control;
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
      // 
      // quitToolStripMenuItem
      // 
      this.quitToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
      this.quitToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.quitToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
      this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
      this.quitToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
      this.quitToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.quitToolStripMenuItem.Size = new System.Drawing.Size(152, 20);
      this.quitToolStripMenuItem.Text = "Quit";
      this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
      // 
      // Login
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(330, 600);
      this.Controls.Add(this.Header);
      this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.MaximumSize = new System.Drawing.Size(330, 600);
      this.MinimumSize = new System.Drawing.Size(330, 600);
      this.Name = "Login";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Form1";
      this.Header.ResumeLayout(false);
      this.Header.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.Logo)).EndInit();
      this.Menu.ResumeLayout(false);
      this.Menu.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Panel Header;
    private System.Windows.Forms.PictureBox Logo;
    private System.Windows.Forms.MenuStrip Menu;
    private System.Windows.Forms.ToolStripMenuItem Welcome;
    private System.Windows.Forms.ToolStripMenuItem visitFontstoreToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
  }
}

