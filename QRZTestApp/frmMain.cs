﻿using Microsoft.Win32;
using QRZLibrary;
using QRZLibrary.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Winsock_Orcas;

namespace QRZConsole
{
    public partial class frmMain : Form
    {
        Logbook qrz;

        private delegate void SafeCallDelegate(string log, bool appendNewLine);
        private string lastQRZ = string.Empty;

        private Winsock ws;

        string ClusterAddr = "gb7ujs.ham-radio-op.net";
        int ClusterPort = 7373;
        private bool ClusterDebug = false;
        string ClusterDXprompt = "ClusterDX>";
        private bool clusterConnected = false;

        private int splitterExpand = 225;
        private int splittercompress = 25;
        private int pixelsResize = 2;

        public frmMain()
        {
            InitializeComponent();

            qrz = new Logbook();
        }


        private void addMonitor(string log, bool appendNewLine = true)
        {
            if (txtMonitor.InvokeRequired)
            {
                var d = new SafeCallDelegate(addMonitor);
                txtMonitor.Invoke(d, new object[] { log });
            }
            else
            {
                txtMonitor.AppendText(log + (appendNewLine ? Environment.NewLine : string.Empty));
                txtMonitor.SelectionStart = txtMonitor.Text.Length;
                txtMonitor.ScrollToCaret();
            }
        }

        private void btnIsLogged_Click(object sender, EventArgs e)
        {
            IsLogged();
        }

        private void IsLogged()
        {
            addMonitor($"Start IsLogged request...");
            bool il = qrz.IsLogged();
            addMonitor($"IsLogged = {il.ToString()}");
            SaveRegKey("IsLoggedIn",  il?"1":"0");
        }

        private void IsLoggedInLastStatus()
        {
            bool loggedin = (GetRegKeyValue("IsLoggedIn") == "1");
            if (loggedin)
                addMonitor($"IsLogged (last status) = {loggedin.ToString()} by [{txtUsername.Text}]");
            else
                addMonitor($"IsLogged (last status) = {loggedin.ToString()}");
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogOut();
        }

        private void LogOut()
        {
            if (GetRegKeyValue("IsLoggedIn") != "1")
            {
                addMonitor($"Warning! IsLogged (last status) = False");
                addMonitor($"Before Logout start IsLogged request with il command]");
            }
            else
            {
                addMonitor($"Start Log Out...");
                bool lo = qrz.LogOut();
                addMonitor($"Logout = {lo.ToString()}");
                if (lo)
                {
                    SaveRegKey("IsLoggedIn", "0");
                    txtUsername.Text = string.Empty;
                    txtPassword.Text = string.Empty;
                    qrz.Qrz = string.Empty;
                    qrz.Password = string.Empty;
                    SaveRegKey("username", qrz.Qrz);
                    SaveRegKey("password", qrz.Password);
                }
            }
        }

        private void btnQRZHome_Click(object sender, EventArgs e)
        {
            QRZHome();
        }

        private void QRZHome()
        {
            addMonitor($"Load QRZ Home...");
            addMonitor($"Load QRZ Home = {qrz.GotoQrzHome()}");
        }

        private void btnGotoLookbook_Click(object sender, EventArgs e)
        {
            GotoLogbook();
        }

        private void GotoLogbook()
        {
            addMonitor($"Load Logbook...");
            addMonitor($"Load Logbook = {qrz.GotoLogbook()}");
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            qrz.Qrz = txtUsername.Text;
            qrz.Password = txtPassword.Text;
            Login();
        }

        private void Login(string prm1 = "", string prm2 = "")
        {
            if (GetRegKeyValue("IsLoggedIn")=="1")
            {
                addMonitor($"Warning! IsLogged (last status) = True by [{GetRegKeyValue("username")}]");
                addMonitor($"Before new Login start IsLogged request with il command]");
            }
            else
            {
                if (prm1 != "")
                    qrz.Qrz = prm1.ToUpper();

                if (prm2 != "")
                    qrz.Password = prm2;

                if (qrz.Qrz != "" && qrz.Password != "")
                {
                    SaveRegKey("username", qrz.Qrz);
                    SaveRegKey("password", qrz.Password);

                    txtUsername.Text = qrz.Qrz;
                    txtPassword.Text = qrz.Password;

                    addMonitor($"Login... QRZ=[{qrz.Qrz}] - Password=[{new string('*', qrz.Password.Length)}]");

                    string errorMessage = string.Empty;
                    bool logged = qrz.Login(out errorMessage);
                    if (logged)
                    {
                        SaveRegKey("IsLoggedIn", "1");
                        addMonitor($"Logged in QRZ=[{qrz.Qrz}]");
                    }
                    else
                        addMonitor(errorMessage);
                }
                else
                {
                    addMonitor($"Unable to Login: username and password are required fields!");
                }
            }
        }

