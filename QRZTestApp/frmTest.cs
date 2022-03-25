using Microsoft.Win32;
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

namespace QRZTestApp
{
    public partial class frmTest : Form
    {
        Logbook qrz;

        private delegate void SafeCallDelegate(string text);

        private string lastQRZ = string.Empty;

        public frmTest()
        {
            InitializeComponent();

            qrz = new Logbook();
        }


        private void addMonitor(string log)
        {
            if (txtMonitor.InvokeRequired)
            {
                var d = new SafeCallDelegate(addMonitor);
                txtMonitor.Invoke(d, new object[] { log });
            }
            else
            {
                txtMonitor.AppendText(log + Environment.NewLine);
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
            string lastStatus = GetRegKeyValue("IsLoggedIn");
            addMonitor($"IsLogged (last status) = {(lastStatus=="1").ToString()}");

        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogOut();
        }

        private void LogOut()
        {
            addMonitor($"Start Log Out...");
            bool lo = qrz.LogOut();
            addMonitor($"Logout = {lo.ToString()}");
            SaveRegKey("IsLoggedIn", "0");
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
            if (prm1 != "")
                qrz.Qrz = prm1;

            if (prm2 != "")
                qrz.Password = prm2;

            if (qrz.Qrz != "" && qrz.Password != "")
            {
                SaveRegKey("username", qrz.Qrz);
                SaveRegKey("password", qrz.Password);

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

        private void Form1_Load(object sender, EventArgs e)
        {
            txtUsername.Text = GetRegKeyValue("username");
            txtPassword.Text = GetRegKeyValue("password");
            txtQRZLookup.Text = GetRegKeyValue("lookup");
            this.Show();

            chkIsLogged.Checked = (GetRegKeyValue("CheckIsLoggedIn") == "1");

            if (chkIsLogged.Checked)
                IsLogged();
            else
                IsLoggedInLastStatus();

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

            GetTableContenteRaw(page);
        }

        private void GetTableContenteRaw(int page)
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

        private void GetTableContenteText(int startpage, int endpage = -1)
        {
            if (startpage < 1)
                int.TryParse(txtLogbookPage.Text, out startpage);

            addMonitor($"Get Table content text of page {startpage}... ");

            string pageText = "";

            List<LogbookEntry> lbentries = qrz.GetLogbookPageContent(startpage);
            if (lbentries != null)
            {
                if (lbentries.Count > 0)
                {
                    string headerLine = new string('-', 211);
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
            if (e.KeyCode == Keys.Return)
            {
                elabCommand(txtCommand.Text);
                txtCommand.SelectAll();
                txtCommand.Focus();
            }
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
                    case "il":
                        IsLogged();
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
                        GetTableContenteText(iprm1);
                        break;
                    case "tr":
                        GetTableContenteRaw(iprm1);
                        break;
                    case "tx":
                        GetTableContentXML(iprm1);
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
                    case "qi":
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
                splitContainer1.SplitterDistance = 25;
                btnSwitchView.Text = "+";
                extra = " (digit sw and press ENTER for show header)";

            }
            else
            {
                splitContainer1.SplitterDistance = 225;
                btnSwitchView.Text = "-";
            }
            addMonitor($"View switched{extra}");
        }

        private void btnCommandList_Click(object sender, EventArgs e)
        {
            CommandList();
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
            addMonitor(GetFixedString($"  Goto Page", cmdlen) + "gp [page]");
            addMonitor(GetFixedString($"  Page Down", cmdlen) + "pd");
            addMonitor(GetFixedString($"  Page Up", cmdlen) + "pu");
            addMonitor(GetFixedString($"  Current Page", cmdlen) + "cp");
            addMonitor(GetFixedString($"  Order Date Asc", cmdlen) + "da");
            addMonitor(GetFixedString($"  Order Date Desc", cmdlen) + "dd");
            addMonitor(GetFixedString($"  Get Table Contente Text", cmdlen) + "tt [page]");
            addMonitor(GetFixedString($"  Get Table Contente Raw", cmdlen) + "tr [page]");
            addMonitor(GetFixedString($"  GetTableContentXML", cmdlen) + "tx [page]");
            addMonitor("General:");
            addMonitor(GetFixedString($"  Clear Monitor", cmdlen) + "cl");
            addMonitor(GetFixedString($"  Switch View", cmdlen) + "sw");
            addMonitor(GetFixedString($"  Switch Check Is Logged at startup", cmdlen) + "sc");
            addMonitor(GetFixedString($"  Switch screen (normal/fullsize)", cmdlen) + "fs");
            addMonitor(GetFixedString($"  Command List", cmdlen) + "cm");
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

            GetTableContenteText(page);
        }
    }
}
