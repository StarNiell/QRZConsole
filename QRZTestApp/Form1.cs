using CefSharp;
using QRZLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


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

            ctimer.Enabled = false;
            ctimer.Interval = 3000;
            ctimer.Tick += Timer_tick;
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

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }


        private void cwb_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            string log = $"LoadingStateChanged - IsLoading={e.IsLoading.ToString()}";
            Debug.WriteLine(log);
            addMonitor($"LoadingStateChanged - IsLoading={e.IsLoading.ToString()}");
        }

        private void addMonitor(string log)
        {
            if (textBox1.InvokeRequired)
            {
                var d = new SafeCallDelegate(addMonitor);
                textBox2.Invoke(d, new object[] { log });
            }
            else
            {
                textBox2.AppendText(log + Environment.NewLine);
                textBox2.SelectionStart = textBox2.Text.Length;
                textBox2.ScrollToCaret();
            }
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            cwb.LoadUrl("https://www.qrz.com/login");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            addMonitor($"username = {GetElementValue("username")}");
            SetElementValue("username", "IU8NQI");
            addMonitor($"username = {GetElementValue("username")}");
            ExecuteScript("next();");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            addMonitor($"password = {GetElementValue("password")}");
            SetElementValue("password", "Goldendrops2016");
            addMonitor($"password = {GetElementValue("password")}");
            ExecuteScript("next();");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            cwb.LoadUrl("https://logbook.qrz.com");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            SetElementValue("ipage", "5");
            ExecuteScript("goto('page')");
        }

        private string GetElementValue(string id, string attribute = "value")
        {
            var task = cwb.GetBrowser().MainFrame.EvaluateScriptAsync($"document.getElementById('{id}').{attribute};");
            task.Wait();

            var response = task.Result;

            return response.Result.ToString();

        }

        private void SetElementValue(string id, string value)
        {
            cwb.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.getElementById('{id}').value='{value}';");
        }

        private void ExecuteScript(string script)
        {
            cwb.GetBrowser().MainFrame.ExecuteJavaScriptAsync(script);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            textBox3.Text = GetElementValue("lbtab", "outerHTML"); // "document.getElementById('lbtab').outerHTML"
        }
    }
}
