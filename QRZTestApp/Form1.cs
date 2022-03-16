using QRZLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        Timer timer = new Timer();
        int step = 0;

        public Form1()
        {
            InitializeComponent();

            qrz = new Logbook();
            timer.Enabled = false;
            timer.Interval = 3000;
            timer.Tick += Timer_tick;

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
            if ((int)x == 2)
            {
                timer.Enabled = false;
                Step2();

            }

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }
    }
}
