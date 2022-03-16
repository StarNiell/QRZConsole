using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QRZLibrary
{
    public class Logbook
    {

        WebBrowser wb;
        WBActions wbAction;
        Timer tmrActionState = new Timer();
        int loginStep = 0;

        private string _qrzUrl = "https://www.qrz.com";
        private string _logbookUrl = "https://logbook.qrz.com";
        private string _qrz;
        private string _password;
        private string _loginPage = "/login";

        public string Qrz { get => _qrz; set => _qrz = value; }
        public string Password { get => _password; set => _password = value; }
        public string QrzUrl { get => _qrzUrl; set => _qrzUrl = value; }
        public string LogbookUrl { get => _logbookUrl; set => _logbookUrl = value; }

        public Logbook()
        {
            wb = new WebBrowser();
            wb.ScriptErrorsSuppressed = true;
            wb.DocumentCompleted += Wb_DocumentCompleted;

            tmrActionState.Enabled = false;
            tmrActionState.Interval = 1000;
            tmrActionState.Tick += tmrActionState_Tick;
        }

        public bool Login(string qrz, string password)
        {
            bool ret = false;
            Qrz = qrz;
            
            Password = password;

            wbAction = WBActions.start;
            wb.Navigate(new Uri(QrzUrl + _loginPage));

            return ret;
        }

        private void Wb_DocumentCompleted(Object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ActionStateChanged();
        }

        private void ActionStateChanged()
        {
            Debug.WriteLine($"wbAction: {wbAction.ToString()} - Title {wb.DocumentTitle}");
            switch (wbAction)
            {
                case WBActions.start:
                    wbAction = WBActions.loginUID;
                    setUID();
                    break;
                case WBActions.loginPWD:
                    setPWD();
                    break;
                case WBActions.openLog:
                    if (wb.DocumentTitle == "Callsign Database - QRZ.com")
                    {
                        wb.Stop();
                        OpenLogbook();
                    }
                    break;
                case WBActions.LogbookReady:
                    if (wb.DocumentTitle == "Logbook by QRZ.COM")
                        OpenLogbookComplete();
                    break;
            }
        }

        private void setUID()
        {
            Debug.WriteLine($"setUID()");
            HtmlDocument d = wb.Document;
            HtmlElement uid = d.GetElementById("username");
            uid.SetAttribute("value", Qrz);
            d.InvokeScript("next");
            tmrActionState.Enabled = true;

        }

        private void setPWD()
        {
            Debug.WriteLine($"setPWD()");
            HtmlDocument d = wb.Document;
            HtmlElement pwd = d.GetElementById("password");
            pwd.SetAttribute("value", Password);
            wbAction = WBActions.openLog;
            d.InvokeScript("next");
        }

        private void OpenLogbook()
        {
            Debug.WriteLine($"OpenLogbook()");
            wbAction = WBActions.LogbookReady;
            wb.Navigate(new Uri(LogbookUrl));
        }

        private void OpenLogbookComplete()
        {
            Debug.WriteLine($"OpenLogbookComplete()");
            int i = 0;
        }

        private object GetJSVariableValue(string varname)
        {
            return wb.Document.InvokeScript("eval", new object[] { varname });
        }

        private void tmrActionState_Tick(object seder, EventArgs e)
        {

            int step = (int)GetJSVariableValue("step");
            if (step == 2)
            {
                Debug.WriteLine($"tmrActionState_Tick()");
                tmrActionState.Enabled = false;
                wbAction = WBActions.loginPWD;
                ActionStateChanged();

            }

        }
    }

    public enum WBActions
    {
        start = 0,
        logout,
        loginUID,
        loginPWD,
        openLog,
        LogbookReady

    }
}
