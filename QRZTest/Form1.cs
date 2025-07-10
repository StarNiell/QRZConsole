using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
        private string errorMessage = "";
        private string _qrzUrl = "https://www.qrz.com/";
        private string _logbookUrl = "https://logbook.qrz.com/";
        private string _lookupUrl = "https://www.qrz.com/lookup/";
        private string _addCallUrl = "https://logbook.qrz.com/logbook/?op=add;addcall=";

        private bool wbroDocumentCompleted = false;

        WebBrowser wb;

        public Form1()
        {
            InitializeComponent();

        }

        private async Task InitWB2()
        {
            string environmentFolderName = Path.Combine(Path.GetTempPath(), $"QRZConsole");
            CoreWebView2Environment cwv2Environment = await CoreWebView2Environment.CreateAsync(null, environmentFolderName, new CoreWebView2EnvironmentOptions());
            await wb2.EnsureCoreWebView2Async(cwv2Environment);
            wb2.NavigationCompleted += Wb2_NavigationtCompleted;

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            wb2.CoreWebView2.Navigate("https://logbook.qrz.com/");
        }

        private void Button2_Click(object sender, EventArgs e)
        {
           
        }



        private async void Button3_Click(object sender, EventArgs e)
        {
           int page = await GotoLoogbookPage(10);
            MessageBox.Show($"Current page: {page}");

        }

        private void Wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            flagDocumentCompleted = true;
            lastCompletePageLoad = e.Url.ToString();
        }

        private void AddMonitor(string log, bool appendNewLine = true)
        {
            if (txtMonitor.InvokeRequired)
            {
                var d = new SafeCallDelegate(AddMonitor);
                txtMonitor.Invoke(d, new object[] { log });
            }
            else
            {
                txtMonitor.AppendText(log + (appendNewLine ? Environment.NewLine : string.Empty));
                txtMonitor.SelectionStart = txtMonitor.Text.Length;
                txtMonitor.ScrollToCaret();
            }
        }

        private async void Button4_Click(object sender, EventArgs e)
        {
            await LogOut();
        }

        public bool GotoQrzHome()
        {
            return NavigateAndWait(_qrzUrl);
        }

        public async Task<bool> IsLogged()
        {
            bool ret = false;
            bool pageLoaded;

            if (wb2.CoreWebView2.Source != null && wb2.CoreWebView2.Source != "about:blank")
            {
                pageLoaded = (wb2.Source.OriginalString.Contains(_qrzUrl))
                          || (wb2.Source.OriginalString.Contains(_logbookUrl))
                          || (wb2.Source.OriginalString.Contains(_lookupUrl));
            }
            else
                pageLoaded = GotoQrzHome();

            if (pageLoaded)
            {
                string s = await GetScriptResult("document.querySelector('nav ul.primary-navigation').textContent.includes('Logout');");
                ret = (s == "true");
            }
            return ret;
        }
        public async Task<bool> LogOut()
        {
            bool ret = false;

            if (await IsLogged())
            {
                string elm = await GetScriptResult("document.querySelector('ul.primary-navigation').textContent.includes('Logout');");
                if (elm != "")
                {

                    string s = await wb2.CoreWebView2.ExecuteScriptAsync("document.querySelector('#qrztop > nav > ul.primary-navigation > li.leaf.last > ul > li:nth-child(7) > a')");
                    if (s != "null")
                    {
                        await wb2.CoreWebView2.ExecuteScriptAsync("document.querySelector('#qrztop > nav > ul.primary-navigation > li.leaf.last > ul > li:nth-child(7) > a').click();");
                        ret = true;
                    }
                }
            }
            return ret;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await InitWB2();
        }

        private async Task<string> GetElementValueAsync(string id, string attribute = "value")
        {
            string s = await wb2.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{id}').getAttribute('{attribute}')");
            return s.Replace("\"", "");
        }

        private async Task<string> GetHTMLTextElement(string id)
        {
            string s = await wb2.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{id}').innerHTML");
            return s; //.Replace("\"", "");
        }

        private async Task<HtmlElement> GetHTMLElementAsync(string parent, string id)
        {
            wb = new WebBrowser();
            wbroDocumentCompleted = false;
            //wb.Navigating += new WebBrowserNavigatingEventHandler(wb_Navigating);
            wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(wbro_DocumentCompleted);
            string g = await GetHTMLTextElement(parent);
            g = Regex.Unescape(g);
            g = g.Remove(0, 1);
            g = g.Remove(g.Length - 1, 1);

            wb.DocumentText = g;

            while (!wbroDocumentCompleted)
            {
                Application.DoEvents();
                Thread.Sleep(CpuSleep);
            }

            wb.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(wbro_DocumentCompleted);

            var ret = wb.Document.GetElementById(id);

            wb = null;

            return ret;
        }


        //private async Task<object> GetJSValue(string jsCode)
        //{
        //    return await wb2.CoreWebView2.ExecuteScriptAsync($"eval('{jsCode}')");
        //}

        private async void SetElementValue(string id, string value)
        {
            await wb2.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{id}').value='{value}'");
        }

        //private void SetValueByParentElementAndTagnameAndClassName(string parentId, string tagname, string classname, string value)
        //{
        //    wb2.CoreWebView2.ExecuteScriptAsync($"document.querySelector('#{parentId} {tagname}.{classname}').value='{value}'");
        //}

        private async void ExecuteScript(string script)
        {
            await wb2.CoreWebView2.ExecuteScriptAsync(script);
        }

        private void Navigate(string Url)
        {
            wb2.CoreWebView2.Navigate(Url);
        }

        private async Task<string> GetScriptResult(string script)
        {
            string t = await wb2.CoreWebView2.ExecuteScriptAsync(script);
            return t.Replace("\"", "");
        }

        private async Task<string> EvalJSValueById(string id)
        {
            return await EvalJS($"document.getElementById('{id}').value");
        }

        private async Task<string> EvalJS(string script)
        {
            string t = "";
            if (wb2.CoreWebView2 != null)
            {
                try
                {
                    t = await wb2.CoreWebView2.ExecuteScriptAsync(script);
                    t = t.Replace("\"", "");
                    Console.WriteLine(t);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"EvalJS Error: {ex.Message}");
                }
            }
            return t;
        }

        public async Task<bool> Login(string Qrz, string Password)
        {
            bool ret = false;
            errorMessage = string.Empty;
            bool stepDone = false;
            bool waitDone = false;
            Stopwatch stopwatch;


            if (NavigateAndWait("https://www.qrz.com/login"))
            {
                if (wb2.CoreWebView2.DocumentTitle == "Login - QRZ.com")
                {
                    // Set the username ***************************************************************
                    SetElementValue("username", Qrz);

                    await EvalJSValueById("username");

                    ExecuteScript("next();");

                    //wait for new step -----------
                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    string fullname = "";

                    while (!waitDone)
                    {
                        fullname = await GetScriptResult("document.getElementById('fullname') ? document.getElementById('fullname').innerText : ''");

                        stepDone = waitDone = (fullname != "" && fullname != "Non Ham User");
                        if (!waitDone)
                        {
                            string usernameError = await GetScriptResult("document.getElementById('usernameError').innerText");
                            if (usernameError != "")
                            {
                                errorMessage = "Invalid username!";
                                waitDone = true;
                            }
                        }

                        Thread.Sleep(CpuSleep);
                        if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                        {
                            break;
                        }
                    }

                    if (!stepDone)
                        return false;

                    // Set the password ***************************************************************
                    SetElementValue("password", Password);
                    ExecuteScript("next();");

                    //wait for new step -----------
                    waitDone = false;
                    stepDone = false;

                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    while (!waitDone)
                    {
                        if (stopwatch.ElapsedMilliseconds > 1000)
                        {
                            stepDone = waitDone = (wb2.CoreWebView2.DocumentTitle == "Callsign Database - QRZ.com");
                            if (wb2.CoreWebView2.DocumentTitle == "Login - QRZ.com")
                            {
                                string alertContainer = await GetScriptResult("document.querySelector('div.alert-container div.alert.alert-warning') ? document.querySelector('div.alert-container div.alert.alert-warning').innerText : ''");
                                if (alertContainer != "")
                                {
                                    string alertWarning = await GetScriptResult("document.querySelector('div.alert-container div.alert.alert-warning').innerText");
                                    if (alertWarning == "Authentication error: \"Invalid username or password!\"")
                                    {
                                        errorMessage = "Invalid username or password!";
                                        waitDone = true;
                                    }
                                }
                            }
                        }

                        //Application.DoEvents();
                        Thread.Sleep(CpuSleep);
                        if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                        {
                            break;
                        }
                    }

                    if (!stepDone)
                        return false;
                    else
                        ret = true;

                }
            }
            return ret;
        }
        private void Wb2_NavigationtCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            //InjectAlertBlocker();
            flagDocumentCompleted = true;
            lastCompletePageLoad = wb2.Source.ToString();
            string log = $"Wb2_NavigationtCompleted - Url={lastCompletePageLoad}";
            Debug.WriteLine(log);
        }

        private bool NavigateAndWait(string Url)
        {
            NavigateAndWait(Url, _pageLoadTimeOut);
            Debug.WriteLine($"***** NavigateAndWait: {Url} COMPLETE *******");
            return true;
        }

        private bool NavigateAndWait(string Url, long timeout)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            wb2.CoreWebView2.Navigate(Url);
            while (lastCompletePageLoad != Url)
            {
                Application.DoEvents();
                Thread.Sleep(CpuSleep);
                if (stopwatch.ElapsedMilliseconds >= timeout)
                    return false;
            }

            return true;
        }

        public async Task<bool> GotoLogbook(bool ForceReload = false)
        {
            bool ret = false;
            ret = (!ForceReload && wb2.CoreWebView2.DocumentTitle == "Logbook by QRZ.COM");
            ret = ret && await GetScriptResult("document.getElementById('button-addon4') != null && document.getElementById('addcall') != null;") == "true";
            if (!ret)
            {
                if (NavigateAndWait(_logbookUrl))
                    ret = wb2.CoreWebView2.DocumentTitle == "Logbook by QRZ.COM";
            }

            return ret;
        }

        private async Task SetElementValueAsync(string id, string value)
        {
            await wb2.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{id}').value = '{value}'");
        }

        private async Task ExecuteScriptAsync(string script)
        {
            await wb2.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task<int> GetCurrentLogbookPage()
        {
            int ret = -1;
            if (await GotoLogbook())
            {
                if (wb2.CoreWebView2.DocumentTitle == "Logbook by QRZ.COM")
                {
                    await EvalJS("document.getElementById('ipage').value");

                    if (await GetScriptResult("document.getElementById('button-addon4') != null") == "true")
                    {
                        if (await GetScriptResult("document.getElementById('ipage') != null") == "true")
                        {
                            string p = await GetElementValueAsync("ipage");

                            if (int.TryParse(p, out int tmp))
                                ret = tmp;
                        }
                    }
                }
            }
            return ret;
        }

        public async Task<int> GotoLoogbookPage(int page)
        {
            int ret = -1;

            if (await GotoLogbook())
            {
               if (await GetScriptResult("document.getElementById('ipage')") != "null")
                {
                    if (await GetElementValueAsync("ipage") != page.ToString())
                    {
                        await SetElementValueAsync("ipage", page.ToString());

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        bool PageLoaded = false;
                        await ExecuteScriptAsync("goto('page')");
                        while (!PageLoaded)
                        {
                            if (stopwatch.ElapsedMilliseconds > 1000)
                            {
                                if (await GetScriptResult("document.getElementById('filterLoading')") != "null")
                                {
                                    await EvalJS("document.getElementById('filterLoading').style");
                                    string style = await GetScriptResult("document.getElementById('filterLoading').getAttribute('style')");
                                    if (style.Contains("display:none"))
                                    {
                                        PageLoaded = true;
                                    }
                                }
                                Thread.Sleep(CpuSleep);
                                if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                                    return -1;
                            }
                        }
                        ret = await GetCurrentLogbookPage();
                    }
                    else
                        ret = page;

                }
            }
            return ret;
        }

        
        private void wbro_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            wbroDocumentCompleted = true;
        }


        public async Task<string> GetLogbookPageContentRaw(int page)
        {
            string ret = string.Empty;

            if (await GotoLoogbookPage(page) == page)
            {
                //wb = new WebBrowser();
                //wbroDocumentCompleted = false;
                ////wb.Navigating += new WebBrowserNavigatingEventHandler(wb_Navigating);
                //wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(wbro_DocumentCompleted);
                //string g = await GetHTMLTextElement("lblistrs");
                //g = Regex.Unescape(g);
                //g = g.Remove(0, 1);
                //g = g.Remove(g.Length - 1, 1);

                //wb.DocumentText = g;

                //while (!wbroDocumentCompleted)
                //{
                //    Application.DoEvents();
                //    Thread.Sleep(CpuSleep);
                //}

                //wb.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(wbro_DocumentCompleted);

                //HtmlElement table = wb.Document.GetElementById("lbtab");

                HtmlElement table = await GetHTMLElementAsync("lblistrs", "lbtab");
                HtmlElementCollection rows = table.GetElementsByTagName("TR");

                foreach (HtmlElement row in rows)
                {
                    HtmlElementCollection cols = row.GetElementsByTagName("TD");
                    int colCount = 0;
                    bool validRow = false;
                    foreach (HtmlElement cell in cols)
                    {
                        colCount++;
                        if (colCount == 1)
                        {
                            if (cell.InnerText != null)
                            {
                                validRow = int.TryParse(cell.InnerText.Trim(), out int tmp);
                            }
                        }
                        if (validRow)
                            ret += cell.InnerText + "\t";
                    }
                    if (validRow)
                        ret += "\r\n";
                }

            }
            return ret;
        }
    }
}
