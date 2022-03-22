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
using Timer = System.Windows.Forms.Timer;

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

            qrz.LoggedIn += Qrz_LoggedIn;
            qrz.Error += Qrz_Error;

        }


        private void Qrz_Error(object sender, OnErrorEventArgs e)
        {
            addMonitor($"### {e.ex.Message}");
        }

        private void Qrz_LoggedIn(object sender, EventArgs e)
        {
            addMonitor($"Login completed.");
        }

        
        private void addMonitor(string log)
        {
            if (textBox1.InvokeRequired)
            {
                var d = new SafeCallDelegate(addMonitor);
                textBox1.Invoke(d, new object[] { log });
            }
            else
            {
                textBox1.AppendText(log + Environment.NewLine);
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
            }
            
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            addMonitor($"Start IsLogged request...");
            addMonitor($"IsLogged = {qrz.IsLogged().ToString()}");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            addMonitor($"Start Log Out...");
            addMonitor($"Logout = {qrz.LogOut().ToString()}");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            addMonitor($"Load QRZ Home...");
            addMonitor($"Load QRZ Home = {qrz.GotoQrzHome()}");
        }

        private void button15_Click(object sender, EventArgs e)
        {
            addMonitor($"Load Logbook...");
            addMonitor($"Load Logbook = {qrz.GotoLogbook()}");
        }

        private void button16_Click(object sender, EventArgs e)
        {
            qrz.Qrz = textBox2.Text;
            qrz.Password = textBox5.Text;
            SaveRegKey("username", qrz.Qrz);
            SaveRegKey("password", qrz.Password);

            addMonitor($"Login... QRZ=[{qrz.Qrz}] - Password=[{new string('*', qrz.Password.Length)}]");
            //qrz.LoginAsync();
            qrz.Login();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            string QRZtoSearch = textBox3.Text;

            SaveRegKey("lookup", QRZtoSearch);

            addMonitor($"ExecQuery {QRZtoSearch}...");
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

        private void button6_Click_1(object sender, EventArgs e)
        {
        }

        private void button7_Click(object sender, EventArgs e)
        {
            addMonitor($"Get QSOs Count... ");
            addMonitor($"QSOs Count = {qrz.GetQSOsCount()}");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            addMonitor($"Get Loogbook Pages... ");
            addMonitor($"Loogbook Pages = {qrz.GetLogbookPages()}");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            addMonitor($"Get Current Loogbook Page... ");
            addMonitor($"Current Loogbook Page = {qrz.GetCurrentLogbookPage()}");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox2.Text = GetRegKeyValue("username");
            textBox5.Text = GetRegKeyValue("password");
            textBox3.Text = GetRegKeyValue("lookup");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(textBox4.Text, out int tmp))
                page = tmp;

            addMonitor($"Goto Page {page}... ");
            addMonitor($"Current Page = {qrz.GotoLoogbookPage(page)}");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(textBox4.Text, out int tmp))
                page = tmp;

            addMonitor($"Get Table content Raw {page}... ");
            addMonitor($"{qrz.GetLogbookPageContentRaw(page)}");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(textBox4.Text, out int tmp))
                page = tmp;

            addMonitor($"Get Table content {page}... ");

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

        private void button3_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(textBox4.Text, out int tmp))
                page = tmp;

            if (page > 1)
                page = page - 1;

            textBox4.Text = page.ToString();

            button10_Click(sender, e);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(textBox4.Text, out int tmp))
                page = tmp;

            page++;

            textBox4.Text = page.ToString();

            button10_Click(sender, e);
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
    }
}