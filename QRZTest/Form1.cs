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
        string lastCompletePageLoad = "";

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
            lastCompletePageLoad = e.Url.ToString();
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

        private void button4_Click(object sender, EventArgs e)
        {
            bool ret = true;
            string Url = "https://logbook.qrz.com/logbook/?op=add;addcall=OY1OF";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            wb.Navigate(Url);
            while (wb.ReadyState != WebBrowserReadyState.Complete && lastCompletePageLoad != Url)
            {
                Application.DoEvents();
                Thread.Sleep(CpuSleep);
                if (stopwatch.ElapsedMilliseconds >= 30000)
                    ret = false;
            }


            string qrzToCall = "OY1OF";
            string freq = "7.125";
            string mode = "SSB";
            string date = "2022-12-30";
            string time = "19:56";
            string comment;

            bool bOK = false;
            string bandFound = string.Empty;

            // Check the Date Format
            bOK = (DateTime.TryParse(date, out DateTime dummyDate));

            // Set the Date in to input data
            if (bOK)
            {
                bOK = false;
                HtmlElement start_date = wb.Document.GetElementById("start_date");
                HtmlElement end_date = wb.Document.GetElementById("end_date");
                if (start_date != null && end_date != null)
                {
                    start_date.SetAttribute("value", date);
                    end_date.SetAttribute("value", date);
                    bOK = true;
                }
            }

            // Check the Time Format
            if (bOK)
                bOK = TimeSpan.TryParse(time, out TimeSpan dummyTime);

            // Set the Time in to input data
            if (bOK)
            {
                bOK = false;
                HtmlElement start_time = wb.Document.GetElementById("start_time");
                HtmlElement end_time = wb.Document.GetElementById("end_time");
                if (start_time != null && end_time != null)
                {
                    start_time.SetAttribute("value", time);
                    end_time.SetAttribute("value", time);
                    bOK = true;
                }
            }

            // Check The Freq
            if (bOK)
            {
                bOK = false;
                HtmlElement freq2 = wb.Document.GetElementById("freq2");
                if (double.TryParse(freq.Replace(".", ","), out double dFreq))
                {
                    bandFound = "40m";
                    bOK = (!string.IsNullOrEmpty(bandFound));
                }
            }

            //Set the Freq
            if (bOK)
            {
                bOK = false;
                HtmlElement freq2 = wb.Document.GetElementById("freq2");
                if (freq2 != null)
                {
                    freq2.SetAttribute("value", freq);
                    wb.Document.InvokeScript("setFreq", new object[] { "2", "0" });
                    //wb.Document.InvokeScript("checkFreq", new object[] { "2", "0" });

                    HtmlElement band2 = wb.Document.GetElementById("band2");
                    if (band2 != null)
                    {
                        band2.SetAttribute("value", bandFound);
                        bOK = true;
                    }
                }
            }

            //Set the Mode
            if (bOK)
            {
                bOK = false;
                HtmlElement mode2 = wb.Document.GetElementById("mode2");
                if (mode2 != null)
                {
                    mode2.SetAttribute("value", mode);
                    wb.Document.InvokeScript("setMode", new object[] { mode, "1", "2" });
                    bOK = true;
                }
            }

            // Set RTS SENT
            if (bOK)
            {
                bOK = false;
                HtmlElement rst_sent = wb.Document.GetElementById("rst_sent");
                if (rst_sent != null)
                {
                    rst_sent.SetAttribute("value", "59");
                    bOK = true;
                }
            }

            // Set RTS RCVD
            if (bOK)
            {
                bOK = false;
                HtmlElement rst_rcvd = wb.Document.GetElementById("rst_rcvd");
                if (rst_rcvd != null)
                {
                    rst_rcvd.SetAttribute("value", "59");
                    bOK = true;
                }
            }

            //Set the comment
            if (bOK)
            {
                if (!string.IsNullOrEmpty(""))
                {
                    bOK = false;
                    HtmlElement comments1 = wb.Document.GetElementById("comments1");
                    if (comments1 != null)
                    {
                        comments1.SetAttribute("value", "");
                        bOK = true;
                    }

                }
            }

            if (bOK)
                ret = true;

            if (ret)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();

                bool PageLoaded = false;

                HtmlElement savebut = wb.Document.GetElementById("savebut");
                if (savebut != null)
                {
                    //object[] o = new object[1];
                    //o[0] = "0";
                    //wb.Document.InvokeScript("newQSOform", o);
                    savebut.InvokeMember("click");
                }

                while (!PageLoaded)
                {
                    if (stopwatch.ElapsedMilliseconds > 1000)
                    {
                        HtmlElement loadingDiv = wb.Document.GetElementById("filterLoading");
                        if (loadingDiv != null)
                            PageLoaded = (loadingDiv.Style.Contains("display: none"));
                    }

                    Application.DoEvents();
                    Thread.Sleep(CpuSleep);
                    if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                        break;
                }

                ret = PageLoaded;
            }

        }

    }
}
