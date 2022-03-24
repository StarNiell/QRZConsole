
namespace QRZTestApp
{
    partial class frmTest
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnOrderDateDesc = new System.Windows.Forms.Button();
            this.btnOrderDateAsc = new System.Windows.Forms.Button();
            this.btnPageDown = new System.Windows.Forms.Button();
            this.btnCurrentPage = new System.Windows.Forms.Button();
            this.btnPageUp = new System.Windows.Forms.Button();
            this.btnGetTableContentXML = new System.Windows.Forms.Button();
            this.btnGetTableContentRaw = new System.Windows.Forms.Button();
            this.txtLogbookPage = new System.Windows.Forms.TextBox();
            this.btnGotoPage = new System.Windows.Forms.Button();
            this.btnLogbookPages = new System.Windows.Forms.Button();
            this.btnQSOCount = new System.Windows.Forms.Button();
            this.btnGotoLookbook = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnLookupAndCheck = new System.Windows.Forms.Button();
            this.btnCheckWorked = new System.Windows.Forms.Button();
            this.txtQRZLookup = new System.Windows.Forms.TextBox();
            this.btnLookup = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnClearMonitor = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnQRZHome = new System.Windows.Forms.Button();
            this.btnLogOut = new System.Windows.Forms.Button();
            this.btnIsLogged = new System.Windows.Forms.Button();
            this.txtMonitor = new System.Windows.Forms.TextBox();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.btnSwitchView = new System.Windows.Forms.Button();
            this.btnCommandList = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(20);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(940, 697);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(932, 671);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Test";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnSwitchView);
            this.splitContainer1.Panel1.Controls.Add(this.txtCommand);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox3);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtMonitor);
            this.splitContainer1.Size = new System.Drawing.Size(926, 665);
            this.splitContainer1.SplitterDistance = 225;
            this.splitContainer1.TabIndex = 1;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnOrderDateDesc);
            this.groupBox3.Controls.Add(this.btnOrderDateAsc);
            this.groupBox3.Controls.Add(this.btnPageDown);
            this.groupBox3.Controls.Add(this.btnCurrentPage);
            this.groupBox3.Controls.Add(this.btnPageUp);
            this.groupBox3.Controls.Add(this.btnGetTableContentXML);
            this.groupBox3.Controls.Add(this.btnGetTableContentRaw);
            this.groupBox3.Controls.Add(this.txtLogbookPage);
            this.groupBox3.Controls.Add(this.btnGotoPage);
            this.groupBox3.Controls.Add(this.btnLogbookPages);
            this.groupBox3.Controls.Add(this.btnQSOCount);
            this.groupBox3.Controls.Add(this.btnGotoLookbook);
            this.groupBox3.Location = new System.Drawing.Point(401, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(518, 182);
            this.groupBox3.TabIndex = 40;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Logbook";
            // 
            // btnOrderDateDesc
            // 
            this.btnOrderDateDesc.Location = new System.Drawing.Point(116, 117);
            this.btnOrderDateDesc.Name = "btnOrderDateDesc";
            this.btnOrderDateDesc.Size = new System.Drawing.Size(95, 31);
            this.btnOrderDateDesc.TabIndex = 20;
            this.btnOrderDateDesc.Text = "Date Desc";
            this.btnOrderDateDesc.UseVisualStyleBackColor = true;
            this.btnOrderDateDesc.Click += new System.EventHandler(this.btnOrderDateDesc_Click);
            // 
            // btnOrderDateAsc
            // 
            this.btnOrderDateAsc.Location = new System.Drawing.Point(19, 117);
            this.btnOrderDateAsc.Name = "btnOrderDateAsc";
            this.btnOrderDateAsc.Size = new System.Drawing.Size(91, 31);
            this.btnOrderDateAsc.TabIndex = 19;
            this.btnOrderDateAsc.Text = "Date Asc";
            this.btnOrderDateAsc.UseVisualStyleBackColor = true;
            this.btnOrderDateAsc.Click += new System.EventHandler(this.btnOrderDateAsc_Click);
            // 
            // btnPageDown
            // 
            this.btnPageDown.Location = new System.Drawing.Point(217, 73);
            this.btnPageDown.Name = "btnPageDown";
            this.btnPageDown.Size = new System.Drawing.Size(21, 31);
            this.btnPageDown.TabIndex = 16;
            this.btnPageDown.Text = "-";
            this.btnPageDown.UseVisualStyleBackColor = true;
            this.btnPageDown.Click += new System.EventHandler(this.btnPageDown_Click);
            // 
            // btnCurrentPage
            // 
            this.btnCurrentPage.Location = new System.Drawing.Point(116, 73);
            this.btnCurrentPage.Name = "btnCurrentPage";
            this.btnCurrentPage.Size = new System.Drawing.Size(95, 31);
            this.btnCurrentPage.TabIndex = 15;
            this.btnCurrentPage.Text = "Current page";
            this.btnCurrentPage.UseVisualStyleBackColor = true;
            this.btnCurrentPage.Click += new System.EventHandler(this.btnCurrentPage_Click);
            // 
            // btnPageUp
            // 
            this.btnPageUp.Location = new System.Drawing.Point(289, 73);
            this.btnPageUp.Name = "btnPageUp";
            this.btnPageUp.Size = new System.Drawing.Size(21, 31);
            this.btnPageUp.TabIndex = 18;
            this.btnPageUp.Text = "+";
            this.btnPageUp.UseVisualStyleBackColor = true;
            this.btnPageUp.Click += new System.EventHandler(this.btnPageUp_Click);
            // 
            // btnGetTableContentXML
            // 
            this.btnGetTableContentXML.Location = new System.Drawing.Point(347, 30);
            this.btnGetTableContentXML.Name = "btnGetTableContentXML";
            this.btnGetTableContentXML.Size = new System.Drawing.Size(153, 31);
            this.btnGetTableContentXML.TabIndex = 21;
            this.btnGetTableContentXML.Text = "Get Table Content XML";
            this.btnGetTableContentXML.UseVisualStyleBackColor = true;
            this.btnGetTableContentXML.Click += new System.EventHandler(this.btnGetTableContentXML_Click);
            // 
            // btnGetTableContentRaw
            // 
            this.btnGetTableContentRaw.Location = new System.Drawing.Point(347, 73);
            this.btnGetTableContentRaw.Name = "btnGetTableContentRaw";
            this.btnGetTableContentRaw.Size = new System.Drawing.Size(153, 31);
            this.btnGetTableContentRaw.TabIndex = 22;
            this.btnGetTableContentRaw.Text = "Get Table Content Raw";
            this.btnGetTableContentRaw.UseVisualStyleBackColor = true;
            this.btnGetTableContentRaw.Click += new System.EventHandler(this.btnGetTableContentRaw_Click);
            // 
            // txtLogbookPage
            // 
            this.txtLogbookPage.Location = new System.Drawing.Point(244, 79);
            this.txtLogbookPage.Name = "txtLogbookPage";
            this.txtLogbookPage.Size = new System.Drawing.Size(39, 20);
            this.txtLogbookPage.TabIndex = 17;
            this.txtLogbookPage.Text = "1";
            this.txtLogbookPage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnGotoPage
            // 
            this.btnGotoPage.Location = new System.Drawing.Point(217, 30);
            this.btnGotoPage.Name = "btnGotoPage";
            this.btnGotoPage.Size = new System.Drawing.Size(93, 31);
            this.btnGotoPage.TabIndex = 13;
            this.btnGotoPage.Text = "Goto Page";
            this.btnGotoPage.UseVisualStyleBackColor = true;
            this.btnGotoPage.Click += new System.EventHandler(this.btnGotoPage_Click);
            // 
            // btnLogbookPages
            // 
            this.btnLogbookPages.Location = new System.Drawing.Point(116, 30);
            this.btnLogbookPages.Name = "btnLogbookPages";
            this.btnLogbookPages.Size = new System.Drawing.Size(95, 31);
            this.btnLogbookPages.TabIndex = 12;
            this.btnLogbookPages.Text = "Logbook Pages";
            this.btnLogbookPages.UseVisualStyleBackColor = true;
            this.btnLogbookPages.Click += new System.EventHandler(this.btnLogbookPages_Click);
            // 
            // btnQSOCount
            // 
            this.btnQSOCount.Location = new System.Drawing.Point(19, 73);
            this.btnQSOCount.Name = "btnQSOCount";
            this.btnQSOCount.Size = new System.Drawing.Size(91, 31);
            this.btnQSOCount.TabIndex = 14;
            this.btnQSOCount.Text = "QSO Count";
            this.btnQSOCount.UseVisualStyleBackColor = true;
            this.btnQSOCount.Click += new System.EventHandler(this.btnQSOCount_Click);
            // 
            // btnGotoLookbook
            // 
            this.btnGotoLookbook.Location = new System.Drawing.Point(19, 30);
            this.btnGotoLookbook.Name = "btnGotoLookbook";
            this.btnGotoLookbook.Size = new System.Drawing.Size(91, 31);
            this.btnGotoLookbook.TabIndex = 11;
            this.btnGotoLookbook.Text = "Goto Logbook";
            this.btnGotoLookbook.UseVisualStyleBackColor = true;
            this.btnGotoLookbook.Click += new System.EventHandler(this.btnGotoLookbook_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnLookupAndCheck);
            this.groupBox2.Controls.Add(this.btnCheckWorked);
            this.groupBox2.Controls.Add(this.txtQRZLookup);
            this.groupBox2.Controls.Add(this.btnLookup);
            this.groupBox2.Location = new System.Drawing.Point(281, 11);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(115, 183);
            this.groupBox2.TabIndex = 37;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Loookup";
            // 
            // btnLookupAndCheck
            // 
            this.btnLookupAndCheck.Location = new System.Drawing.Point(10, 119);
            this.btnLookupAndCheck.Name = "btnLookupAndCheck";
            this.btnLookupAndCheck.Size = new System.Drawing.Size(94, 54);
            this.btnLookupAndCheck.TabIndex = 10;
            this.btnLookupAndCheck.Text = "Lookup and Check Worked";
            this.btnLookupAndCheck.UseVisualStyleBackColor = true;
            this.btnLookupAndCheck.Click += new System.EventHandler(this.btnLookupAndCheck_Click);
            // 
            // btnCheckWorked
            // 
            this.btnCheckWorked.Location = new System.Drawing.Point(10, 90);
            this.btnCheckWorked.Name = "btnCheckWorked";
            this.btnCheckWorked.Size = new System.Drawing.Size(94, 27);
            this.btnCheckWorked.TabIndex = 9;
            this.btnCheckWorked.Text = "Check Worked";
            this.btnCheckWorked.UseVisualStyleBackColor = true;
            this.btnCheckWorked.Click += new System.EventHandler(this.btnCheckWorked_Click);
            // 
            // txtQRZLookup
            // 
            this.txtQRZLookup.Location = new System.Drawing.Point(10, 37);
            this.txtQRZLookup.Name = "txtQRZLookup";
            this.txtQRZLookup.Size = new System.Drawing.Size(94, 20);
            this.txtQRZLookup.TabIndex = 7;
            this.txtQRZLookup.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtQRZLookup_KeyDown);
            // 
            // btnLookup
            // 
            this.btnLookup.Location = new System.Drawing.Point(10, 61);
            this.btnLookup.Name = "btnLookup";
            this.btnLookup.Size = new System.Drawing.Size(94, 27);
            this.btnLookup.TabIndex = 8;
            this.btnLookup.Text = "Lookup";
            this.btnLookup.UseVisualStyleBackColor = true;
            this.btnLookup.Click += new System.EventHandler(this.btnLookup_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnCommandList);
            this.groupBox1.Controls.Add(this.btnClearMonitor);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtPassword);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtUsername);
            this.groupBox1.Controls.Add(this.btnLogin);
            this.groupBox1.Controls.Add(this.btnQRZHome);
            this.groupBox1.Controls.Add(this.btnLogOut);
            this.groupBox1.Controls.Add(this.btnIsLogged);
            this.groupBox1.Location = new System.Drawing.Point(16, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 183);
            this.groupBox1.TabIndex = 36;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "QRZ.COM Startup";
            // 
            // btnClearMonitor
            // 
            this.btnClearMonitor.Location = new System.Drawing.Point(17, 147);
            this.btnClearMonitor.Name = "btnClearMonitor";
            this.btnClearMonitor.Size = new System.Drawing.Size(94, 26);
            this.btnClearMonitor.TabIndex = 23;
            this.btnClearMonitor.Text = "Clear Monitor";
            this.btnClearMonitor.UseVisualStyleBackColor = true;
            this.btnClearMonitor.Click += new System.EventHandler(this.btnClearMonitor_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 40;
            this.label2.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(93, 97);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(70, 20);
            this.txtPassword.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 38;
            this.label1.Text = "Username";
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(93, 71);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(70, 20);
            this.txtUsername.TabIndex = 4;
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(169, 68);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(70, 49);
            this.btnLogin.TabIndex = 6;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnQRZHome
            // 
            this.btnQRZHome.Location = new System.Drawing.Point(17, 31);
            this.btnQRZHome.Name = "btnQRZHome";
            this.btnQRZHome.Size = new System.Drawing.Size(70, 31);
            this.btnQRZHome.TabIndex = 1;
            this.btnQRZHome.Text = "QRZ Home";
            this.btnQRZHome.UseVisualStyleBackColor = true;
            this.btnQRZHome.Click += new System.EventHandler(this.btnQRZHome_Click);
            // 
            // btnLogOut
            // 
            this.btnLogOut.Location = new System.Drawing.Point(169, 31);
            this.btnLogOut.Name = "btnLogOut";
            this.btnLogOut.Size = new System.Drawing.Size(70, 31);
            this.btnLogOut.TabIndex = 3;
            this.btnLogOut.Text = "Log Out";
            this.btnLogOut.UseVisualStyleBackColor = true;
            this.btnLogOut.Click += new System.EventHandler(this.btnLogOut_Click);
            // 
            // btnIsLogged
            // 
            this.btnIsLogged.Location = new System.Drawing.Point(93, 31);
            this.btnIsLogged.Name = "btnIsLogged";
            this.btnIsLogged.Size = new System.Drawing.Size(70, 31);
            this.btnIsLogged.TabIndex = 2;
            this.btnIsLogged.Text = "Is Logged";
            this.btnIsLogged.UseVisualStyleBackColor = true;
            this.btnIsLogged.Click += new System.EventHandler(this.btnIsLogged_Click);
            // 
            // txtMonitor
            // 
            this.txtMonitor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMonitor.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMonitor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.txtMonitor.Location = new System.Drawing.Point(0, 0);
            this.txtMonitor.Multiline = true;
            this.txtMonitor.Name = "txtMonitor";
            this.txtMonitor.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMonitor.Size = new System.Drawing.Size(922, 432);
            this.txtMonitor.TabIndex = 24;
            this.txtMonitor.WordWrap = false;
            // 
            // txtCommand
            // 
            this.txtCommand.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtCommand.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtCommand.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCommand.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.txtCommand.Location = new System.Drawing.Point(0, 201);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new System.Drawing.Size(922, 20);
            this.txtCommand.TabIndex = 41;
            this.txtCommand.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCommand_KeyDown);
            // 
            // btnSwitchView
            // 
            this.btnSwitchView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSwitchView.Location = new System.Drawing.Point(902, 199);
            this.btnSwitchView.Name = "btnSwitchView";
            this.btnSwitchView.Size = new System.Drawing.Size(21, 21);
            this.btnSwitchView.TabIndex = 42;
            this.btnSwitchView.Text = "-";
            this.btnSwitchView.UseVisualStyleBackColor = true;
            this.btnSwitchView.Click += new System.EventHandler(this.btnSwitchView_Click);
            // 
            // btnCommandList
            // 
            this.btnCommandList.Location = new System.Drawing.Point(145, 146);
            this.btnCommandList.Name = "btnCommandList";
            this.btnCommandList.Size = new System.Drawing.Size(94, 26);
            this.btnCommandList.TabIndex = 41;
            this.btnCommandList.Text = "Command List";
            this.btnCommandList.UseVisualStyleBackColor = true;
            this.btnCommandList.Click += new System.EventHandler(this.btnCommandList_Click);
            // 
            // frmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(940, 697);
            this.Controls.Add(this.tabControl1);
            this.KeyPreview = true;
            this.Name = "frmTest";
            this.Text = "QRZ Library Test App";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmTest_KeyDown);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnGotoLookbook;
        private System.Windows.Forms.Button btnQSOCount;
        private System.Windows.Forms.Button btnLogbookPages;
        private System.Windows.Forms.TextBox txtMonitor;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnPageDown;
        private System.Windows.Forms.Button btnCurrentPage;
        private System.Windows.Forms.Button btnPageUp;
        private System.Windows.Forms.Button btnGetTableContentXML;
        private System.Windows.Forms.Button btnGetTableContentRaw;
        private System.Windows.Forms.TextBox txtLogbookPage;
        private System.Windows.Forms.Button btnGotoPage;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtQRZLookup;
        private System.Windows.Forms.Button btnLookup;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnQRZHome;
        private System.Windows.Forms.Button btnLogOut;
        private System.Windows.Forms.Button btnIsLogged;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Button btnCheckWorked;
        private System.Windows.Forms.Button btnLookupAndCheck;
        private System.Windows.Forms.Button btnClearMonitor;
        private System.Windows.Forms.Button btnOrderDateDesc;
        private System.Windows.Forms.Button btnOrderDateAsc;
        private System.Windows.Forms.TextBox txtCommand;
        private System.Windows.Forms.Button btnSwitchView;
        private System.Windows.Forms.Button btnCommandList;
    }
}

