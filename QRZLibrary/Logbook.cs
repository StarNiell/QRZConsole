using QRZLibrary.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace QRZLibrary
{
    public class Logbook
    {

        WebBrowser wb;

        WBActions wbAction;
        Timer tmrActionState = new Timer();

        public delegate void LoggedInEventHaldler(object sender, EventArgs e);
        public event LoggedInEventHaldler LoggedIn;
        public delegate void ErrorEventHaldler(object sender, OnErrorEventArgs e);
        public event ErrorEventHaldler Error;

        private int PagesLoadedAfterLogin = 0;
        private bool flagDocumentCompleted = false;

        private string _qrzUrl = "https://www.qrz.com";
        private string _logbookUrl = "https://logbook.qrz.com";
        private string _lookupUrl = "https://www.qrz.com/lookup";
        private string _qrz;
        private string _password;
        private string _loginPage = "/login";
        private long _pageLoadTimeOut = 30000;
        private int _cpuSleep = 50;

        public string Qrz { get => _qrz; set => _qrz = value; }
        public string Password { get => _password; set => _password = value; }
        public string QrzUrl { get => _qrzUrl; set => _qrzUrl = value; }
        public string LogbookUrl { get => _logbookUrl; set => _logbookUrl = value; }
        public long PageLoadTimeOut { get => _pageLoadTimeOut; set => _pageLoadTimeOut = value; }
        public int CpuSleep { get => _cpuSleep; set => _cpuSleep = value; }
        public string LookupUrl { get => _lookupUrl; set => _lookupUrl = value; }

        public Logbook()
        {

            QRZHelper.initWBEdge11();
            
            wb = new WebBrowser();
            wb.ScriptErrorsSuppressed = true;
            wb.DocumentCompleted += Wb_DocumentCompleted;

            //For Async methods
            tmrActionState.Enabled = false;
            tmrActionState.Interval = 1000;
            tmrActionState.Tick += tmrActionState_Tick;

        }

        #region Private Methods

        private void Wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            flagDocumentCompleted = true;

            string log = $"Wb_DocumentCompleted - Url={e.Url}";
            Debug.WriteLine(log);

            ActionStateChanged();

        }

        private bool NavigateAndWait(string Url)
        {
            return NavigateAndWait(Url, PageLoadTimeOut);
        }

        private bool NavigateAndWait(string Url, long timeout)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            wb.Navigate(Url);
            while (wb.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
                Thread.Sleep(CpuSleep);
                if (stopwatch.ElapsedMilliseconds >= timeout)
                    return false;
            }

            return true;
        }

        private void InvokeMember(HtmlElement el, string memberToInvoke)
        {
            InvokeMember(el, memberToInvoke, PageLoadTimeOut);
        }

        private void InvokeMember(HtmlElement el, string memberToInvoke, long timeout)
        {
            el.InvokeMember(memberToInvoke);
        }


        private bool InvokeMemberAndWaitReload(HtmlElement el, string memberToInvoke)
        {
            return InvokeMemberAndWaitReload(el, memberToInvoke, PageLoadTimeOut);
        }

        private bool InvokeMemberAndWaitReload(HtmlElement el, string memberToInvoke, long timeout)
        {
            bool InvokeCompleted = false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            flagDocumentCompleted = false;
            el.InvokeMember(memberToInvoke);
            while (!InvokeCompleted)
            {
                if (stopwatch.ElapsedMilliseconds > 1000)
                    InvokeCompleted = flagDocumentCompleted;

                Application.DoEvents();
                Thread.Sleep(CpuSleep);
                if (stopwatch.ElapsedMilliseconds >= timeout)
                    return false;
            }

            return true;
        }

        private void setUID()
        {
            Debug.WriteLine($"setUID()");
            SetElementValue("username", Qrz);
            ExecuteScript("next");
            tmrActionState.Enabled = true;
        }

        private void setPWD()
        {
            Debug.WriteLine($"setPWD()");
            SetElementValue("password", Password);
            wbAction = WBActions.openQRZ;
            ExecuteScript("next");
        }

        private string GetElementValue(string id, string attribute = "value")
        {
            HtmlElement el = wb.Document.GetElementById(id);
            return el.GetAttribute(attribute);
        }

        private object GetJSValue(string jsCode)
        {
            return wb.Document.InvokeScript("eval", new object[] { jsCode });
        }

        private void SetElementValue(string id, string value)
        {
            HtmlDocument d = wb.Document;
            HtmlElement el = d.GetElementById(id);
            el.SetAttribute("value", value);
        }

        private void ExecuteScript(string script)
        {
            wb.Document.InvokeScript(script);
        }

        private void Navigate(string Url)
        {
            wb.Navigate(Url);
        }

        private void tmrActionState_Tick(object seder, EventArgs e)
        {

            int step = (int)GetJSValue("step");
            if (step == 2)
            {
                Debug.WriteLine($"tmrActionState_Tick()");
                tmrActionState.Enabled = false;
                wbAction = WBActions.loginPWD;
                ActionStateChanged();
            }
        }

        private void ActionStateChanged()
        {
            Debug.WriteLine($"wbAction: {wbAction.ToString()} - Title {wb.Document.Title}");
            switch (wbAction)
            {
                case WBActions.start:
                    wbAction = WBActions.loginUID;
                    setUID();
                    break;
                case WBActions.loginPWD:
                    setPWD();
                    break;
                case WBActions.openQRZ:
                    PagesLoadedAfterLogin++;
                    if (wb.Document.Title == "Callsign Database - QRZ.com")
                    {
                        PagesLoadedAfterLogin = 0;
                        wbAction = WBActions.noAction;
                        if (LoggedIn != null)
                            LoggedIn(this, new EventArgs());
                    }
                    else
                    {
                        if (PagesLoadedAfterLogin >= 1)
                        {
                            PagesLoadedAfterLogin = 0;
                            wbAction = WBActions.noAction;
                            if (Error != null)
                            {
                                OnErrorEventArgs errevarg = new OnErrorEventArgs();
                                errevarg.ex = new Exception("Unable to Login QRZ.COM. Check Username and password");
                                Error(this, errevarg);
                            }
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Public Methods

        public bool IsLogged()
        {
            bool ret = false;
            bool pageLoaded = false;

            if (wb.Url != null)
            {
                pageLoaded = (wb.Url.OriginalString.Contains(_qrzUrl))
                          || (wb.Url.OriginalString.Contains(_logbookUrl))
                          || (wb.Url.OriginalString.Contains(_lookupUrl));
            }
                
            else
                pageLoaded = GotoQrzHome();


            if (pageLoaded)
            {
                HtmlElement el = QRZHelper.GetElementByTagAndClassName(wb.Document.Body, "ul", "primary-navigation");
                if (el != null)
                {
                    HtmlElement el1 = QRZHelper.GetLastChildren(el, "LI");
                    if (el1 != null)
                    {
                        HtmlElement sub = QRZHelper.GetElementByTagAndClassName(el1, "ul", "sub");
                        if (sub != null)
                        {
                            HtmlElement elm = QRZHelper.GetElementContainsValue(sub, "Logout");
                            ret = (elm != null);
                        }
                    }
                }
            }
            return ret;
        }

        public bool Login()
        {
            return Login(Qrz, Password);
        }

        public bool Login(string qrz, string password)
        {
            bool ret = false;

            bool stepDone = false;
            bool waitDone = false;
            Stopwatch stopwatch = null;

            Qrz = qrz;
            Password = password;

            if (NavigateAndWait(QrzUrl + _loginPage))
            {
                if (wb.Document.Title == "Login - QRZ.com")
                {
                    // Set the username ***************************************************************
                    SetElementValue("username", Qrz);
                    ExecuteScript("next");

                    //wait for new step -----------
                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    while (!waitDone)
                    {
                        stepDone = waitDone = (wb.Document.GetElementById("trustInfo") != null);
                        waitDone = (wb.Document.GetElementById("usernameError") != null);

                        Application.DoEvents();
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
                    ExecuteScript("next");

                    //wait for new step -----------
                    waitDone = false;
                    stepDone = false;

                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    while (!waitDone)
                    {
                        if (stopwatch.ElapsedMilliseconds > 1000)
                        {
                            stepDone = waitDone = (wb.Document.Title == "Callsign Database - QRZ.com");
                            waitDone = (wb.Document.Title == "Login - QRZ.com");
                        }

                        Application.DoEvents();
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

        public void LoginAsync()
        {
            LoginAsync(Qrz, Password);
        }

        public void LoginAsync(string qrz, string password)
        {
            Qrz = qrz;

            Password = password;

            if (NavigateAndWait(QrzUrl + _loginPage))
            {
                if (wb.Document.Title == "Login - QRZ.com")
                {
                    wbAction = WBActions.start;
                    ActionStateChanged();
                }
                else
                {
                    if (IsLogged())
                    {
                        wbAction = WBActions.noAction;
                        if (LoggedIn != null)
                            LoggedIn(this, new EventArgs());
                    }
                    else
                    {
                        if (Error != null)
                        {
                            OnErrorEventArgs e = new OnErrorEventArgs();
                            e.ex = new Exception($"Unable to Login QRZ.com. Current page title is: [{wb.Document.Title ?? string.Empty}]");
                            Error(this, e);
                        }
                    }

                }
            }

        }

        public bool LogOut()
        {
            bool ret = false;

            if (IsLogged())
            {
                HtmlElement el = QRZHelper.GetElementByTagAndClassName(wb.Document.Body, "ul", "primary-navigation");
                if (el != null)
                {
                    HtmlElement el1 = QRZHelper.GetLastChildren(el, "LI");
                    if (el1 != null)
                    {
                        HtmlElement sub = QRZHelper.GetElementByTagAndClassName(el1, "ul", "sub");
                        if (sub != null)
                        {
                            HtmlElement elm = QRZHelper.GetElementContainsValue(sub, "Logout");
                            if (elm != null)
                            {
                                HtmlElement link = elm.FirstChild;
                                if (link != null)
                                {
                                    ret = InvokeMemberAndWaitReload(link, "Click");
                                }
                            }
                            
                        }
                    }

                }
            }
            return ret;
        }

        public bool GotoQrzHome()
        {
            return NavigateAndWait(QrzUrl);
        }

        public bool GotoLogbook(bool ForceReload = false)
        {
            bool ret = false;
            ret = (!ForceReload && wb.Document != null && wb.Document.Title == "Logbook by QRZ.COM");
            if (!ret)
            {
                if (NavigateAndWait(LogbookUrl))
                    ret = wb.Document.Title == "Logbook by QRZ.COM";
            }

            return ret;
        }

        public bool GotoLookup(bool ForceReload = false)
        {
            bool ret = false;
            ret = (!ForceReload && wb.Document != null && wb.Document?.GetElementById("tquery") != null);
            if (!ret) {
                if (NavigateAndWait(LookupUrl))
                    ret = wb.Document.Title.Equals("QRZ Callsign Database Search by QRZ Ham Radio");
            }
            return ret;
        }

        public LookupEntry ExecQuery(string QRZsearch)
        {
            LookupEntry ret = null; ;
            bool queryTimeOut = false;
            long timeout = PageLoadTimeOut;
            bool resultPageLoaded = false;
            bool resultFound = false;

            if (GotoLookup())
            {
                SetElementValue("tquery", QRZsearch.ToUpper());
                HtmlElement sbmt = wb.Document.GetElementById("tsubmit");
                if (sbmt != null)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    flagDocumentCompleted = false;
                    InvokeMember(sbmt, "Click");
                    while (!resultPageLoaded)
                    {
                        if (flagDocumentCompleted)
                        {
                            if ((wb.Document.GetElementById("qrzcenter") != null))
                                resultPageLoaded = (wb.Document.GetElementById("qrzcenter").OuterText.IndexOf($"no results for {QRZsearch.ToUpper()}") > 0);
                            resultPageLoaded = resultPageLoaded || (wb.Document.GetElementById("csdata") != null) || (wb.Document.GetElementById("csdata") != null);
                        }

                        Application.DoEvents();
                        Thread.Sleep(CpuSleep);
                        if (stopwatch.ElapsedMilliseconds >= timeout)
                        {
                            queryTimeOut = true;
                            break;
                        }
                    }
                    if (!queryTimeOut)
                    {
                        LookupEntry entry = new LookupEntry();
                        resultFound = (wb.Document.GetElementById("csdata") != null);
                        if (resultFound)
                        {
                            HtmlElement csdata = wb.Document.GetElementById("csdata");
                            if (csdata != null)
                            {
                                ExecuteScript("showqem");
                                entry.QRZ = csdata.Children[0].OuterText.Trim();
                                entry.DXCC = csdata.Children[1].OuterHtml.Substring(csdata.Children[1].OuterHtml.IndexOf("?dxcc=") + 6, 3);
                                entry.Country = csdata.Children[1].OuterText.Trim();
                                if (csdata.Children[3].Children.Count > 0)
                                    entry.Name = csdata.Children[3].Children[0].OuterText?.Trim();
                                if (csdata.Children[3].OuterText != null)
                                {
                                    string[] rawArray = csdata.Children[3].OuterText.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    entry.Address1 = rawArray.Length > 1 ? rawArray[1] : String.Empty;
                                    entry.Address2 = rawArray.Length > 2 ? rawArray[2] : String.Empty;
                                    entry.Address3 = rawArray.Length > 3 ? rawArray[3] : String.Empty;
                                }
                                entry.Email = (csdata.Children[5].OuterText != null) ? csdata.Children[5].OuterText.Replace("Email: ", "") : string.Empty;
                            }
                        }
                        else
                        {
                            entry.QRZ = QRZsearch;
                            entry.Name = "No result found!";
                        }
                        ret = entry;
                    }
                }
            }
            return ret;
        }

        public int GetQSOsCount(bool ForceReload = false)
        {
            int ret = -1;

            if (GotoLogbook(ForceReload))
            {
                HtmlElement el = wb.Document.GetElementById("lbmenu");
                if (el != null)
                {
                    HtmlElement el1 = QRZHelper.GetElementByTagAndClassName(el, "SPAN", " hide-lt-800 qcnt");
                    if (el1 != null)
                    {
                        HtmlElement el2 = QRZHelper.GetElementByTagAndClassName(el1, "SPAN", "mitm");
                        if (int.TryParse(el2.InnerText, out int tmp))
                            ret = tmp;
                    }
                }

            }

            return ret;
        }


        
        public int GetLogbookPages(bool ForceReload = false)
        {
            int ret = -1;

            if (GotoLogbook(ForceReload))
            {
                HtmlElement el = wb.Document.GetElementById("listnav");
                if (el != null)
                {
                    HtmlElement el1 = QRZHelper.GetElementByTagAndClassName(el, "SPAN", "input-group-text  hide-lt-1100");
                    if (el1 != null)
                    {
                        string pages = el1.InnerText?.Replace("of ", "");
                        if (int.TryParse(pages, out int tmp))
                            ret = tmp;
                    }
                }

            }

            return ret;
        }

        public int GetCurrentLogbookPage()
        {
            int ret = -1;

            if (wb.Document != null && wb.Document.Title == "Logbook by QRZ.COM")
            {
                HtmlElement el = wb.Document.GetElementById("ipage");
                if (el != null)
                    if (int.TryParse(el.GetAttribute("value"), out int tmp))
                        ret = tmp;
            }
            return ret;
        }


        public int GotoLoogbookPage(int page)
        {
            int ret = -1;

            if (wb.Document != null && wb.Document.Title == "Logbook by QRZ.COM")
            {
                HtmlElement el = wb.Document.GetElementById("ipage");
                if (el != null)
                {
                    int cp = -1;

                    el.SetAttribute("value", page.ToString());

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    bool PageLoaded = false;
                    object[] o = new object[1];
                    o[0] = "page";
                    wb.Document.InvokeScript("goto", o);
                    while (!PageLoaded)
                    {
                        if (stopwatch.ElapsedMilliseconds > 1000)
                        {
                            HtmlElement loadingDiv = wb.Document.GetElementById("filterLoading");
                            PageLoaded = (loadingDiv.Style.Contains("display: none"));
                        }

                        Application.DoEvents();
                        Thread.Sleep(CpuSleep);
                        if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                            return -1;

                    }
                    ret = GetCurrentLogbookPage();
                    ;
                }
                   
            }
            return ret;
        }

        public string GetLogbookPageContentRaw(int page)
        {
            string ret = string.Empty;

            if (GotoLoogbookPage(page) == page)
            {
                HtmlElement table = wb.Document.GetElementById("lbtab");
                HtmlElementCollection rows = table.GetElementsByTagName("TR");

                foreach (HtmlElement row in rows)
                {
                    HtmlElementCollection cols = row.GetElementsByTagName("TD");
                    foreach (HtmlElement cell in cols)
                    {
                        ret += cell.InnerText + "\t";
                    }
                    ret += "\r\n";
                }

            }
            return ret;
        }

        public List<LogbookEntry> GetLogbookPageContent(int page)
        {
            List<LogbookEntry> ret = new List<LogbookEntry>();
            if (GotoLoogbookPage(page) == page)
            {
                HtmlElement table = wb.Document.GetElementById("lbtab");
                HtmlElementCollection rows = table.GetElementsByTagName("TR");

                foreach (HtmlElement row in rows)
                {
                    LogbookEntry lbrow = new LogbookEntry();
                    HtmlElementCollection cols = row.GetElementsByTagName("TD");
                    int currCol = 0;
                    string tmp = string.Empty;
                    string DateTimeStr = string.Empty;
                    foreach (HtmlElement cell in cols)
                    {
                        currCol++;
                        switch (currCol)
                        {
                            case 1:
                                lbrow.position = QRZHelper.GetIntByString(cell.InnerText);
                                break;
                            case 2:
                                DateTimeStr = cell.InnerText;
                                break;
                            case 3:
                                DateTimeStr += $" {cell.InnerText}";
                                lbrow.QSODateTime = QRZHelper.GetDateTimeByString(DateTimeStr);
                                break;
                            case 4:
                                lbrow.Call = cell.InnerText;
                                break;
                            case 5:
                                lbrow.Band = cell.InnerText;
                                break;
                            case 6:
                                lbrow.Frequency = cell.InnerText;
                                break;
                            case 7:
                                lbrow.Mode = cell.InnerText;
                                break;
                            case 8:
                                lbrow.GridLocator = cell.InnerText;
                                break;
                            case 9:
                                // Flag Image
                                break;
                            case 10:
                                lbrow.Country = cell.InnerText;
                                break;
                            case 11:
                                lbrow.OperatorName = cell.InnerText;
                                break;
                            case 12:
                                lbrow.Comments = cell.InnerText;
                                break;
                        }

                    }
                    if (lbrow.position > 0)
                        ret.Add(lbrow);
                }
            }
            return ret;
        }

        #endregion

    }

    public enum WBActions
    {
        noAction = 0,
        start,
        logout,
        loginUID,
        loginPWD,
        openQRZ
    }

    public class OnErrorEventArgs : EventArgs
    {

        private Exception m_ex;

        public Exception ex
        {
            set
            {
                m_ex = value;
            }
            get
            {
                return m_ex;
            }
        }
    }

}
