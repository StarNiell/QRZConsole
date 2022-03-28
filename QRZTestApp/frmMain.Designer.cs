
namespace QRZConsole
{
    partial class frmMain
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnSwitchView = new System.Windows.Forms.Button();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnQSOforPage = new System.Windows.Forms.Button();
            this.btnOrderDateDesc = new System.Windows.Forms.Button();
            this.btnOrderDateAsc = new System.Windows.Forms.Button();
            this.btnPageDown = new System.Windows.Forms.Button();
            this.btnCurrentPage = new System.Windows.Forms.Button();
            this.btnPageUp = new System.Windows.Forms.Button();
            this.btnGetTableContentText = new System.Windows.Forms.Button();
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
            this.chkShowHeader = new System.Windows.Forms.CheckBox();
            this.chkIsLogged = new System.Windows.Forms.CheckBox();
            this.btnCommandList = new System.Windows.Forms.Button();
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
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer1.Size = new System.Drawing.Size(940, 691);
            this.splitContainer1.SplitterDistance = 232;
            this.splitContainer1.TabIndex = 1;
            // 
            // btnSwitchView
            // 
            this.btnSwitchView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSwitchView.Location = new System.Drawing.Point(916, 207);
            this.btnSwitchView.Name = "btnSwitchView";
            this.btnSwitchView.Size = new System.Drawing.Size(20, 20);
            this.btnSwitchView.TabIndex = 42;
            this.btnSwitchView.Text = "-";
            this.btnSwitchView.UseVisualStyleBackColor = true;
            this.btnSwitchView.Click += new System.EventHandler(this.btnSwitchView_Click);
            // 
            // txtCommand
            // 
            this.txtCommand.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtCommand.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtCommand.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCommand.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.txtCommand.Location = new System.Drawing.Point(0, 208);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new System.Drawing.Size(936, 20);
            this.txtCommand.TabIndex = 41;
            this.txtCommand.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCommand_KeyDown);
            this.txtCommand.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCommand_KeyPress);
            this.txtCommand.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtCommand_KeyUp);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnQSOforPage);
            this.groupBox3.Controls.Add(this.btnOrderDateDesc);
            this.groupBox3.Controls.Add(this.btnOrderDateAsc);
            this.groupBox3.Controls.Add(this.btnPageDown);
            this.groupBox3.Controls.Add(this.btnCurrentPage);
            this.groupBox3.Controls.Add(this.btnPageUp);
            this.groupBox3.Controls.Add(this.btnGetTableContentText);
            this.groupBox3.Controls.Add(this.btnGetTableContentXML);
            this.groupBox3.Controls.Add(this.btnGetTableContentRaw);
            this.groupBox3.Controls.Add(this.txtLogbookPage);
            this.groupBox3.Controls.Add(this.btnGotoPage);
            this.groupBox3.Controls.Add(this.btnLogbookPages);
            this.groupBox3.Controls.Add(this.btnQSOCount);
            this.groupBox3.Controls.Add(this.btnGotoLookbook);
            this.groupBox3.Location = new System.Drawing.Point(372, 8);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(518, 186);
            this.groupBox3.TabIndex = 40;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Logbook";
            // 
            // btnQSOforPage
            // 
            this.btnQSOforPage.Location = new System.Drawing.Point(11, 89);
            this.btnQSOforPage.Name = "btnQSOforPage";
            this.btnQSOforPage.Size = new System.Drawing.Size(91, 31);
            this.btnQSOforPage.TabIndex = 24;
            this.btnQSOforPage.Text = "QSO for page";
            this.btnQSOforPage.UseVisualStyleBackColor = true;
            this.btnQSOforPage.Click += new System.EventHandler(this.btnQSOforPage_Click);
            // 
            // btnOrderDateDesc
            // 
            this.btnOrderDateDesc.Location = new System.Drawing.Point(209, 89);
            this.btnOrderDateDesc.Name = "btnOrderDateDesc";
            this.btnOrderDateDesc.Size = new System.Drawing.Size(93, 31);
            this.btnOrderDateDesc.TabIndex = 20;
            this.btnOrderDateDesc.Text = "Date Desc";
            this.btnOrderDateDesc.UseVisualStyleBackColor = true;
            this.btnOrderDateDesc.Click += new System.EventHandler(this.btnOrderDateDesc_Click);
            // 
            // btnOrderDateAsc
            // 
            this.btnOrderDateAsc.Location = new System.Drawing.Point(108, 89);
            this.btnOrderDateAsc.Name = "btnOrderDateAsc";
            this.btnOrderDateAsc.Size = new System.Drawing.Size(95, 31);
            this.btnOrderDateAsc.TabIndex = 19;
            this.btnOrderDateAsc.Text = "Date Asc";
            this.btnOrderDateAsc.UseVisualStyleBackColor = true;
            this.btnOrderDateAsc.Click += new System.EventHandler(this.btnOrderDateAsc_Click);
            // 
            // btnPageDown
            // 
            this.btnPageDown.Location = new System.Drawing.Point(209, 52);
            this.btnPageDown.Name = "btnPageDown";
            this.btnPageDown.Size = new System.Drawing.Size(21, 31);
            this.btnPageDown.TabIndex = 16;
            this.btnPageDown.Text = "-";
            this.btnPageDown.UseVisualStyleBackColor = true;
            this.btnPageDown.Click += new System.EventHandler(this.btnPageDown_Click);
            // 
            // btnCurrentPage
            // 
            this.btnCurrentPage.Location = new System.Drawing.Point(108, 52);
            this.btnCurrentPage.Name = "btnCurrentPage";
            this.btnCurrentPage.Size = new System.Drawing.Size(95, 31);
            this.btnCurrentPage.TabIndex = 15;
            this.btnCurrentPage.Text = "Current page";
            this.btnCurrentPage.UseVisualStyleBackColor = true;
            this.btnCurrentPage.Click += new System.EventHandler(this.btnCurrentPage_Click);
            // 
            // btnPageUp
            // 
            this.btnPageUp.Location = new System.Drawing.Point(281, 52);
            this.btnPageUp.Name = "btnPageUp";
            this.btnPageUp.Size = new System.Drawing.Size(21, 31);
            this.btnPageUp.TabIndex = 18;
            this.btnPageUp.Text = "+";
            this.btnPageUp.UseVisualStyleBackColor = true;
            this.btnPageUp.Click += new System.EventHandler(this.btnPageUp_Click);
            // 
            // btnGetTableContentText
            // 
            this.btnGetTableContentText.Location = new System.Drawing.Point(308, 87);
            this.btnGetTableContentText.Name = "btnGetTableContentText";
            this.btnGetTableContentText.Size = new System.Drawing.Size(133, 31);
            this.btnGetTableContentText.TabIndex = 23;
            this.btnGetTableContentText.Text = "Get Table Content Text";
            this.btnGetTableContentText.UseVisualStyleBackColor = true;
            this.btnGetTableContentText.Click += new System.EventHandler(this.GetTableContentText_Click);
            // 
            // btnGetTableContentXML
            // 
            this.btnGetTableContentXML.Location = new System.Drawing.Point(308, 18);
            this.btnGetTableContentXML.Name = "btnGetTableContentXML";
            this.btnGetTableContentXML.Size = new System.Drawing.Size(133, 31);
            this.btnGetTableContentXML.TabIndex = 21;
            this.btnGetTableContentXML.Text = "Get Table Content XML";
            this.btnGetTableContentXML.UseVisualStyleBackColor = true;
            this.btnGetTableContentXML.Click += new System.EventHandler(this.btnGetTableContentXML_Click);
            // 
            // btnGetTableContentRaw
            // 
            this.btnGetTableContentRaw.Location = new System.Drawing.Point(308, 52);
            this.btnGetTableContentRaw.Name = "btnGetTableContentRaw";
            this.btnGetTableContentRaw.Size = new System.Drawing.Size(133, 31);
            this.btnGetTableContentRaw.TabIndex = 22;
            this.btnGetTableContentRaw.Text = "Get Table Content Raw";
            this.btnGetTableContentRaw.UseVisualStyleBackColor = true;
            this.btnGetTableContentRaw.Click += new System.EventHandler(this.btnGetTableContentRaw_Click);
            // 
            // txtLogbookPage
            // 
            this.txtLogbookPage.Location = new System.Drawing.Point(236, 58);
            this.txtLogbookPage.Name = "txtLogbookPage";
            this.txtLogbookPage.Size = new System.Drawing.Size(39, 20);
            this.txtLogbookPage.TabIndex = 17;
            this.txtLogbookPage.Text = "1";
            this.txtLogbookPage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnGotoPage
            // 
            this.btnGotoPage.Location = new System.Drawing.Point(209, 18);
            this.btnGotoPage.Name = "btnGotoPage";
            this.btnGotoPage.Size = new System.Drawing.Size(93, 31);
            this.btnGotoPage.TabIndex = 13;
            this.btnGotoPage.Text = "Goto Page";
            this.btnGotoPage.UseVisualStyleBackColor = true;
            this.btnGotoPage.Click += new System.EventHandler(this.btnGotoPage_Click);
            // 
            // btnLogbookPages
            // 
            this.btnLogbookPages.Location = new System.Drawing.Point(108, 18);
            this.btnLogbookPages.Name = "btnLogbookPages";
            this.btnLogbookPages.Size = new System.Drawing.Size(95, 31);
            this.btnLogbookPages.TabIndex = 12;
            this.btnLogbookPages.Text = "Logbook Pages";
            this.btnLogbookPages.UseVisualStyleBackColor = true;
            this.btnLogbookPages.Click += new System.EventHandler(this.btnLogbookPages_Click);
            // 
            // btnQSOCount
            // 
            this.btnQSOCount.Location = new System.Drawing.Point(11, 52);
            this.btnQSOCount.Name = "btnQSOCount";
            this.btnQSOCount.Size = new System.Drawing.Size(91, 31);
            this.btnQSOCount.TabIndex = 14;
            this.btnQSOCount.Text = "QSO Count";
            this.btnQSOCount.UseVisualStyleBackColor = true;
            this.btnQSOCount.Click += new System.EventHandler(this.btnQSOCount_Click);
            // 
            // btnGotoLookbook
            // 
            this.btnGotoLookbook.Location = new System.Drawing.Point(11, 18);
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
            this.groupBox2.Location = new System.Drawing.Point(255, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(108, 186);
            this.groupBox2.TabIndex = 37;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Loookup";
            // 
            // btnLookupAndCheck
            // 
            this.btnLookupAndCheck.Location = new System.Drawing.Point(6, 101);
            this.btnLookupAndCheck.Name = "btnLookupAndCheck";
            this.btnLookupAndCheck.Size = new System.Drawing.Size(94, 54);
            this.btnLookupAndCheck.TabIndex = 10;
            this.btnLookupAndCheck.Text = "Lookup and Check Worked";
            this.btnLookupAndCheck.UseVisualStyleBackColor = true;
            this.btnLookupAndCheck.Click += new System.EventHandler(this.btnLookupAndCheck_Click);
            // 
            // btnCheckWorked
            // 
            this.btnCheckWorked.Location = new System.Drawing.Point(6, 72);
            this.btnCheckWorked.Name = "btnCheckWorked";
            this.btnCheckWorked.Size = new System.Drawing.Size(94, 27);
            this.btnCheckWorked.TabIndex = 9;
            this.btnCheckWorked.Text = "Check Worked";
            this.btnCheckWorked.UseVisualStyleBackColor = true;
            this.btnCheckWorked.Click += new System.EventHandler(this.btnCheckWorked_Click);
            // 
            // txtQRZLookup
            // 
            this.txtQRZLookup.Location = new System.Drawing.Point(6, 19);
            this.txtQRZLookup.Name = "txtQRZLookup";
            this.txtQRZLookup.Size = new System.Drawing.Size(94, 20);
            this.txtQRZLookup.TabIndex = 7;
            this.txtQRZLookup.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtQRZLookup_KeyDown);
            // 
            // btnLookup
            // 
            this.btnLookup.Location = new System.Drawing.Point(6, 43);
            this.btnLookup.Name = "btnLookup";
            this.btnLookup.Size = new System.Drawing.Size(94, 27);
            this.btnLookup.TabIndex = 8;
            this.btnLookup.Text = "Lookup";
            this.btnLookup.UseVisualStyleBackColor = true;
            this.btnLookup.Click += new System.EventHandler(this.btnLookup_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkShowHeader);
            this.groupBox1.Controls.Add(this.chkIsLogged);
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
            this.groupBox1.Location = new System.Drawing.Point(11, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(235, 188);
            this.groupBox1.TabIndex = 36;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "QRZ.COM Startup";
            // 
            // chkShowHeader
            // 
            this.chkShowHeader.AutoSize = true;
            this.chkShowHeader.Location = new System.Drawing.Point(8, 169);
            this.chkShowHeader.Name = "chkShowHeader";
            this.chkShowHeader.Size = new System.Drawing.Size(183, 17);
            this.chkShowHeader.TabIndex = 43;
            this.chkShowHeader.Text = "Show Header (this box) at startup";
            this.chkShowHeader.UseVisualStyleBackColor = true;
            this.chkShowHeader.CheckedChanged += new System.EventHandler(this.chkShowHeader_CheckedChanged);
            // 
            // chkIsLogged
            // 
            this.chkIsLogged.AutoSize = true;
            this.chkIsLogged.Location = new System.Drawing.Point(8, 146);
            this.chkIsLogged.Name = "chkIsLogged";
            this.chkIsLogged.Size = new System.Drawing.Size(157, 17);
            this.chkIsLogged.TabIndex = 42;
            this.chkIsLogged.Text = "Check Is Logged ad startup";
            this.chkIsLogged.UseVisualStyleBackColor = true;
            this.chkIsLogged.CheckedChanged += new System.EventHandler(this.chkIsLogged_CheckedChanged);
            // 
            // btnCommandList
            // 
            this.btnCommandList.Location = new System.Drawing.Point(134, 111);
            this.btnCommandList.Name = "btnCommandList";
            this.btnCommandList.Size = new System.Drawing.Size(94, 26);
            this.btnCommandList.TabIndex = 41;
            this.btnCommandList.Text = "Command List";
            this.btnCommandList.UseVisualStyleBackColor = true;
            this.btnCommandList.Click += new System.EventHandler(this.btnCommandList_Click);
            // 
            // btnClearMonitor
            // 
            this.btnClearMonitor.Location = new System.Drawing.Point(6, 112);
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
            this.label2.Location = new System.Drawing.Point(6, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 40;
            this.label2.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(82, 85);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(70, 20);
            this.txtPassword.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 38;
            this.label1.Text = "Username";
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(82, 59);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(70, 20);
            this.txtUsername.TabIndex = 4;
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(158, 56);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(70, 49);
            this.btnLogin.TabIndex = 6;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnQRZHome
            // 
            this.btnQRZHome.Location = new System.Drawing.Point(6, 19);
            this.btnQRZHome.Name = "btnQRZHome";
            this.btnQRZHome.Size = new System.Drawing.Size(70, 31);
            this.btnQRZHome.TabIndex = 1;
            this.btnQRZHome.Text = "QRZ Home";
            this.btnQRZHome.UseVisualStyleBackColor = true;
            this.btnQRZHome.Click += new System.EventHandler(this.btnQRZHome_Click);
            // 
            // btnLogOut
            // 
            this.btnLogOut.Location = new System.Drawing.Point(158, 19);
            this.btnLogOut.Name = "btnLogOut";
            this.btnLogOut.Size = new System.Drawing.Size(70, 31);
            this.btnLogOut.TabIndex = 3;
            this.btnLogOut.Text = "Log Out";
            this.btnLogOut.UseVisualStyleBackColor = true;
            this.btnLogOut.Click += new System.EventHandler(this.btnLogOut_Click);
            // 
            // btnIsLogged
            // 
            this.btnIsLogged.Location = new System.Drawing.Point(82, 19);
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
            this.txtMonitor.ReadOnly = true;
            this.txtMonitor.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMonitor.Size = new System.Drawing.Size(936, 451);
            this.txtMonitor.TabIndex = 24;
            this.txtMonitor.WordWrap = false;
            this.txtMonitor.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMonitor_KeyPress);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(940, 691);
            this.Controls.Add(this.splitContainer1);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(956, 726);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QRZ Console";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmTest_KeyDown);
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
        private System.Windows.Forms.CheckBox chkIsLogged;
        private System.Windows.Forms.CheckBox chkShowHeader;
        private System.Windows.Forms.Button btnGetTableContentText;
        private System.Windows.Forms.Button btnQSOforPage;
    }
}

