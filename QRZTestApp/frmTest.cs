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
            addMonitor($"Start IsLogged request...");
            addMonitor($"IsLogged = {qrz.IsLogged().ToString()}");
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            addMonitor($"Start Log Out...");
            addMonitor($"Logout = {qrz.LogOut().ToString()}");
        }

        private void btnQRZHome_Click(object sender, EventArgs e)
        {
            addMonitor($"Load QRZ Home...");
            addMonitor($"Load QRZ Home = {qrz.GotoQrzHome()}");
        }

        private void btnGotoLookbook_Click(object sender, EventArgs e)
        {
            addMonitor($"Load Logbook...");
            addMonitor($"Load Logbook = {qrz.GotoLogbook()}");
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            qrz.Qrz = txtUsername.Text;
            qrz.Password = txtPassword.Text;

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
            } else
            {
                addMonitor($"Unable to Login: username and password are required fields!");
            }
        }

        private void btnLookup_Click(object sender, EventArgs e)
        {
            string QRZtoSearch = txtQRZLookup.Text;

            SaveRegKey("lookup", QRZtoSearch);

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

            SaveRegKey("lookup", QRZtoSearch);

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
            addMonitor($"Get QSOs Count... ");
            addMonitor($"QSOs Count = {qrz.GetQSOsCount()}");
        }

        private void btnLogbookPages_Click(object sender, EventArgs e)
        {
            addMonitor($"Get Loogbook Pages... ");
            addMonitor($"Loogbook Pages = {qrz.GetLogbookPages()}");
        }

        private void btnCurrentPage_Click(object sender, EventArgs e)
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
        }

        private void btnGotoPage_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

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

            addMonitor($"Get Table content Raw of page {page}... ");
            addMonitor($"{qrz.GetLogbookPageContentRaw(page)}");
        }

        private void btnGetTableContentXML_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

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
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            if (page > 1)
                page = page - 1;

            txtLogbookPage.Text = page.ToString();

            btnGotoPage_Click(sender, e);
        }

        private void btnPageUp_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            page++;

            txtLogbookPage.Text = page.ToString();

            btnGotoPage_Click(sender, e);
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
            txtMonitor.Text = string.Empty;
        }

        private void btnOrderDateAsc_Click(object sender, EventArgs e)
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
            addMonitor($"Set Logbook Order Date Desc... ");
            int currOrder = qrz.SetLogbookDateOrder(1);
            if (currOrder == 1)
                addMonitor($"Logbook Order Date is \"Desc\"");
            else
                addMonitor($"Unable to Set Logbook Order Date Desc");
        }
    }
}
