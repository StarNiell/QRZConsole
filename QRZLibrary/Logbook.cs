using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using QRZLibrary.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QRZLibrary
{
    public class Logbook
    {

        WebView2 wb2;
        WebBrowser wb;

        private bool flagDocumentCompleted = false;
        private bool wbroDocumentCompleted = false;

        private string _qrzUrl = "https://www.qrz.com/";
        private string _logbookUrl = "https://logbook.qrz.com/";
        private string _lookupUrl = "https://www.qrz.com/db/";
        private string _addCallUrl = "https://logbook.qrz.com/logbook/?op=add;addcall=";
        private string _qrz;
        private string _password;
        //private string _loginPage = "/login";
        private long _pageLoadTimeOut = 30000;
        private int _cpuSleep = 50;
        private string lastCompletePageLoad = string.Empty;
        private bool navigationError = false;
        private List<BandFrequencyRange> bandFrequenciesRange;
        private List<string> modes;
        public string errorMessage = string.Empty;
        public string outMsg = string.Empty;

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
            bandFrequenciesRange = QRZHelper.GetBandFrequencies();
            modes = QRZHelper.GetModes();
        }

        public async Task IninWebView2()
        {
            try
            {
                wb2 = new WebView2();
                string environmentFolderName = Path.Combine(Path.GetTempPath(), $"QRZConsole");
                CoreWebView2Environment cwv2Environment = await CoreWebView2Environment.CreateAsync(null, environmentFolderName, new CoreWebView2EnvironmentOptions());
                await wb2.EnsureCoreWebView2Async(cwv2Environment);

                wb2.NavigationCompleted += Wb2_NavigationtCompleted;
                wb2.NavigationStarting += Wb2_NavigationStarting;
                wb2.SourceChanged += Wb2_SourceChanged;
                wb2.ContentLoading += Wb2_ContentLoading;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
            }
        }

        private void Wb2_ContentLoading(object sender, CoreWebView2ContentLoadingEventArgs e)
        {
            Debug.WriteLine($"  Wb2_ContentLoading > IsErrorPage: {e.IsErrorPage}");
            if (e.IsErrorPage)
            {
                navigationError = true;
            }
        }

        private void Wb2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            Debug.WriteLine($"  Wb2_SourceChanged > IsNewDocument: {e.IsNewDocument}");
        }

        private void Wb2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            Debug.WriteLine($"  Wb2_NavigationStarting {e.Uri}");
        }

        #region Private Methods
        private void Wb2_NavigationtCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            flagDocumentCompleted = true;
            lastCompletePageLoad = wb2.Source.ToString();
            Debug.WriteLine($"Wb2_NavigationtCompleted - Url={lastCompletePageLoad}");
        }

        private async Task<bool> NavigateAndWait(string Url)
        {
            Debug.WriteLine($">>>>> NavigateAndWait: {Url} start...");
            await NavigateAndWait(Url, PageLoadTimeOut);
            Debug.WriteLine($"***** NavigateAndWait: {Url} COMPLETE *******");
            return lastCompletePageLoad == Url;
        }

        private async Task<bool> NavigateAndWait(string Url, long timeout)
        {
            lastCompletePageLoad = string.Empty;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            navigationError = false;
            wb2.CoreWebView2.Navigate(Url);
            while (lastCompletePageLoad != Url)
            {
                await Task.Delay(CpuSleep);
                if (stopwatch.ElapsedMilliseconds >= timeout)
                    return false;
                if (navigationError)
                    return false;
            }

            return true;
        }

        private async Task<string> GetElementValueAsync(string id, string attribute = "value")
        {
            string s = await wb2.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{id}').getAttribute('{attribute}')");
            return s.Replace("\"", "");
        }

        private async Task<string> GetValueElementAsync(string id)
        {
            string s = await wb2.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{id}').value");
            return s.Replace("\"", "");
        }

        private async Task SetElementValueAsync(string id, string value)
        {
            await wb2.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{id}').value = '{value}'");
        }

        //private async Task<object> GetJSValue(string jsCode)
        //{
        //    return await wb2.CoreWebView2.ExecuteScriptAsync($"eval('{jsCode}')");
        //}

        private async Task SetElementValue(string id, string value)
        {
            await wb2.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{id}').value='{value}'");
        }

        //private void SetValueByParentElementAndTagnameAndClassName(string parentId, string tagname, string classname, string value)
        //{
        //    wb2.CoreWebView2.ExecuteScriptAsync($"document.querySelector('#{parentId} {tagname}.{classname}').value='{value}'");
        //}

        private async Task<string> ExecuteScriptAsync(string script)
        {
            return await wb2.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task<bool> IsLogged()
        {
            bool ret = false;
            bool pageLoaded = false;

            if (wb2.CoreWebView2.Source != null && wb2.CoreWebView2.Source != "about:blank")
            {
                pageLoaded = (wb2.Source.OriginalString.Contains(_qrzUrl))
                          || (wb2.Source.OriginalString.Contains(_logbookUrl))
                          || (wb2.Source.OriginalString.Contains(_lookupUrl));
                //pageLoaded = false;
            }

            if (!pageLoaded)
                pageLoaded = await GotoQrzHome();

            if (pageLoaded)
            {
                string s = await GetScriptResult("document.querySelector('nav ul.primary-navigation').textContent.includes('Logout');");
                ret = (s == "true");
            }
            return ret;
        }

        public async Task<bool> Login()
        {
            return await Login(Qrz, Password);
        }

        public async Task<bool> Login(string Qrz, string Password)
        {
            bool ret = false;
            errorMessage = string.Empty;
            bool stepDone = false;
            bool waitDone = false;
            Stopwatch stopwatch;

            lastCompletePageLoad = "";
            if (await NavigateAndWait("https://www.qrz.com/login"))
            {
                if (wb2.CoreWebView2.DocumentTitle == "Login - QRZ.com")
                {
                    // Set the username ***************************************************************
                    await SetElementValueAsync("username", Qrz);

                    await ExecuteScriptAsync("next();");

                    //wait for new step -----------
                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    string fullname;

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

                        await Task.Delay(CpuSleep);
                        if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                        {
                            break;
                        }
                    }

                    if (!stepDone)
                        return false;

                    // Set the password ***************************************************************
                    await SetElementValueAsync("password", Password);
                    await ExecuteScriptAsync("next();");

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
                                bool alertContainer = await ExecuteScriptAsync("document.querySelector('body > div.container-fluid.main-content.theme-showcase > div > div.alert-container > div.alert.alert-warning').innerText.includes('Invalid username or password')") == "true";
                                if (alertContainer)
                                {
                                    errorMessage = "Invalid username or password!";
                                    waitDone = true;
                                }
                            }
                        }

                        await Task.Delay(CpuSleep);
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

        public async Task<bool> LogOut()
        {
            bool ret = false;

            if (await IsLogged())
            {
                if (await ExecuteScriptAsync("document.querySelector('ul.primary-navigation').textContent.includes('Logout');") == "true")
                {
                    if (await ExecuteScriptAsync("document.querySelector('ul.primary-navigation').textContent.includes('Edit');") == "true")
                    {
                        if (wb2.CoreWebView2.Source.Contains("op=add;addcall"))
                        {
                            await wb2.CoreWebView2.ExecuteScriptAsync("document.querySelector('#qrztop > nav > ul.primary-navigation > li.leaf.last > ul > li:nth-child(7) > a').click();");
                        }
                        else
                        {
                            await wb2.CoreWebView2.ExecuteScriptAsync("document.querySelector('body > div.qrztop > nav > ul.primary-navigation > li:nth-child(10) > ul > li:nth-child(7) > a').click();");
                        }
                    }
                    else
                        await wb2.CoreWebView2.ExecuteScriptAsync("document.querySelector('body > div.qrztop > nav > ul.primary-navigation > li:nth-child(10) > ul > li:nth-child(6) > a').click();");
                    ret = true;
                }
            }
            return ret;
        }

        public async Task<bool> GotoQrzHome()
        {
            return await NavigateAndWait(QrzUrl);
        }

        public async Task<bool> GotoLogbook(bool ForceReload = false)
        {
            bool ret = (!ForceReload && wb2.CoreWebView2.DocumentTitle == "Logbook by QRZ.COM");
            ret = ret && await GetScriptResult("document.getElementById('button-addon4') != null && document.getElementById('addcall') != null;") == "true";
            if (!ret)
            {
                if (await NavigateAndWait(LogbookUrl))
                    ret = wb2.CoreWebView2.DocumentTitle == "Logbook by QRZ.COM";
            }

            return ret;
        }

        public async Task<bool> GotoLookup(bool ForceReload = false)
        {
            bool ret = (!ForceReload && await ExecuteScriptAsync("document.getElementById('tquery') != null") == "true");
            if (!ret)
            {
                lastCompletePageLoad = "";
                if (await NavigateAndWait(LookupUrl))
                    ret = (wb2.CoreWebView2.DocumentTitle == "QRZ Callsign Database Search by QRZ Ham Radio" || wb2.CoreWebView2.DocumentTitle == "The Home for Amateur Radio by QRZ Ham Radio");
            }
            return ret;
        }

        private async Task<bool> StartAddCall(string QRZToCall)
        {
            bool ret = false;
            if (await NavigateAndWait($"{AddCallUrl}{QRZToCall}"))
            {
                ret = wb2.CoreWebView2.DocumentTitle.Equals("Logbook by QRZ.COM")
                    && (await ExecuteScriptAsync("document.getElementById('rst_sent') != null") == "true");
            }
            return ret;
        }

        // Add QSO into Logbook
        public async Task<bool> AddQSOToLogbook(string qrzToCall, string freq, string mode, string date, string time, string comment)
        {
            bool ret = false;

            if (await StartAddCall(qrzToCall))
            {
                if (await SetQSOData(qrzToCall, freq, mode, date, time, comment))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    bool PageLoaded = false;

                    if (await ExecuteScriptAsync("document.getElementById('savebut') != null") == "true")
                    {
                        await ExecuteScriptAsync("document.getElementById('savebut').click()");
                    }

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
                        }

                        await Task.Delay(CpuSleep);
                        if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                            break;
                    }

                    ret = PageLoaded;
                }
            }

            return ret;
        }

        public async Task<bool> EditQSOToLogbook(int position, string qrzToCall, string freq, string mode, string date, string time, string comment)
        {
            bool ret = false;

            if (await StartEditQSO(position))
            {
                if (await SetQSOData(qrzToCall, freq, mode, date, time, comment))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    bool PageLoaded = false;
                    //flagDocumentCompleted = false;
                    if (await ExecuteScriptAsync("document.getElementById('savebut') != null") == "true")
                    {
                        await ExecuteScriptAsync("document.getElementById('savebut').click()");
                    }

                    while (!PageLoaded)
                    {
                        if (stopwatch.ElapsedMilliseconds > 1000)
                        {
                            //if (flagDocumentCompleted)
                            //{
                                if ((wb2.CoreWebView2.DocumentTitle == "Logbook by QRZ.COM"))
                                {
                                    if (await ExecuteScriptAsync("document.getElementById('lb_body') != null") == "true")
                                    {
                                        PageLoaded = (await ExecuteScriptAsync("document.getElementById('lb_body').innerText.includes('QSO Detail')") == "true");
                                    }
                                }

                            //}
                        }

                        await Task.Delay(CpuSleep);
                        if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                            break;
                    }

                    ret = PageLoaded;
                }
            }

            return ret;
        }

        public async Task<bool> DeleteQSOFromLogbook(int position)
        {
            bool ret = false;

            if (await OpenQSORecord(position))
            {
                await ExecuteScriptAsync("document.getElementById('lbmenu').getElementsByTagName('input').namedItem('op').value = 'logdel';");

                bool PageLoaded = false;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                flagDocumentCompleted = false;
                await ExecuteScriptAsync("document.getElementById('lbmenu').submit()");
                while (!PageLoaded)
                {
                    if (flagDocumentCompleted)
                    {
                        if ((wb2.CoreWebView2.DocumentTitle == "Logbook by QRZ.COM"))
                        {
                            PageLoaded = (await ExecuteScriptAsync("document.getElementById('addcall') != null") == "true");
                        }
                    }

                    await Task.Delay(CpuSleep);
                    if (stopwatch.ElapsedMilliseconds >= PageLoadTimeOut)
                    {
                        break;
                    }
                }

                ret = PageLoaded;
            }

            return ret;
        }


        // Edit QSO
        public async Task<bool> StartEditQSO(int position)
        {
            bool ret = false;

            if (await OpenQSORecord(position))
            {
                bool PageLoaded = false;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                flagDocumentCompleted = false;
                await ExecuteScriptAsync("lb_go('edit','')");

                while (!PageLoaded)
                {
                    if (flagDocumentCompleted)
                    {
                        if ((wb2.CoreWebView2.DocumentTitle == "Logbook by QRZ.COM"))
                        {
                            PageLoaded = await ExecuteScriptAsync("document.getElementById('start_date') != null") == "true";
                        }
                    }

                    await Task.Delay(CpuSleep);
                    if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                    {
                        break;
                    }
                }

                ret = PageLoaded;
            }

            return ret;
        }


        public async Task<bool> OpenQSORecord(int position) 
        {
            bool ret = false;
            int qsoForPage = await GetEntriesForPage();
            if (qsoForPage > 0)
            {
                float fQsoForPage = (float)qsoForPage;
                float fPosition = (float)position;
                bool HaveRestStart = ((fPosition % fQsoForPage) != 0);
                int page = (position / qsoForPage) + (HaveRestStart ? 1 : 0);

                if (await GotoLoogbookPage(page) == page)
                {
                    HtmlElement table = await GetHTMLElementWait("lblistrs", "lbtab");
                    HtmlElement thead = QRZHelper.GetElementByTagAndClassName(table, "thead", "");
                    if (thead.TagName != null)
                    {
                        HtmlElementCollection colnames = thead.GetElementsByTagName("TH");
                        if (colnames != null)
                        {
                            HtmlElement tbody = QRZHelper.GetElementByTagAndClassName(table, "tbody", "");
                            if (tbody != null)
                            {
                                HtmlElementCollection rows = tbody.GetElementsByTagName("TR");
                                foreach (HtmlElement row in rows)
                                {

                                    int curPos = QRZHelper.GetIntByStringOrNegative(row.GetAttribute("data-pos"));
                                    if (curPos >= 0)
                                        curPos++;

                                    if (curPos == position)
                                    {
                                        string pos = row.GetAttribute("data-pos");
                                        string num = row.GetAttribute("data-rownum");
                                        string bid = row.GetAttribute("data-bookid");

                                        /*
                                        HtmlElement lbmenu = wb.Document.GetElementById("lbmenu");
                                        lbmenu.GetElementsByTagName("input").GetElementsByName("bookid")[0].SetAttribute("value", bid);
                                        lbmenu.GetElementsByTagName("input").GetElementsByName("logpos")[0].SetAttribute("value", pos);
                                        */
                                        await ExecuteScriptAsync($"document.getElementById('lbmenu').getElementsByTagName('input').namedItem('bookid').value = '{bid}';");
                                        await ExecuteScriptAsync($"document.getElementById('lbmenu').getElementsByTagName('input').namedItem('logpos').value = '{pos}';");

                                        bool PageLoaded = false;

                                        Stopwatch stopwatch = new Stopwatch();
                                        stopwatch.Start();
                                        flagDocumentCompleted = false;
                                        
                                        await ExecuteScriptAsync("document.getElementById('lbmenu').submit()");

                                        while (!PageLoaded)
                                        {
                                            if (flagDocumentCompleted)
                                            {
                                                if ((wb2.CoreWebView2.DocumentTitle == "Logbook by QRZ.COM"))
                                                {
                                                    PageLoaded = (await ExecuteScriptAsync("document.getElementById('lb_body') != null && document.getElementById('lb_body').innerText.includes('QSO Detail');") == "true");
                                                }

                                            }

                                            await Task.Delay(CpuSleep);
                                            if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                                            {
                                                break;
                                            }
                                        }

                                        ret = PageLoaded;
                                    }
                                    if (curPos >= position)
                                        break;
                                }

                            }
                        }
                    }
                    
                }
            }
            return ret;
        }

        // Insert Data il QSO ADD or EDIT fields
        private async Task<bool> SetQSOData(string qrzToCall, string freq, string mode, string date, string time, string comment)
        {
            bool ret = false;
            bool bOK = false;
            string bandFound = string.Empty;

            // Check the Date Format
            bOK = (DateTime.TryParse(date, out DateTime dummyDate));

            // Set the Date in to input data
            if (bOK)
            {
                bOK = (await ExecuteScriptAsync("document.getElementById('start_date') != null") == "true")
                    &&
                    (await ExecuteScriptAsync("document.getElementById('end_date') != null") == "true");


                if (bOK)
                {
                    await SetElementValue("start_date", date);
                    await SetElementValue("end_date", date);
                    bOK = true;
                }

                // Check the Time Format
                if (bOK)
                    bOK = TimeSpan.TryParse(time, out TimeSpan dummyTime);

                // Set the Time in to input data
                if (bOK)
                {
                    bOK = (await ExecuteScriptAsync("document.getElementById('start_time') != null") == "true")
                    &&
                    (await ExecuteScriptAsync("document.getElementById('end_time') != null") == "true");

                    if (bOK)
                    {
                        await SetElementValue("start_time", time);
                        await SetElementValue("end_time", time);
                        bOK = true;
                    }
                }

                // Check The Freq
                if (bOK)
                {
                    bOK = false;
                    if (double.TryParse(freq.Replace(".", ","), out double dFreq))
                    {
                        bandFound = bandFrequenciesRange.Where(x => dFreq >= x.Min && dFreq <= x.Max).FirstOrDefault()?.Band;
                    }
                    bOK = (!string.IsNullOrEmpty(bandFound));
                }

                //Set the Freq
                if (bOK)
                {
                    bOK = await ExecuteScriptAsync("document.getElementById('freq2') != null") == "true";
                    if (bOK)
                    {
                        await SetElementValue("freq2", freq);
                        await ExecuteScriptAsync($"setFreq('2, '0')");

                        bOK = false;
                        if (await ExecuteScriptAsync("document.getElementById('band2') != null") == "true")
                        {
                            await SetElementValue("band2", bandFound);
                            bOK = true;
                        }

                        if (bOK)
                        {
                            await SetElementValue("freq1", freq);
                            await ExecuteScriptAsync($"setFreq('1, '0')");

                            bOK = false;
                            if (await ExecuteScriptAsync("document.getElementById('band1') != null") == "true")
                            {
                                await SetElementValue("band1", bandFound);
                                bOK = true;
                            }
                        }
                    }
                }

                //Chek The Mode
                if (bOK)
                {
                    bOK = false;
                    if (!string.IsNullOrEmpty(mode))
                    {
                        bOK = modes.Where(x => x == mode).Any();
                    }
                }

                //Set the Mode
                if (bOK)
                {
                    if (await ExecuteScriptAsync("document.getElementById('mode2') != null") == "true")
                    {
                        await SetElementValue("mode2", mode);
                        await ExecuteScriptAsync($"setMode('1, '2')");

                        bOK = true;
                    }

                    if (bOK)
                    {
                        if (await ExecuteScriptAsync("document.getElementById('mode1') != null") == "true")
                        {
                            await SetElementValue("mode1", mode);
                            await ExecuteScriptAsync($"setMode('1, '1')");

                            bOK = true;
                        }

                    }

                }

                // Set RTS SENT
                if (bOK)
                {
                    bOK = await ExecuteScriptAsync("document.getElementById('rst_sent') != null") == "true";
                    if (bOK)
                    {
                        await SetElementValue("rst_sent", "59");
                        bOK = true;
                    }
                }

                // Set RTS RCVD
                if (bOK)
                {
                    bOK = await ExecuteScriptAsync("document.getElementById('rst_rcvd') != null") == "true";
                    if (bOK)
                    {
                        await SetElementValue("rst_rcvd", "59");
                        bOK = true;
                    }
                }

                //Set the comment
                if (bOK)
                {
                    if (!string.IsNullOrEmpty(comment))
                    {
                        // TODO: La parte dei commenti non è visibile
                        bOK = await ExecuteScriptAsync("document.getElementById('comments1') != null") == "true";
                        if (bOK)
                        {
                            await SetElementValue("comments1", "comment");
                            bOK = true;
                        }
                        bOK = true; // TODO: La parte dei commenti non è visibile
                    }
                }
            }
            if (bOK)
                return true;

            return ret;
        }

        public async Task<LookupEntry> ExecQuery(string QRZsearch)
        {
            LookupEntry ret = new LookupEntry();
            bool queryTimeOut = false;
            long timeout = PageLoadTimeOut;
            bool resultPageLoaded = false;
            bool resultFound;

            if (await GotoLookup())
            {
                if (wb2.CoreWebView2.DocumentTitle != "The Home for Amateur Radio by QRZ Ham Radio")
                {
                    Debug.Print($"before start lookup you are in: {wb2.CoreWebView2.Source}");

                    await ExecuteScriptAsync($"document.querySelector('#topcall > div.magic > div.tqueryWrapper > input.tquery').value = '{QRZsearch.ToUpper()}'");

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    //flagDocumentCompleted = false;
                    await ExecuteScriptAsync("document.getElementById('tsubmit').click()");
                    while (!resultPageLoaded)
                    {
                        if (true)
                        {
                            bool f = await ExecuteScriptAsync("document.getElementById('qrzcenter') != null") == "true";
                            if (f)
                            {
                                resultPageLoaded = await ExecuteScriptAsync($"document.getElementById('csdata')") != "null";
                                if (!resultPageLoaded)
                                {
                                    resultPageLoaded = await ExecuteScriptAsync($"document.getElementById('qrzcenter').outerText.includes('no results for {QRZsearch.ToUpper()}') > 0") == "true";
                                }
                            }
                        }

                        await Task.Delay(CpuSleep);
                        if (stopwatch.ElapsedMilliseconds >= timeout)
                        {
                            queryTimeOut = true;
                            break;
                        }
                    }


                    Debug.Print($"after lookup you are in: {wb2.CoreWebView2.Source}");
                    if (!queryTimeOut)
                    {
                        LookupEntry entry = new LookupEntry();
                        resultFound = await ExecuteScriptAsync("document.getElementById('csdata') != null") == "true";
                        if (resultFound)
                        {
                            await ExecuteScriptAsync("showqem()");
                            HtmlElement csdata = await GetHTMLElementWait("calldata", "csdata");
                            if (csdata != null)
                            {
                                entry.QRZ = csdata.Children[0].OuterText.Trim();
                                entry.DXCC = csdata.Children[1].OuterHtml.Substring(csdata.Children[1].OuterHtml.IndexOf("?dxcc=") + 6, 3);
                                entry.DXCC = Regex.Match(entry.DXCC, @"\d+").Value;
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

                                HtmlElement detbox = await GetHTMLElementWait("t_detail", "dt");
                                if (detbox != null)
                                {
                                    HtmlElementCollection rows = detbox.GetElementsByTagName("TR");

                                    foreach (HtmlElement row in rows)
                                    {
                                        HtmlElementCollection cols = row.GetElementsByTagName("TD");
                                        foreach (HtmlElement cell in cols)
                                        {
                                            switch (cell.InnerText)
                                            {
                                                case "Grid Square":
                                                    entry.GridSquare = cell.NextSibling.InnerText;
                                                    break;
                                                case "Lookups":
                                                    entry.Lookups = cell.NextSibling.InnerText;
                                                    break;
                                                case "Distance":
                                                    entry.Distance = cell.NextSibling.InnerText;
                                                    break;
                                                case "US State":
                                                    entry.UsState = cell.NextSibling.InnerText;
                                                    break;
                                                case "US County":
                                                    entry.UsCounty = cell.NextSibling.InnerText;
                                                    break;
                                            }
                                        }

                                    }
                                }
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
                else
                {
                    LookupEntry entry = new LookupEntry
                    {
                        QRZ = QRZsearch,
                        Name = "Too many lookups"
                    };
                    ret = entry;
                }
            }
            return ret;
        }

        public async Task<string> CheckWorkedRaw(string QRZsearch)
        {
            string ret = string.Empty;
            if (await StartAddCall(QRZsearch))
            {
                HtmlElement lblist = await GetHTMLElementWait("lbform", "lblist");
                if (lblist != null)
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

        public async Task<int> GetEntriesForPage()
        {
            int ret = -1;
            if (await GotoLogbook())
            {
                string s = await GetValueElementAsync("dispOpt_rpp");
                    ret = QRZHelper.GetIntByString(s);
            }
            return ret;
        }

        public async Task<int> SetEntriesForPage(int entries) //, out string msg)
        {
            int ret = -1;
            string defaultMsg = "Invalid parameter! Enter one of the following values: 5, 10, 15, 20, 25, 50, 100, 200";
            outMsg = string.Empty;
            if (entries != 5
                && entries != 10
                && entries != 15
                && entries != 20
                && entries != 25
                && entries != 50
                && entries != 100
                && entries != 200
                )
                outMsg = defaultMsg;
            else
            {
                if (await GotoLogbook())
                {
                    int currValue;

                    string rowsForPage = await GetValueElementAsync("dispOpt_rpp");
                    if (!string.IsNullOrEmpty(rowsForPage))
                    {
                        currValue = int.Parse(rowsForPage);
                        if (currValue > 0 && currValue != entries)
                        {
                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();

                            bool PageLoaded = false;

                            await SetElementValue("dispOpt_rpp", entries.ToString());
                            await ExecuteScriptAsync("updateDisplayOptions('rpp')");

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
                                    await Task.Delay(CpuSleep);
                                    if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                                        break;
                                }
                            }

                            if (PageLoaded)
                            {
                                if (await GotoLogbook(true))
                                    ret = await GetEntriesForPage();
                            }
                        }
                        else if (currValue > 0 && currValue == entries)
                        {
                            ret = entries;
                        }
                    }
                }
            }
            return ret;
        }

        public async Task<int> GetQSOsCount(bool ForceReload = false)
        {
            int ret = -1;

            if (await GotoLogbook(ForceReload))
            {
                HtmlElement el = await GetHTMLElementWait("lb_body", "lbmenu");
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

        public async Task<List<KeyValuePair<string, bool>>> GetLocatorWorked() //TODO: da completare
        {
            List<KeyValuePair<string, bool>> ret = new List<KeyValuePair<string, bool>>();
            /*
            if (await GotoLogbook())
            {
                bool stepDone = false;
                bool waitDone = false;
                Stopwatch stopwatch;

                Debug.WriteLine("Load Award section...");

                await ExecuteScriptAsync("lb_go('awards', '')");

                //wait for new step -----------
                stopwatch = new Stopwatch();
                stopwatch.Start();

                while (!waitDone)
                {
                    stepDone = waitDone = (await ExecuteScriptAsync("document.getElementById('container') != null") == "true");

                    await Task.Delay(CpuSleep);

                        if (stopwatch.ElapsedMilliseconds >= PageLoadTimeOut)
                        {
                            break;
                        }
                }

                if (!stepDone)
                {
                    Debug.WriteLine("Unable load Award section...");
                    return ret;
                }

                stepDone = false;
                waitDone = false;
                stopwatch = null;

                Debug.WriteLine("Expand Award section...");
                //HtmlElementCollection elH3s = wb.Document.GetElementsByTagName("h3");

                //foreach (HtmlElement elH3 in elH3s)
                //{
                //    if (elH3.InnerText.Contains("Grid Squared Award"))
                //    {
                //        elH3.InvokeMember("click");
                //    }
                //}

                ////wait for new step -----------
                //stopwatch = new Stopwatch();
                //stopwatch.Start();

                //while (!waitDone)
                //{
                //    HtmlElement container = wb.Document.GetElementById("lbtab");
                //    stepDone = waitDone = (container != null);

                //    Application.DoEvents();
                //    Thread.Sleep(CpuSleep);
                //    if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                //    {
                //        break;
                //    }
                //}

                //if (!stepDone)
                //{
                //    Debug.WriteLine("Unable expand Award section.");
                //    return ret;
                //}

                //Debug.WriteLine("Get W100 Award table...");

                //HtmlElement el1 = wb.Document.GetElementById("content-inside-4");
                //if (el1 != null)
                //{
                //    HtmlElement tblW100 = QRZHelper.GetElementByTagAndClassName(el1, "table", "w100");
                //    if (tblW100 != null)
                //    {
                //        HtmlElementCollection els = tblW100.GetElementsByTagName("tr");

                //        foreach (HtmlElement el in els)
                //        {
                //            if (!string.IsNullOrEmpty(el.Id))
                //            {
                //                if (el.Id.Contains("mainRow"))
                //                {
                //                    string locator = QRZHelper.GetElementByTagAndClassName(el, "span", "ptr lent").InnerText;
                //                    bool confirmed = (QRZHelper.GetElementByTagAndClassName(el, "td", "lstat").InnerText == "Confirmed");
                //                    ret.Add(new KeyValuePair<string, bool>(locator, confirmed));
                //                }
                //            }
                //        }
                //    }
                //    else
                //    {
                //        Debug.WriteLine("Unable to locate W100 Award table.");
                //        return ret;
                //    }
                //}

            }
            */
            return ret;
        }

        public async Task<int> GetLogbookPages(bool ForceReload = false)
        {
            int ret = -1;

            if (await GotoLogbook(ForceReload))
            {
                HtmlElement el = await GetHTMLElementWait("lbmenu", "listnav");
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
                            if (int.TryParse(await GetElementValueAsync("ipage"), out int tmp))
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
                                await Task.Delay(CpuSleep);
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

        private void Wbro_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            wbroDocumentCompleted = true;
        }


        public async Task<string> GetLogbookPageContentRaw(int page)
        {
            string ret = string.Empty;

            if (await GotoLoogbookPage(page) == page)
            {
                HtmlElement table = await GetHTMLElementWait("lblistrs", "lbtab");
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

        private async Task<string> GetHTMLTextElement(string id)
        {
            string s = await wb2.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{id}').innerHTML");
            return s;
        }

        private async Task<HtmlElement> GetHTMLElementWait(string parent, string id)
        {
            HtmlElement htmlElement = null;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (htmlElement == null)
            {
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    htmlElement = await GetHTMLElementAsync(parent, id);
                }

                await Task.Delay(CpuSleep);
                if (stopwatch.ElapsedMilliseconds >= _pageLoadTimeOut)
                    break;
            }
            return await GetHTMLElementAsync(parent, id);
        }

        private async Task<HtmlElement> GetHTMLElementAsync(string parent, string id)
        {
            bool bFound = true;

            wb = new WebBrowser();
            wbroDocumentCompleted = false;
            wb.ScriptErrorsSuppressed = true;
            wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(Wbro_DocumentCompleted);
            string g = await GetHTMLTextElement(parent);
            g = Regex.Unescape(g);
            g = g.Remove(0, 1);
            g = g.Remove(g.Length - 1, 1);

            wb.DocumentText = g;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (!wbroDocumentCompleted)
            {
                await Task.Delay(CpuSleep);
                if (stopwatch.ElapsedMilliseconds >= 1000)
                {
                    bFound = false;
                    break;
                }
            }

            wb.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(Wbro_DocumentCompleted);
            HtmlElement ret = null;
            if (bFound)
            {
                ret = wb.Document.GetElementById(id);
            }

            wb = null;

            return ret;
        }

        public async Task<List<LogbookEntry>> GetLogbookPageContent(int page)
        {

            List<LogbookEntry> ret = new List<LogbookEntry>();
            if (await GotoLoogbookPage(page) == page)
            {
                HtmlElement table = await GetHTMLElementWait("lblistrs", "lbtab");
                HtmlElement thead = QRZHelper.GetElementByTagAndClassName(table, "thead", "");
                if (thead.TagName != null)
                {
                    HtmlElementCollection colnames = thead.GetElementsByTagName("TH");
                    if (colnames != null)
                    {
                        HtmlElement tbody = QRZHelper.GetElementByTagAndClassName(table, "tbody", "");
                        if (tbody != null)
                        {
                            HtmlElementCollection rows = tbody.GetElementsByTagName("TR");
                            foreach (HtmlElement row in rows)
                            {
                                LogbookEntry lbrow = new LogbookEntry();
                                HtmlElementCollection cols = row.GetElementsByTagName("TD");
                                lbrow.LoTWSent = row.GetAttribute("className").Contains("lotw_sent");
                                int currCol = 0;
                                string tmp;
                                string DateTimeStr = string.Empty;
                                foreach (HtmlElement cell in cols)
                                {
                                    string cellText = cell.InnerText ?? String.Empty;

                                    switch (colnames[currCol].Id)
                                    {
                                        case "th_lnum":
                                            lbrow.position = QRZHelper.GetIntByString(cellText);
                                            break;
                                        case "th_date":
                                            DateTimeStr = cellText;
                                            break;
                                        case "th_time":
                                            DateTimeStr += $" {cellText}";
                                            lbrow.QSODateTime = QRZHelper.GetDateTimeByString(DateTimeStr);
                                            break;
                                        case "th_call2":
                                            lbrow.Call = cellText.Replace("Ø", "0");
                                            break;
                                        case "th_band1":
                                            lbrow.Band = cellText;
                                            break;
                                        case "th_freq2":
                                            lbrow.Frequency = cellText;
                                            break;
                                        case "th_mode2":
                                            lbrow.Mode = cellText;
                                            break;
                                        case "th_grid2":
                                            lbrow.GridLocator = cellText;
                                            break;
                                        case "th_country2":
                                            lbrow.Country = cellText;
                                            break;
                                        case "th_name2":
                                            lbrow.OperatorName = cellText;
                                            break;
                                        case "th_comments":
                                            lbrow.Comments = cellText;
                                            break;
                                        case "th_status":
                                            lbrow.Confirmed = cellText.StartsWith("Confirmed");
                                            break;
                                    }
                                    currCol++;
                                }
                                if (lbrow.position > 0)
                                {
                                    lbrow.DXCC = string.Empty;
                                    if (!string.IsNullOrEmpty(lbrow.Country))
                                        lbrow.DXCC = QRZHelper.GetDXCCByCountry(lbrow.Country);
                                    ret.Add(lbrow);
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }


        public async Task<List<LogbookEntry>> GetLogbookEntriesByRange(int start, int end)
        {
            List<LogbookEntry> ret = new List<LogbookEntry>();
            if (start <= end)
            {
                int qsoForPage = await GetEntriesForPage();
                if (qsoForPage > 0)
                {
                    float fStart = (float)start;
                    float fEnd = (float)end;
                    float fQsoForPage = (float)qsoForPage;

                    bool HaveRestStart = ((fStart % fQsoForPage) != 0);
                    bool HaveRestEnd = ((fEnd % fQsoForPage) != 0);

                    int pageStart = (start / qsoForPage) + (HaveRestStart ? 1 : 0);
                    int pageEnd = (end / qsoForPage) + (HaveRestEnd ? 1 : 0);
                    if (pageStart > 0 && pageEnd > 0 && pageStart <= pageEnd)
                    {
                        for (int i = pageStart; i <= pageEnd; i++)
                        {
                            if (await GotoLoogbookPage(i) == i)
                            {
                                HtmlElement table = await GetHTMLElementWait("lblistrs", "lbtab");
                                HtmlElement thead = QRZHelper.GetElementByTagAndClassName(table, "thead", "");
                                if (thead.TagName != null)
                                {
                                    HtmlElementCollection colnames = thead.GetElementsByTagName("TH");
                                    if (colnames != null)
                                    {
                                        HtmlElement tbody = QRZHelper.GetElementByTagAndClassName(table, "tbody", "");
                                        if (tbody != null)
                                        {
                                            HtmlElementCollection rows = tbody.GetElementsByTagName("TR");
                                            foreach (HtmlElement row in rows)
                                            {

                                                int curPos = QRZHelper.GetIntByStringOrNegative(row.GetAttribute("data-pos"));
                                                if (curPos >= 0)
                                                    curPos++;

                                                if (start <= curPos && curPos <= end)
                                                {

                                                    LogbookEntry lbrow = new LogbookEntry();
                                                    HtmlElementCollection cols = row.GetElementsByTagName("TD");
                                                    int currCol = 0;
                                                    //int curPos = -1;
                                                    string tmp = string.Empty;
                                                    string DateTimeStr = string.Empty;
                                                    lbrow.LoTWSent = row.GetAttribute("className").Contains("lotw_sent");
                                                    foreach (HtmlElement cell in cols)
                                                    {
                                                        string cellText = cell.InnerText ?? String.Empty;

                                                        switch (colnames[currCol].Id)
                                                        {
                                                            case "th_lnum":
                                                                //curPos = QRZHelper.GetIntByString(cell.InnerText);
                                                                //if (!(start <= curPos && curPos <= end))
                                                                //    break;
                                                                lbrow.position = curPos;
                                                                break;
                                                            case "th_date":
                                                                DateTimeStr = cellText;
                                                                break;
                                                            case "th_time":
                                                                DateTimeStr += $" {cellText}";
                                                                lbrow.QSODateTime = QRZHelper.GetDateTimeByString(DateTimeStr);
                                                                break;
                                                            case "th_call2":
                                                                lbrow.Call = cellText.Replace("Ø", "0");
                                                                break;
                                                            case "th_band1":
                                                                lbrow.Band = cellText;
                                                                break;
                                                            case "th_freq2":
                                                                lbrow.Frequency = cellText;
                                                                break;
                                                            case "th_mode2":
                                                                lbrow.Mode = cellText;
                                                                break;
                                                            case "th_grid2":
                                                                lbrow.GridLocator = cellText;
                                                                break;
                                                            case "th_country2":
                                                                lbrow.Country = cellText;
                                                                break;
                                                            case "th_name2":
                                                                lbrow.OperatorName = cellText;
                                                                break;
                                                            case "th_comments":
                                                                lbrow.Comments = cellText;
                                                                break;
                                                            case "th_status":
                                                                lbrow.Confirmed = cellText.StartsWith("Confirmed");
                                                                break;
                                                        }
                                                        if (curPos > -1 && (!(start <= curPos && curPos <= end)))
                                                            break;
                                                        currCol++;
                                                    }
                                                    if (lbrow.position > 0)
                                                    {
                                                        lbrow.DXCC = string.Empty;
                                                        if (!string.IsNullOrEmpty(lbrow.Country))
                                                            lbrow.DXCC = QRZHelper.GetDXCCByCountry(lbrow.Country);
                                                        ret.Add(lbrow);
                                                    }
                                                }
                                                if (curPos > end)
                                                    break;
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        public async Task<int> SetLogbookDateOrder(int dateOrder)
        {
            int ret = -1;
            string contains;
            string mustContains;


            if (await GotoLogbook())
            {
                HtmlElement th_date = await GetHTMLElementWait("lblistrs", "th_date");
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

                    if (await ExecuteScriptAsync($"document.getElementById('th_date').className.includes('{contains}');") == "true")
                    {
                        bool orderSetOk = false;
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        await ExecuteScriptAsync("document.getElementById('th_date').click()");

                        while (!orderSetOk)
                        {
                            if (stopwatch.ElapsedMilliseconds > 1000)
                            {
                                if (await ExecuteScriptAsync($"document.getElementById('th_date').className.includes('{mustContains}');") == "true")
                                {
                                    ret = dateOrder;
                                    orderSetOk = true;
                                }

                            }

                            await Task.Delay(CpuSleep);
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

        private async Task<string> GetScriptResult(string script)
        {
            string t = await wb2.CoreWebView2.ExecuteScriptAsync(script);
            return t.Replace("\"", "");
        }

        //private async Task<string> EvalJSValueById(string id)
        //{
        //    return await EvalJS($"document.getElementById('{id}').value");
        //}

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
    }
}
