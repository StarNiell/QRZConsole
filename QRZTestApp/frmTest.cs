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
            addMonitor($"IsLogged = {qrz.IsLogged().ToString()}");
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogOut();
        }

        private void LogOut()
        {
            addMonitor($"Start Log Out...");
            addMonitor($"Logout = {qrz.LogOut().ToString()}");
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
                    addMonitor($"Logged in QRZ=[{qrz.Qrz}]");
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
        }

        private void Lookup(string QRZtoSearch)
        {
            QRZtoSearch = QRZtoSearch.ToUpper();

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

        private void btnCheckWorked_Click(object sender, EventArgs e)
        {
            string QRZtoSearch = txtQRZLookup.Text;

            QRZtoSearch = QRZtoSearch.ToUpper();

            GetWorked(QRZtoSearch);

        }

        private void GetWorked(string QRZtoSearch)
        {
            QRZtoSearch = QRZtoSearch.ToUpper();

            SaveRegKey("lookup", QRZtoSearch);

            addMonitor("");
            addMonitor($"Check Worked {QRZtoSearch}...");

            string ret = qrz.CheckWorkedRaw(QRZtoSearch);
            if (ret != "")
                addMonitor(ret);
            else
                addMonitor("No result found!");
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

            btnIsLogged.PerformClick();

            txtCommand.Text = "insert command here";
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

            addMonitor($"Get Table content of page {page}... ");

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
                    default:
                        addMonitor($"invalid comamnd: {command}");
                        break;
                }
            }
        }

        private void btnSwitchView_Click(object sender, EventArgs e)
        {
            SwitchView();
        }

        private void SwitchView()
        {
            if (btnSwitchView.Text == "-")
            {
                splitContainer1.SplitterDistance = 25;
                btnSwitchView.Text = "+";

            }
            else
            {
                splitContainer1.SplitterDistance = 225;
                btnSwitchView.Text = "-";
            }
        }

        private void btnCommandList_Click(object sender, EventArgs e)
        {
            CommandList();
        }

        private void CommandList()
        {
            addMonitor($"Command List");
            addMonitor($"---------------------------");
            addMonitor($"  cm Command List");
            addMonitor($"  il Is Logged");
            addMonitor($"  li Login");
            addMonitor($"    -username");
            addMonitor($"    -password");
            addMonitor($"  lo LogOut");
            addMonitor($"  lu Lookup");
            addMonitor($"    -qrz");
            addMonitor($"  cw GetWorked");
            addMonitor($"    -qrz");
            addMonitor($"  lw Lookup and GetWorked");
            addMonitor($"    -qrz");
            addMonitor($"  hm QRZHome");
            addMonitor($"  lb GotoLogbook");
            addMonitor($"  qc QSOCount");
            addMonitor($"  lp LogbookPages");
            addMonitor($"  gp GotoPage");
            addMonitor($"    -page");
            addMonitor($"  pd Page Down");
            addMonitor($"  pu Page Up");
            addMonitor($"  cp Current Page");
            addMonitor($"  da Order Date Asc");
            addMonitor($"  dd Order Date Desc");
            addMonitor($"  tr Get Table Contente Raw");
            addMonitor($"    -page");
            addMonitor($"  tx GetTableContentXML");
            addMonitor($"    -page");
            addMonitor($"  cl Clear Monitor");
            addMonitor($"  sw Switch View");
            addMonitor($"---------------------------");
        }

        private void frmTest_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                txtCommand.SelectAll();
                txtCommand.Focus();
            }
        }
    }
}
