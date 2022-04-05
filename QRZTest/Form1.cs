using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QRZTest
{
    public partial class Form1 : Form
    {
        private bool flagDocumentCompleted = false;
        private long _pageLoadTimeOut = 30000;
        private int CpuSleep = 50;
        private delegate void SafeCallDelegate(string log, bool appendNewLine);

        public Form1()
        {
            InitializeComponent();

            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
               @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
               true))
            {
                string app = $"{Assembly.GetEntryAssembly().GetName().Name}.exe";
                key.SetValue(app, 11001, Microsoft.Win32.RegistryValueKind.DWord);
                key.Close();
            }

            wb.ScriptErrorsSuppressed = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            wb.Navigate("https://logbook.qrz.com/");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bool PageLoaded = false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            flagDocumentCompleted = false;

            wb.Document.InvokeScript("lb_go", new object[] { "edit", "" });

            while (!PageLoaded)
            {
                if (flagDocumentCompleted)
                {
                    if ((wb.Document.Title == "Logbook by QRZ.COM"))
                    {
                        HtmlElement start_date = wb.Document.GetElementById("start_date");
                        PageLoaded = (start_date != null);
                    }
                }

                Application.DoEvents();
                Thread.Sleep(CpuSleep);
                if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                {
                    break;
                }

            }

            if (PageLoaded)
            {
                addMonitor($"Button 2 PageLoaded ");
            }
            else
            {
                addMonitor($"Button 2 NOT PageLoaded ");
            }

        }



        private void button3_Click(object sender, EventArgs e)
        {
            HtmlElement lbmenu = wb.Document.GetElementById("lbmenu");
            lbmenu.GetElementsByTagName("input").GetElementsByName("op")[0].SetAttribute("value", "logdel");

            bool PageLoaded = false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            flagDocumentCompleted = false;
            lbmenu.InvokeMember("submit");
            while (!PageLoaded)
            {
                if (flagDocumentCompleted)
                {
                    if ((wb.Document.Title == "Logbook by QRZ.COM"))
                    {
                        HtmlElement addcall = wb.Document.GetElementById("addcall");
                        PageLoaded = (addcall != null);
                    }

                }

                Application.DoEvents();
                Thread.Sleep(CpuSleep);
                if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                {
                    break;
                }
            }

            if (PageLoaded)
            {
                addMonitor($"Button 3 PageLoaded ");
            }
            else
            {
                addMonitor($"Button 3 NOT PageLoaded ");
            }
        }

        private void wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            flagDocumentCompleted = true;
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

    }
}
