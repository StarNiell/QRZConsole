﻿using QRZLibrary.Classes;
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

        //WebBrowser wb;
        WebBrowser wb;

        WBActions wbAction;
        Timer tmrActionState = new Timer();

        public delegate void LoggedInEventHaldler(object sender, EventArgs e);
        public event LoggedInEventHaldler LoggedIn;
        public delegate void ErrorEventHaldler(object sender, OnErrorEventArgs e);
        public event ErrorEventHaldler Error;

        private string _qrzUrl = "https://www.qrz.com";
        private string _logbookUrl = "https://logbook.qrz.com";
        private string _lookupUrl = "https://www.qrz.com/lookup";
        private string _qrz;
        private string _password;
        private string _loginPage = "/login";
        private long _pageLoadTimeOut = 30000;
        private int _cpuSleep = 50;

        //private int PagesCountLoadAfterLogin = 11;
        //private int PageCounterLoadingAfterLogin = 0;

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
            tmrActionState.Enabled = false;
            tmrActionState.Interval = 1000;
            tmrActionState.Tick += tmrActionState_Tick;

        }

        #region Private Methods

        private void Wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
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


        private bool InvokeMemberAndWait(HtmlElement el, string memberToInvoke)
        {
            return InvokeMemberAndWait(el, memberToInvoke, PageLoadTimeOut);
        }

        private bool InvokeMemberAndWait(HtmlElement el, string memberToInvoke, long timeout)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            el.InvokeMember(memberToInvoke);
            while (wb.ReadyState != WebBrowserReadyState.Complete)
            {
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
                    if (wb.Document.Title == "Callsign Database - QRZ.com")
                    {
                        //PageCounterLoadingAfterLogin++;
                        //if (PageCounterLoadingAfterLogin >= PagesCountLoadAfterLogin)
                        //{
                            wbAction = WBActions.noAction;
                            if (LoggedIn != null)
                                LoggedIn(this, new EventArgs());
                        //}
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
                    HtmlElement elm = QRZHelper.GetElementContainsValue(el, Qrz);
                    ret = (elm != null);

                }
            }

            return ret;
        }

        public void Login()
        {
            Login(Qrz, Password);
        }

        public void Login(string qrz, string password)
        {
            //PageCounterLoadingAfterLogin = 0;
            Qrz = qrz;

            Password = password;

            //wbAction = WBActions.start;
            //Navigate(QrzUrl + _loginPage);


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
                    HtmlElement el1 = QRZHelper.GetElementContainsValue(el, Qrz);
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
                                    ret = InvokeMemberAndWait(link, "Click");
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

        public bool GotoLogbook()
        {
            bool ret = false;
            if (NavigateAndWait(LogbookUrl))
                ret = wb.Document.Title == "Logbook by QRZ.COM";

            return ret;
        }

        public bool GotoLookup()
        {
            bool ret = false;
            if (NavigateAndWait(LookupUrl))
                ret = wb.Document.Title.Equals("QRZ Callsign Database Search by QRZ Ham Radio");

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
                    InvokeMember(sbmt, "Click");
                    while (!resultPageLoaded)
                    {
                        bool test = (wb.Document.GetElementById("qrzcenter") != null);
                        if (test)
                        {
                            resultPageLoaded = (wb.Document.GetElementById("qrzcenter").OuterText.IndexOf($"no results for {QRZsearch.ToUpper()}") > 0);
                        }

                        resultPageLoaded = resultPageLoaded || (wb.Document.GetElementById("csdata") != null) || (wb.Document.GetElementById("csdata") != null);
                                    //|| (wb.Document.GetElementById("tqry") != null);
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
                                entry.Name = csdata.Children[3].Children[0].OuterText.Trim();
                                string[] rawArray = csdata.Children[3].OuterText.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                entry.Address1 = rawArray.Length > 1 ? rawArray[1] : String.Empty;
                                entry.Address2 = rawArray.Length > 2 ? rawArray[2] : String.Empty;
                                entry.Address3 = rawArray.Length > 3 ? rawArray[3] : String.Empty;
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