        private void btnLookup_Click(object sender, EventArgs e)
        {
            string QRZtoSearch = txtQRZLookup.Text;

            QRZtoSearch = QRZtoSearch.ToUpper();

            Lookup(QRZtoSearch);
            lastQRZ = QRZtoSearch;
        }

        private void Lookup(string QRZtoSearch = "")
        {
            if (QRZtoSearch != "")
            {
                QRZtoSearch = QRZtoSearch.ToUpper();
                lastQRZ = QRZtoSearch;
            }
            else
                QRZtoSearch = lastQRZ;

            if (QRZtoSearch != "")
            {
                SaveRegKey("lookup", QRZtoSearch);
                txtQRZLookup.Text = QRZtoSearch;

                addMonitor("");
                addMonitor($"Lookup {QRZtoSearch}...");
                LookupEntry entry = qrz.ExecQuery(QRZtoSearch);
                addMonitor($"Resut = --->");
                addMonitor($"   QRZ       = {entry.QRZ}");
                addMonitor($"   DXCC      = {entry.DXCC}");
                addMonitor($"   Country   = {entry.Country}");
                addMonitor($"   Name      = {entry.Name}");
                addMonitor($"   Address 1 = {entry.Address1}");
                addMonitor($"   Address 2 = {entry.Address2}");
                addMonitor($"   Address 3 = {entry.Address3}");
                addMonitor($"   Email     = {entry.Email}");
                addMonitor($"------------------------------------");
            }
            else
                addMonitor($"No QRZ to search");
        }

        private void btnCheckWorked_Click(object sender, EventArgs e)
        {
            string QRZtoSearch = txtQRZLookup.Text;

            QRZtoSearch = QRZtoSearch.ToUpper();

            GetWorked(QRZtoSearch);

        }

        private void GetWorked(string QRZtoSearch = "")
        {
            if (QRZtoSearch != "")
            {
                QRZtoSearch = QRZtoSearch.ToUpper();
                lastQRZ = QRZtoSearch;
            }
            else
                QRZtoSearch = lastQRZ;

            if (QRZtoSearch != "")
            {
                SaveRegKey("lookup", QRZtoSearch);

                addMonitor("");
                addMonitor($"Check Worked {QRZtoSearch}...");

                string ret = qrz.CheckWorkedRaw(QRZtoSearch);
                if (ret != "")
                    addMonitor(ret);
                else
                    addMonitor("No result found!");
            }
            else
                addMonitor("No QRZ to check");
        }

        private void btnLookupAndCheck_Click(object sender, EventArgs e)
        {
            btnLookup.PerformClick();
            btnCheckWorked.PerformClick();
            txtQRZLookup.SelectAll();
            txtQRZLookup.Focus();
        }

        private void btnQSOCount_Click(object sender, EventArgs e)
        {
            QSOCount();
        }

        private void QSOCount()
        {
            addMonitor($"Get QSOs Count... ");
            addMonitor($"QSOs Count = {qrz.GetQSOsCount()}");
        }

        private void btnLogbookPages_Click(object sender, EventArgs e)
        {
            LogbookPages();
        }

        private void LogbookPages()
        {
            addMonitor($"Get Loogbook Pages... ");
            addMonitor($"Loogbook Pages = {qrz.GetLogbookPages()}");
        }

        private void btnCurrentPage_Click(object sender, EventArgs e)
        {
            CurrentPage();
        }

        private void CurrentPage()
        {
            addMonitor($"Get Current Loogbook Page... ");
            int currentPage = qrz.GetCurrentLogbookPage();
            addMonitor($"Current Loogbook Page = {currentPage}");
            if (currentPage > 0)
                txtLogbookPage.Text = currentPage.ToString();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Text = $"{Application.ProductName} v.{System.Windows.Forms.Application.ProductVersion}-beta";

            txtUsername.Text = GetRegKeyValue("username");
            txtPassword.Text = GetRegKeyValue("password");
            txtQRZLookup.Text = GetRegKeyValue("lookup");
            lastQRZ = txtQRZLookup.Text;

            this.Show();

            chkIsLogged.Checked = (GetRegKeyValue("CheckIsLoggedIn") == "1");

            addMonitor($"Welcome to {this.Text}");
            addMonitor("");

            if (chkIsLogged.Checked)
                IsLogged();
            else
                IsLoggedInLastStatus();

            addMonitor("");

            chkShowHeader.Checked = (GetRegKeyValue("ShowHeader") == "1");

            if (!chkShowHeader.Checked)
                SwitchView();

            txtCommand.Text = "insert command here (digit cm and press ENTER for Command List)";
            txtCommand.SelectAll();
            txtCommand.Focus();
        }

