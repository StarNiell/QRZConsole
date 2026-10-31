namespace QRZConsole
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        // Layout: ribbon (top), status bar (bottom), console (fill).
        // The ribbon content is built in code (frmMain.Ribbon.cs).
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.consolePanel = new System.Windows.Forms.Panel();
            this.console = new QRZConsole.UI.ConsoleView();
            this.ribbon = new QRZConsole.UI.RibbonBar();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLogin = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusQrz = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusPage = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusView = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusCluster = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBusy = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusTimer = new System.Windows.Forms.Timer(this.components);
            this.consolePanel.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // consolePanel
            //
            this.consolePanel.Controls.Add(this.console);
            this.consolePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consolePanel.Name = "consolePanel";
            this.consolePanel.Padding = new System.Windows.Forms.Padding(8, 6, 2, 2);
            this.consolePanel.TabIndex = 0;
            //
            // console
            //
            this.console.Dock = System.Windows.Forms.DockStyle.Fill;
            this.console.Name = "console";
            this.console.TabIndex = 0;
            //
            // ribbon
            //
            this.ribbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribbon.Name = "ribbon";
            this.ribbon.TabIndex = 1;
            this.ribbon.TabStop = false;
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLogin,
            this.statusQrz,
            this.statusPage,
            this.statusView,
            this.statusCluster,
            this.statusBusy});
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.SizingGrip = true;
            this.statusStrip.TabIndex = 2;
            //
            // status labels
            //
            this.statusLogin.Name = "statusLogin";
            this.statusLogin.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statusQrz.Name = "statusQrz";
            this.statusQrz.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statusPage.Name = "statusPage";
            this.statusPage.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statusView.Name = "statusView";
            this.statusView.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statusCluster.Name = "statusCluster";
            this.statusBusy.Name = "statusBusy";
            this.statusBusy.Spring = true;
            this.statusBusy.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // statusTimer
            //
            this.statusTimer.Interval = 1000;
            //
            // frmMain
            //
            // No automatic scaling: the ribbon measures itself in device pixels and the console font
            // is in points; the window size is adapted to the DPI in the constructor.
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1100, 720);
            this.Controls.Add(this.consolePanel);
            this.Controls.Add(this.ribbon);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = false;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QRZ Console";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.consolePanel.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel consolePanel;
        private QRZConsole.UI.ConsoleView console;
        private QRZConsole.UI.RibbonBar ribbon;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLogin;
        private System.Windows.Forms.ToolStripStatusLabel statusQrz;
        private System.Windows.Forms.ToolStripStatusLabel statusPage;
        private System.Windows.Forms.ToolStripStatusLabel statusView;
        private System.Windows.Forms.ToolStripStatusLabel statusCluster;
        private System.Windows.Forms.ToolStripStatusLabel statusBusy;
        private System.Windows.Forms.Timer statusTimer;
    }
}
