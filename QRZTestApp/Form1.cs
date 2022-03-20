using QRZLibrary;
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
    public partial class Form1 : Form
    {
        Logbook qrz;

        private delegate void SafeCallDelegate(string text);

        Timer timer = new Timer();
        Timer ctimer = new Timer();
        int step = 0;

        public Form1()
        {
            InitializeComponent();

            qrz = new Logbook();
            timer.Enabled = false;
            timer.Interval = 3000;
            timer.Tick += Timer_tick;

            qrz.Qrz = "IU8NQI";
            qrz.Password = "Goldendrops2016";

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

        private void button1_Click(object sender, EventArgs e)
        {
            
            qrz.Login("IU8NQI", "Goldendrops2016");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate("https://www.qrz.com/login");

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Step1();

        }

        private void Step1()
        {
            var x = webBrowser1.Document.InvokeScript("eval", new object[] { "step" });
            step = (int)x;
            HtmlDocument d = webBrowser1.Document;
            HtmlElement uid = d.GetElementById("username");
            uid.SetAttribute("value", "IU8NQI");
            d.InvokeScript("next");
            timer.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Step2();
        }

        private void Step2()
        {
            HtmlDocument d = webBrowser1.Document;
            HtmlElement pwd = d.GetElementById("password");
            pwd.SetAttribute("value", "Goldendrops2016");
            d.InvokeScript("next");
        }

        private void Timer_tick(object seder, EventArgs e)
        {

            var x = webBrowser1.Document.InvokeScript("eval", new object[] { "step" });
            if (x != null)
            {
                if ((int)x == 2)
                {
                    timer.Enabled = false;
                    Step2();

                }
            }
            else
                timer.Enabled = false;

        }

        private void button5_Click(object sender, EventArgs e)
        {

            bool ret = NavigateAndWait("https://www.qrz.com", 30000);

            bool test = false;
            HtmlElement el = QRZHelper.GetElementByTagAndClassName(webBrowser1.Document.Body, "ul", "primary-navigation");
            if (el != null)
            {
                HtmlElement elm = QRZHelper.GetElementContainsValue(el, "IU8NQI");
                test = (elm != null);

            }

        }

        private bool NavigateAndWait(string Url, long timeout)
        {
            bool ret = false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            webBrowser1.Navigate(Url);
            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
                Thread.Sleep(50);
                if (stopwatch.ElapsedMilliseconds >= timeout)
                    return false;
            }

            return true;

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

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
            addMonitor($"Login... QRZ=[{qrz.Qrz}] - Password=[{new string('*', qrz.Password.Length)}]");
            qrz.Login();
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }
    }
}