        private void btnGotoPage_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            GotoPage(page);
        }

        private void GotoPage(int page)
        {
            addMonitor($"Goto Page {page}... ");
            int currentPage = qrz.GotoLoogbookPage(page);
            addMonitor($"Current Page = {currentPage}");
            txtLogbookPage.Text = currentPage.ToString();
        }

        private void btnGetTableContentRaw_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            GetTableContenteRawView(page);
        }

        private void GetTableContenteRawView(int page)
        {
            if (page < 1)
                int.TryParse(txtLogbookPage.Text, out page);

            addMonitor($"Get Table content Raw of page {page}... ");
            addMonitor($"{qrz.GetLogbookPageContentRaw(page)}");
        }

        private void btnGetTableContentXML_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            GetTableContentXML(page);
        }

        private void GetTableContentXML(int page)
        {
            if (page < 1)
                int.TryParse(txtLogbookPage.Text, out page);

            addMonitor($"Get Table content XML of page {page}... ");

            List<LogbookEntry> lbentries = qrz.GetLogbookPageContent(page);
            if (lbentries != null)
            {
                if (lbentries.Count > 0)
                {
                    string xml = QRZHelper.SerializeLogbookEntryCollection(lbentries);
                    addMonitor("");
                    addMonitor($"************************************************************************************************");
                    addMonitor(xml);
                    addMonitor($"************************************************************************************************");
                    addMonitor("");
                }
                else
                    addMonitor("Found 0 entries");
            }
        }

        private void GetQSObyRangeTextView(int start, int end)
        {
            addMonitor($"Get QSOs by range (Text View): from position {start} to {end}... ");

            List<LogbookEntry> lbentries = qrz.GetLogbookEntriesByRange(start, end);
            if (lbentries != null)
            {
                if (lbentries.Count > 0)
                {
                    string headerLine = new string('-', 223);
                    addMonitor("");
                    addMonitor(headerLine);

                    string headerText = "";
                    headerText += "| ";
                    headerText += GetFixedString("Num.", 5);
                    headerText += " | ";
                    headerText += GetFixedString("Date - Time", 16);
                    headerText += " | ";
                    headerText += GetFixedString("Call", 13);
                    headerText += " | ";
                    headerText += GetFixedString("Band", 5);
                    headerText += " | ";
                    headerText += GetFixedString("Freq.", 7);
                    headerText += " | ";
                    headerText += GetFixedString("Mode", 6);
                    headerText += " | ";
                    headerText += GetFixedString("GridLoc", 8);
                    headerText += " | ";
                    headerText += GetFixedString("Country", 20);
                    headerText += " | ";
                    headerText += GetFixedString("Operator Name", 50);
                    headerText += " | ";
                    headerText += GetFixedString("Comments", 50);
                    headerText += " |";
                    headerText += "Confirmed";
                    headerText += " | ";
                    addMonitor(headerText);
                    addMonitor(headerLine);

                    foreach (LogbookEntry entry in lbentries)
                    {
                        string rowText = "";
                        rowText += "| ";
                        rowText += GetFixedString(entry.position.ToString().PadLeft(5), 5);
                        rowText += " | ";
                        rowText += GetFixedString(entry.QSODateTime.ToString("yyyy-MM-dd HH:mm"), 16);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Call, 13);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Band, 5);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Frequency, 7);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Mode, 6);
                        rowText += " | ";
                        rowText += GetFixedString(entry.GridLocator, 8);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Country, 20);
                        rowText += " | ";
                        rowText += GetFixedString(entry.OperatorName, 50);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Comments, 50);
                        rowText += " |";
                        rowText += entry.Confirmed ? "    *    " : "         ";
                        rowText += " | ";
                        addMonitor(rowText);
                    }
                    addMonitor(headerLine);

                }
                else
                    addMonitor("Found 0 entries");
            }
        }


        private void GetQSObyRangeTextRaw(int start, int end)
        {
            addMonitor($"Get QSOs by range (Text Raw): from position {start} to {end}... ");

            List<LogbookEntry> lbentries = qrz.GetLogbookEntriesByRange(start, end);
            if (lbentries != null)
            {
                if (lbentries.Count > 0)
                {

                    string headerText = "";
                    headerText += "Num.";
                    headerText += "\t";
                    headerText += "Date Time";
                    headerText += "\t";
                    headerText += "Call";
                    headerText += "\t";
                    headerText += "Band";
                    headerText += "\t";
                    headerText += "Frequency";
                    headerText += "\t";
                    headerText += "Mode";
                    headerText += "\t";
                    headerText += "GridLocator";
                    headerText += "\t";
                    headerText += "Country";
                    headerText += "\t";
                    headerText += "Operator Name";
                    headerText += "\t";
                    headerText += "Comments";
                    headerText += "\t";
                    headerText += "Confirmed";
                    addMonitor(headerText);

                    foreach (LogbookEntry entry in lbentries)
                    {
                        string rowText = "";
                        rowText += entry.position;
                        rowText += "\t";
                        rowText += entry.QSODateTime.ToString("yyyy-MM-dd HH:mm");
                        rowText += "\t";
                        rowText += entry.Call;
                        rowText += "\t";
                        rowText += entry.Band;
                        rowText += "\t";
                        rowText += entry.Frequency;
                        rowText += "\t";
                        rowText += entry.Mode;
                        rowText += "\t";
                        rowText += entry.GridLocator;
                        rowText += "\t";
                        rowText += entry.Country;
                        rowText += "\t";
                        rowText += entry.OperatorName;
                        rowText += "\t";
                        rowText += entry.Comments;
                        rowText += "\t";
                        rowText += entry.Confirmed ? "*" : "";
                        addMonitor(rowText);
                    }
                    addMonitor("");
                }
                else
                    addMonitor("Found 0 entries");
            }
        }

        private void GetTableContentTextView(int page)
        {
            if (page < 1)
                int.TryParse(txtLogbookPage.Text, out page);

            addMonitor($"Get Table content text of page {page}... ");

            List<LogbookEntry> lbentries = qrz.GetLogbookPageContent(page);
            if (lbentries != null)
            {
                if (lbentries.Count > 0)
                {
                    string headerLine = new string('-', 217);
                    addMonitor("");
                    addMonitor(headerLine);

                    string headerText = "";
                    headerText += "| ";
                    headerText += GetFixedString("Num.", 5);
                    headerText += " | ";
                    headerText += GetFixedString("Date - Time", 16);
                    headerText += " | ";
                    headerText += GetFixedString("Call", 13);
                    headerText += " | ";
                    headerText += GetFixedString("Band", 5);
                    headerText += " | ";
                    headerText += GetFixedString("Freq.", 7);
                    headerText += " | ";
                    headerText += GetFixedString("Mode", 6);
                    headerText += " | ";
                    headerText += "QSL";
                    headerText += " | ";
                    headerText += GetFixedString("GridLoc", 8);
                    headerText += " | ";
                    headerText += GetFixedString("Country", 20);
                    headerText += " | ";
                    headerText += GetFixedString("Operator Name", 50);
                    headerText += " | ";
                    headerText += GetFixedString("Comments", 50);
                    headerText += " |";
                    addMonitor(headerText);
                    addMonitor(headerLine);

                    foreach (LogbookEntry entry in lbentries)
                    {
                        string rowText = "";
                        rowText += "| ";
                        rowText += GetFixedString(entry.position.ToString().PadLeft(5), 5);
                        rowText += " | ";
                        rowText += GetFixedString(entry.QSODateTime.ToString("yyyy-MM-dd HH:mm"), 16);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Call, 13);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Band, 5);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Frequency, 7);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Mode, 6);
                        rowText += " | ";
                        rowText += entry.Confirmed ? " * " : "   ";
                        rowText += " | ";
                        rowText += GetFixedString(entry.GridLocator, 8);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Country, 20);
                        rowText += " | ";
                        rowText += GetFixedString(entry.OperatorName, 50);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Comments, 50);
                        rowText += " |";
                        addMonitor(rowText);
                    }
                    addMonitor(headerLine);

                }
                else
                    addMonitor("Found 0 entries");
            }
        }

        private void btnPageDown_Click(object sender, EventArgs e)
        {
            PageDown();
        }

        private void PageDown()
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            if (page > 1)
                page = page - 1;

            txtLogbookPage.Text = page.ToString();

            GotoPage(page);
        }

        private void btnPageUp_Click(object sender, EventArgs e)
        {
            PageUp();
        }

        private void PageUp()
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            page++;

            txtLogbookPage.Text = page.ToString();

            GotoPage(page);
        }

        private void SaveRegKey(string key, string value)
        {
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\{Application.ProductName}");

            regkey.SetValue(key, value);
            regkey.Close();
        }

        private string GetRegKeyValue(string key)
        {
            string ret = string.Empty;

            RegistryKey regkey = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\{Application.ProductName}");

            //if it does exist, retrieve the stored values  
            if (regkey != null)
            {
                ret = regkey.GetValue(key)?.ToString();
                regkey.Close();
            }
            return ret;
        }

        private void txtQRZLookup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                btnLookupAndCheck.PerformClick();
            }
        }

        private void btnClearMonitor_Click(object sender, EventArgs e)
        {
            ClearMonitor();
        }

        private void ClearMonitor()
        {
            txtMonitor.Text = string.Empty;
        }

        private void btnOrderDateAsc_Click(object sender, EventArgs e)
        {
            OrderDateAsc();
        }

        private void OrderDateAsc()
        {
            addMonitor($"Set Logbook Order Date Asc... ");
            int currOrder = qrz.SetLogbookDateOrder(0);
            if (currOrder == 0)
                addMonitor($"Logbook Order Date is \"Asc\"");
            else
                addMonitor($"Unable to Set Logbook Order Date Asc");
        }

        private void GetEntriesForPage()
        {
            addMonitor($"Get QSO for page... ");
            addMonitor($"QSO for page = {qrz.GetEntriesForPage()}");
        }

        private void btnOrderDateDesc_Click(object sender, EventArgs e)
        {
            OrderDateDesc();
        }

        private void OrderDateDesc()
        {
            addMonitor($"Set Logbook Order Date Desc... ");
            int currOrder = qrz.SetLogbookDateOrder(1);
            if (currOrder == 1)
                addMonitor($"Logbook Order Date is \"Desc\"");
            else
                addMonitor($"Unable to Set Logbook Order Date Desc");
        }

        private void txtCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void btnDing_Click(object sender, EventArgs e)
        {

        }

        private void txtCommand_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

                elabCommand(txtCommand.Text);


                txtCommand.SelectAll();
                txtCommand.Focus();

                e.Handled = true;
            }
        }
        private void txtCommand_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void elabCommand(string command)
        {
            if (command.Trim() != "")
            {
                string cmd = string.Empty;
                string[] param;
                if (command.IndexOf(" ") > 0)
                {
                    param = command.Split(" ".ToCharArray()[0]);
                    cmd = param[0];
                }
                else
                {
                    cmd = command;
                }

                string prm1 = string.Empty;
                string prm2 = string.Empty;
                if (command.IndexOf(" ") > 0)
                {
                    if (command.Split(" ".ToCharArray()[0]).Length > 1)
                        prm1 = command.Split(" ".ToCharArray()[0])[1];
                    if (command.Split(" ".ToCharArray()[0]).Length > 2)
                        prm2 = command.Split(" ".ToCharArray()[0])[2];
                }

                addMonitor("");
                addMonitor($">{command}");
                
                cmd = cmd.ToLower();

                int iprm1 = 0;
                if (int.TryParse(prm1, out int tmp1))
                    iprm1 = tmp1;

                int iprm2 = 0;
                if (int.TryParse(prm2, out int tmp2))
                    iprm2 = tmp2;

                switch (cmd)
                {
                    case "cm":
                        CommandList();
                        break;
                    case "sl":
                        ShortcutList();
                        break;
                    case "il":
                        IsLogged();
                        break;
                    case "ls":
                        IsLoggedInLastStatus();
                        break;
                    case "li":
                        Login(prm1, prm2);
                        break;
                    case "lo":
                        LogOut();
                        break;
                    case "lu":
                        Lookup(prm1);
                        break;
                    case "cw":
                        GetWorked(prm1);
                        break;
                    case "lw":
                        Lookup(prm1);
                        GetWorked(prm1);
                        break;
                    case "wl":
                        GetWorked(prm1);
                        Lookup(prm1);
                        break;
                    case "hm":
                        QRZHome();
                        break;
                    case "lb":
                        GotoLogbook();
                        break;
                    case "qc":
                        QSOCount();
                        break;
                    case "qp":
                        GetEntriesForPage();
                        break;
                    case "lp":
                        LogbookPages();
                        break;
                    case "gp":
                        GotoPage(iprm1);
                        break;
                    case "pd":
                        PageDown();
                        break;
                    case "pu":
                        PageUp();
                        break;
                    case "cp":
                        CurrentPage();
                        break;
                    case "da":
                        OrderDateAsc();
                        break;
                    case "dd":
                        OrderDateDesc();
                        break;
                    case "tt":
                        GetTableContentTextView(iprm1);
                        break;
                    case "tr":
                        GetTableContenteRawView(iprm1);
                        break;
                    case "tx":
                        GetTableContentXML(iprm1);
                        break;
                    case "qt":
                        GetQSObyRangeTextView(iprm1, iprm2);
                        break;
                    case "qr":
                        GetQSObyRangeTextRaw(iprm1, iprm2);
                        break;
                    case "cl":
                        ClearMonitor();
                        break;
                    case "sw":
                        SwitchView();
                        break;
                    case "sc":
                        SwitchCheckIsLoggedAtStartup();
                        break;
                    case "fs":
                        SwitchFullScreen();
                        break;
                    case "dx":
                        ClusterDxOpen(prm1, prm2);
                        break;
                    case "sb":
                        ClusterDxSelectBand(prm1, iprm2);
                        break;
                    case "dc":
                        ClusterDxCmd(command);
                        break;
                    case "qi":
                    case "exit":
                        this.Close();
                        break;
                    default:
                        addMonitor($"invalid comamnd: {command}");
                        break;
                }
            }
        }

        private void SwitchFullScreen()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                addMonitor("Console switched to normal screen");
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                addMonitor("Console switched to full screen");
            }
        }
        private void SwitchCheckIsLoggedAtStartup()
        {
            if (chkIsLogged.Checked)
                chkIsLogged.Checked = false;
            else
                chkIsLogged.Checked = true;
        }

        private void btnSwitchView_Click(object sender, EventArgs e)
        {
            SwitchView();
        }

        private void SwitchView()
        {
            string extra = "";
            if (btnSwitchView.Text == "-")
            {
                splitContainer1.SplitterDistance = splittercompress;
                btnSwitchView.Text = "+";
                extra = " (digit sw and press ENTER for show header)";

            }
            else
            {
                splitContainer1.SplitterDistance = splitterExpand;
                btnSwitchView.Text = "-";
            }
            addMonitor($"View switched{extra}");
        }

        private void resetSplitter()
        {
            if (btnSwitchView.Text == "-")
            {
                splitContainer1.SplitterDistance = splitterExpand;
            }
            else
            {
                splitContainer1.SplitterDistance = splittercompress;
            }
        }

        private void btnCommandList_Click(object sender, EventArgs e)
        {
            CommandList();
        }

        private void ShortcutList()
        {
            int cmdlen = 40;
            addMonitor($"Shortcut List");
            addMonitor($"-------------------------------------------------------------------------");
            addMonitor(GetFixedString($"  Description", cmdlen) + "Keys...");
            addMonitor($"-------------------------------------------------------------------------");
            addMonitor("View:");
            addMonitor(GetFixedString($"  Zoom in", cmdlen) + "CTRL++");
            addMonitor(GetFixedString($"  Zoom out", cmdlen) + "CTRL+-");
            addMonitor(GetFixedString($"  Zoom reset", cmdlen) + "CTRL+0");
            addMonitor("Behaviors:");
            addMonitor(GetFixedString($"  Set the cursor in the command field", cmdlen) + "F2");
            addMonitor($"-------------------------------------------------------------------------");

        }

        private void CommandList()
        {
            int cmdlen = 40;
            addMonitor($"Command List");
            addMonitor($"-------------------------------------------------------------------------");
            addMonitor(GetFixedString($"  Description", cmdlen) + "command [param] ...");
            addMonitor($"-------------------------------------------------------------------------");
            addMonitor("Credentials:");
            addMonitor(GetFixedString($"  Is Logged", cmdlen) + "il");
            addMonitor(GetFixedString($"  Is Logged (last status)", cmdlen) + "ls");
            addMonitor(GetFixedString($"  Login", cmdlen) + "li [username] [password]");
            addMonitor(GetFixedString($"  LogOut", cmdlen) + "lo");
            addMonitor("Search:");
            addMonitor(GetFixedString($"  Lookup", cmdlen) + "lu [qrz]");
            addMonitor(GetFixedString($"  Check Worked", cmdlen) + "gw [qrz]");
            addMonitor(GetFixedString($"  Lookup and Check Worked", cmdlen) + "lw [qrz]");
            addMonitor(GetFixedString($"  Check Worked and Lookup", cmdlen) + "wl [qrz]");
            addMonitor("Logbook:");
            addMonitor(GetFixedString($"  QSO Count", cmdlen) + "qc");
            addMonitor(GetFixedString($"  Logbook pages", cmdlen) + "lp");
            addMonitor(GetFixedString($"  QSO for page", cmdlen) + "qp");
            addMonitor(GetFixedString($"  Goto Page", cmdlen) + "gp [page]");
            addMonitor(GetFixedString($"  Page Down", cmdlen) + "pd");
            addMonitor(GetFixedString($"  Page Up", cmdlen) + "pu");
            addMonitor(GetFixedString($"  Current Page", cmdlen) + "cp");
            addMonitor(GetFixedString($"  Order Date Asc", cmdlen) + "da");
            addMonitor(GetFixedString($"  Order Date Desc", cmdlen) + "dd");
            addMonitor(GetFixedString($"  Get Table Contente Text", cmdlen) + "tt [page]");
            addMonitor(GetFixedString($"  Get Table Contente Raw", cmdlen) + "tr [page]");
            addMonitor(GetFixedString($"  Get Table Content XML", cmdlen) + "tx [page]");
            addMonitor(GetFixedString($"  Get QSOs by range Text View", cmdlen) + "qt [start position] [end position]");
            addMonitor(GetFixedString($"  Get QSOs by range Text Raw", cmdlen) + "qr [start position] [end position]");
            addMonitor("Cluster DX:");
            addMonitor(GetFixedString($"  Open Cluster DX", cmdlen) + "dx");
            addMonitor(GetFixedString($"  Show DX on", cmdlen) + "sb [band] [items] (band example: [40m] [10m] or [hf] [vhf] [uhf])");
            addMonitor(GetFixedString($"  DXSpider command", cmdlen) + "dc [DXSpider command: http://www.dxcluster.org/main/usermanual_en-12.html]");
            addMonitor("General:");
            addMonitor(GetFixedString($"  Clear Monitor", cmdlen) + "cl");
            addMonitor(GetFixedString($"  Switch View", cmdlen) + "sw");
            addMonitor(GetFixedString($"  Switch Check Is Logged at startup", cmdlen) + "sc");
            addMonitor(GetFixedString($"  Switch screen (normal/fullsize)", cmdlen) + "fs");
            addMonitor(GetFixedString($"  Command List", cmdlen) + "cm");
            addMonitor(GetFixedString($"  Shortcut List", cmdlen) + "sl");
            addMonitor(GetFixedString($"  Quit", cmdlen) + "qi");
            addMonitor($"-------------------------------------------------------------------------");
        }

        private void frmTest_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                txtCommand.SelectAll();
                txtCommand.Focus();
            }

            if (e.Control && (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus))
            {
                if (txtMonitor.Font.Size <= 40)
                {
                    txtMonitor.Font = new Font(txtMonitor.Font.FontFamily, txtMonitor.Font.Size + 1);
                    txtCommand.Font = txtMonitor.Font;
                    splitterExpand = splitterExpand + pixelsResize;
                    splittercompress = splittercompress + pixelsResize;
                    btnSwitchView.Height = btnSwitchView.Height + pixelsResize;
                    btnSwitchView.Width = btnSwitchView.Width + pixelsResize;
                    btnSwitchView.Top = btnSwitchView.Top - pixelsResize;
                    btnSwitchView.Left = btnSwitchView.Left - pixelsResize;
                    resetSplitter();
                }
            }

            if (e.Control && (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus))
            {
                if (txtMonitor.Font.Size >= 6)
                {
                    txtMonitor.Font = new Font(txtMonitor.Font.FontFamily, txtMonitor.Font.Size - 1);
                    txtCommand.Font = txtMonitor.Font;
                    splitterExpand = splitterExpand - pixelsResize;
                    splittercompress = splittercompress - pixelsResize;
                    btnSwitchView.Height = btnSwitchView.Height - pixelsResize;
                    btnSwitchView.Width = btnSwitchView.Width - pixelsResize;
                    btnSwitchView.Top = btnSwitchView.Top + pixelsResize;
                    btnSwitchView.Left = btnSwitchView.Left + pixelsResize;
                    resetSplitter();

                }
            }

            if (e.Control && (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0))
            {
                txtMonitor.Font = new Font(txtMonitor.Font.FontFamily, 8.25f);
                txtCommand.Font = txtMonitor.Font;
                splitterExpand = 225;
                splittercompress = 25;
                btnSwitchView.Height = 20;
                btnSwitchView.Width = 20;
                resetSplitter();
            }

        }

        private string GetFixedString(string str, int len)
        {
            if (string.IsNullOrEmpty(str))
                str = "";
            //return str.PadRight(len);
            return str.PadRight(len).Substring(0, len);
        }

        private void chkIsLogged_CheckedChanged(object sender, EventArgs e)
        {
            SaveRegKey("CheckIsLoggedIn", chkIsLogged.Checked?"1":"0");
            string cilasu = "disabled";
            if (chkIsLogged.Checked)
                cilasu = "enabled";
            addMonitor($"Check Is Logged at startup = {cilasu}");
        }

        private void chkShowHeader_CheckedChanged(object sender, EventArgs e)
        {
            SaveRegKey("ShowHeader", chkShowHeader.Checked ? "1" : "0");
            string cilasu = "disabled";
            if (chkShowHeader.Checked)
                cilasu = "enabled";
            addMonitor($"Show Header at startup = {cilasu}");
        }

        private void GetTableContentText_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            GetTableContentTextView(page);
        }

        private void btnQSOforPage_Click(object sender, EventArgs e)
        {
            GetEntriesForPage();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            disposeSocket();
        }

        #region ClusterDX
        private void Ws_SendComplete(object sender, WinsockSendEventArgs e)
        {
            if (ClusterDebug)
                addMonitor($"CLuester DX Send Complete");
        }

        private void Ws_DataArrival(object sender, WinsockDataArrivalEventArgs e)
        {
            string dataArrival = "";
            object obj = ws.Get();
            if (obj.GetType() == Type.GetType("string"))
                dataArrival = obj.ToString();
            else if (obj.GetType() == Type.GetType("System.Byte[]"))
                dataArrival = Encoding.Default.GetString((byte[])obj);

            dataArrival = dataArrival.Replace($"{ClusterDXprompt}\r\n", string.Empty);

            if (dataArrival != string.Empty && clusterConnected)
            {
                addMonitor(dataArrival, false);
            }

            if (!clusterConnected)
            {
                clusterConnected = (dataArrival.IndexOf("WX disabled for ") > 0);
                if (clusterConnected)
                    addMonitor("Cluster DX Logged ln");
            }


        }

        private void Ws_Disconnected(object sender, EventArgs e)
        {
            addMonitor($"Cluster DX disconnected: {ws.RemoteHost}:{ws.RemotePort}");
            clusterConnected = false;
        }

        private void Ws_ErrorReceived(object sender, WinsockErrorReceivedEventArgs e)
        {
            addMonitor($"#Error: {e.Message}");
        }

        private void Ws_Connected(object sender, WinsockConnectedEventArgs e)
        {
            addMonitor($"Clusterd DX connected: {ws.RemoteHost}:{ws.RemotePort}");
            LoginClusterDX();
        }

        private void ClusterDxSelectBand(string band, int items = 0)
        {
            if (!clusterConnected)
            {
                addMonitor("Cluster DX not available. Digit dx for login Cluster DX");
            }
            else
            {
                if (items == 0)
                    items = 20;
                Send($"show/dx {items} on {band}");
            }
        }

        private void ClusterDxCmd(string command)
        {
            if (!clusterConnected)
            {
                addMonitor("Cluster DX not available. Digit dx for login Cluster DX");
            }
            else
            {
                string cmd = command.Substring(3, command.Length - 3);
                Send($"{cmd}");
            }
        }

        private void ClusterDxOpen(string usrename = "", string passowrd = "")
        {

            if (txtUsername.Text != "")
            {
                if (ws == null)
                {
                    ws = new Winsock();
                    ws.LegacySupport = true;

                    ws.Connected += Ws_Connected;
                    ws.ErrorReceived += Ws_ErrorReceived;
                    ws.Disconnected += Ws_Disconnected;
                    ws.DataArrival += Ws_DataArrival;
                    ws.SendComplete += Ws_SendComplete;

                }
                if (ws.State != WinsockStates.Connected)
                {
                    ws.Connect(ClusterAddr, ClusterPort);
                }

                if (clusterConnected)
                    addMonitor("Cluster already connected");
            }
            else
                addMonitor("Please! Befor connect Cluster DX Login in QRZ.com using: li [username] [password] command");


        }

        private void LoginClusterDX()
        {

            addMonitor("Login Cluster DX...");
            Send(txtUsername.Text);
            Send($"set/prompt {ClusterDXprompt}");
            Send($"unset/echo");
            //SendWSCommandByQueue($"set/qth {City} {StateProv}, {Country}");
            Send("unset/announce");
            Send("unset/anntalk");
            Send("unset/dx");
            Send("unset/talk");
            Send("unset/wcy");
            Send("unset/wwv");
            Send("unset/wx");
            //Send($"show/dx 50 on 40m");

        }

        private void Send(string request)
        {
            if (ClusterDebug)
                addMonitor($"Send: {request}");

            ws.Send($"{request}\n");
        }

        private void disposeSocket()
        {
            if (ws != null)
            {
                ws.Connected -= Ws_Connected;
                ws.ErrorReceived -= Ws_ErrorReceived;
                ws.Disconnected -= Ws_Disconnected;
                ws.DataArrival -= Ws_DataArrival;
                ws.SendComplete -= Ws_SendComplete;

                ws.Close();

                ws.Dispose();

                ws = null;

            }
        }
        #endregion

        private void txtMonitor_KeyPress(object sender, KeyPressEventArgs e)
        {
            txtCommand.Text = e.KeyChar.ToString();
            txtCommand.SelectionStart = txtCommand.Text.Length;
            txtCommand.Focus();
        }
    }
}