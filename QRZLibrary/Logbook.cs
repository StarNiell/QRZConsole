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

        private bool flagDocumentCompleted = false;

        private string _qrzUrl = "https://www.qrz.com";
        private string _logbookUrl = "https://logbook.qrz.com";
        private string _lookupUrl = "https://www.qrz.com/lookup";
        private string _addCallUrl = "https://logbook.qrz.com/logbook/?op=add;addcall=";
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
        public string AddCallUrl { get => _addCallUrl; set => _addCallUrl = value; }

        public Logbook()
        {
            QRZHelper.initWBEdge11();
            
            wb = new WebBrowser();
            wb.ScriptErrorsSuppressed = true;
            wb.DocumentCompleted += Wb_DocumentCompleted;

        }

        #region Private Methods

        private void Wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            flagDocumentCompleted = true;
            string log = $"Wb_DocumentCompleted - Url={e.Url}";
            Debug.WriteLine(log);
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

        public bool Login(out string errorMessage)
        {
            return Login(Qrz, Password, out errorMessage);
        }

        public bool Login(string qrz, string password, out string errorMessage)
        {
            bool ret = false;
            errorMessage = string.Empty;
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
                        HtmlElement fullname = wb.Document.GetElementById("fullname");
                        if (fullname != null)
                        {
                            stepDone = waitDone = ((fullname.InnerText??string.Empty) != "");
                        }
                        if (!waitDone)
                        {
                            HtmlElement usernameError = wb.Document.GetElementById("usernameError");
                            if (usernameError != null)
                            {
                                if ((!usernameError.Style.Contains("display: none")))
                                {
                                    errorMessage = "Invalid username!";
                                    waitDone = true;
                                }

                            }
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
                            if (wb.Document.Title == "Login - QRZ.com")
                            {

                                HtmlElement alertContainer = QRZHelper.GetElementByTagAndClassName(wb.Document.Body, "div", "alert-container");
                                if (alertContainer != null)
                                {
                                    HtmlElement alertWarning = QRZHelper.GetElementByTagAndClassName(wb.Document.Body, "div", "alert alert-warning");
                                    if (alertWarning.InnerText == "Authentication error: \"Invalid username or password!\"")
                                    {
                                        errorMessage = "Invalid username or password!";
                                        waitDone = true;
                                    }
                                }
                            }
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
            ret = (!ForceReload && wb.Document != null && wb.Document.Title == "Logbook by QRZ.COM") && (wb.Document.GetElementById("button-addon4") != null);
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

        private bool StartAddCall(string QRZToCall)
        {
            bool ret = false;
            if (NavigateAndWait($"{AddCallUrl}{QRZToCall}"))
            {
                ret = wb.Document.Title.Equals("Logbook by QRZ.COM")
                    && (wb.Document.GetElementById("rst_sent") != null);
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

        public string CheckWorkedRaw(string QRZsearch)
        {
            string ret = string.Empty;
            if (StartAddCall(QRZsearch))
            {
                HtmlElement lblist = wb.Document.GetElementById("lblist");
                if (lblist != null )
                {
                    HtmlElement seenBeforeTable = QRZHelper.GetElementByTagAndClassName(lblist, "table", "styledTable seenBeforeTable");
                    if (seenBeforeTable != null)
                    {
                        HtmlElementCollection rows = seenBeforeTable.GetElementsByTagName("TR");

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
            if (GotoLogbook())
            {
                if (wb.Document != null && wb.Document.Title == "Logbook by QRZ.COM" && (wb.Document.GetElementById("button-addon4") != null))
                {
                    HtmlElement el = wb.Document.GetElementById("ipage");
                    if (el != null)
                        if (int.TryParse(el.GetAttribute("value"), out int tmp))
                            ret = tmp;
                }
            }
            return ret;
        }


        public int GotoLoogbookPage(int page)
        {
            int ret = -1;

            if (GotoLogbook())
            {
                HtmlElement el = wb.Document.GetElementById("ipage");
                if (el != null)
                {
                    if (el.GetAttribute("value").ToString() != page.ToString())
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
                    }
                    else
                        ret = page;
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
                            case 13:
                                lbrow.Confirmed = cell.InnerText.StartsWith("Confirmed");
                                break;
                        }
                    }
                    if (lbrow.position > 0)
                        ret.Add(lbrow);
                }
            }
            return ret;
        }

        public int SetLogbookDateOrder(int dateOrder)
        {
            int ret = -1;
            string contains = string.Empty;
            string mustContains = string.Empty;


            if (GotoLogbook())
            {
                HtmlElement th_date = wb.Document.GetElementById("th_date");
                if (th_date != null)
                {
                    if (dateOrder == 0)
                    {
                        contains = "sortAsc";
                        mustContains = "sortDesc";
                    }
                    else if (dateOrder == 1)
                    {
                        contains = "sortDesc";
                        mustContains = "sortAsc";
                    }
                    else
                        return -1;

                    if (th_date.GetAttribute("className").Contains(contains))
                    {
                        bool orderSetOk = false;
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        th_date.InvokeMember("click");

                        while (!orderSetOk)
                        {
                            if (stopwatch.ElapsedMilliseconds > 1000)
                            {
                                th_date = wb.Document.GetElementById("th_date");
                                if (th_date.GetAttribute("className").Contains(mustContains))
                                {
                                    ret = dateOrder;
                                    orderSetOk = true;
                                }

                            }

                            Application.DoEvents();
                            Thread.Sleep(CpuSleep);
                            if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                                return -1;

                        }
                    }
                    else if (th_date.GetAttribute("className").Contains(mustContains))
                    {
                        ret = dateOrder;
                    }
                }
            }

            return ret;
        }

        #endregion

    }

}
