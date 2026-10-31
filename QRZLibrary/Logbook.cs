#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using QRZConsole.Core.Data;
using QRZConsole.Core.Models;
using QRZConsole.Core.Scraping;
using QRZConsole.Core.Services;
using static QRZLibrary.BrowserSession;

namespace QRZLibrary
{
    /// <summary>
    /// <see cref="ILogbookService"/> implemented by driving QRZ.com in an off-screen WebView2.
    /// Page data is read with JavaScript that returns JSON (the *Script constants at the end) and
    /// turned into models by <see cref="QrzPageParser"/>; ids and selectors are in <see cref="QrzSite"/>.
    /// </summary>
    public sealed class Logbook : ILogbookService
    {
        /// <summary>Time given to an AJAX reload to show its spinner.</summary>
        private static readonly TimeSpan SpinnerAppearWithin = TimeSpan.FromSeconds(1);

        /// <summary>Pause before checking the result of an action the old code also waited for.</summary>
        private static readonly TimeSpan ShortWait = TimeSpan.FromSeconds(1);

        /// <summary>Time given to optional content (e-mail, "seen before" table) to appear.</summary>
        private static readonly TimeSpan OptionalContentWait = TimeSpan.FromSeconds(2);

        private static readonly int[] ValidEntriesPerPage = { 5, 10, 15, 20, 25, 50, 100, 200 };

        private BrowserSession? _browser;
        private bool _verbose;

        public string Qrz { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; private set; } = string.Empty;

        public event Action<string>? Diagnostic;

        public bool Verbose
        {
            get => _verbose;
            set
            {
                _verbose = value;
                if (_browser != null) _browser.Verbose = value;
            }
        }

        private BrowserSession Browser => _browser ?? throw new InvalidOperationException("The browser is not initialized yet.");

        public async Task InitializeAsync()
        {
            _browser = await BrowserSession.CreateAsync();
            _browser.Verbose = _verbose;
            _browser.Diagnostic += message => Diagnostic?.Invoke(message);
        }

        #region Account

        public async Task<bool> IsLoggedAsync()
        {
            string url = Browser.Url;
            bool onQrzPage = url.Length > 0 && url != "about:blank"
                             && (url.Contains(QrzSite.Urls.Home) || url.Contains(QrzSite.Urls.Logbook) || url.Contains(QrzSite.Urls.Lookup));

            if (!onQrzPage && !await GotoQrzHomeAsync())
                return false;

            return await Browser.TestAsync($"document.querySelector({Js(QrzSite.Navigation.Menu)}).textContent.includes('Logout')");
        }

        public async Task<bool> LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (!await Browser.NavigateAsync(QrzSite.Urls.Login) || Browser.Title != QrzSite.Titles.Login)
                return Fail("Unable to open the login page.");

            // Step 1: username. The site then shows the operator name, or an error.
            await Browser.SetValueAsync(QrzSite.LoginForm.Username, Qrz);
            await Browser.RunAsync(QrzSite.LoginForm.NextStepScript);

            string fullName = $"(e => e ? e.innerText : '')(document.getElementById({Js(QrzSite.LoginForm.FullName)}))";
            string userError = $"(e => e ? e.innerText : '')(document.getElementById({Js(QrzSite.LoginForm.UsernameError)}))";
            bool usernameAccepted = false;

            await Browser.WaitUntilAsync(async () =>
            {
                string name = await Browser.TextAsync(fullName);
                if (name.Length > 0 && name != "Non Ham User")
                {
                    usernameAccepted = true;
                    return true;
                }

                if ((await Browser.TextAsync(userError)).Length > 0)
                {
                    ErrorMessage = "Invalid username!";
                    return true;
                }
                return false;
            }, "login: username accepted");

            if (!usernameAccepted)
                return false;

            // Step 2: password. Success lands on the callsign database page.
            await Browser.SetValueAsync(QrzSite.LoginForm.Password, Password);
            await Browser.RunAsync(QrzSite.LoginForm.NextStepScript);
            await Task.Delay(ShortWait); // let the page react before reading title and alerts

            string invalidAlert = $"document.querySelector({Js(QrzSite.LoginForm.InvalidPasswordAlert)}).innerText.includes({Js(QrzSite.LoginForm.InvalidPasswordText)})";
            bool loggedIn = false;

            await Browser.WaitUntilAsync(async () =>
            {
                if (Browser.Title == QrzSite.Titles.AfterLogin)
                {
                    loggedIn = true;
                    return true;
                }

                if (Browser.Title == QrzSite.Titles.Login && await Browser.TestAsync(invalidAlert))
                {
                    ErrorMessage = "Invalid username or password!";
                    return true;
                }
                return false;
            }, "login: password accepted");

            return loggedIn;
        }

        public async Task<bool> LogOutAsync()
        {
            if (!await IsLoggedAsync())
                return false;

            string menu = $"document.querySelector({Js(QrzSite.Navigation.MenuAny)}).textContent";
            if (!await Browser.TestAsync($"{menu}.includes('Logout')"))
                return false;

            string link;
            if (await Browser.TestAsync($"{menu}.includes('Edit')"))
                link = Browser.Url.Contains("op=add;addcall") ? QrzSite.Navigation.LogoutOnAddPage : QrzSite.Navigation.LogoutWithEditMenu;
            else
                link = QrzSite.Navigation.LogoutDefault;

            return await Browser.RunAsync($"document.querySelector({Js(link)}).click()");
        }

        #endregion

        #region Navigation

        public Task<bool> GotoQrzHomeAsync() => Browser.NavigateAsync(QrzSite.Urls.Home);

        public Task<bool> GotoLogbookAsync() => GotoLogbookAsync(forceReload: false);

        private async Task<bool> GotoLogbookAsync(bool forceReload)
        {
            if (!forceReload && Browser.Title == QrzSite.Titles.Logbook
                && await Browser.IdExistsAsync(QrzSite.Logbook.PageMarker)
                && await Browser.IdExistsAsync(QrzSite.Logbook.AddCallBox))
            {
                return true;
            }

            if (!await Browser.NavigateAsync(QrzSite.Urls.Logbook) || Browser.Title != QrzSite.Titles.Logbook)
                return false;

            // The QSO list may be (re)loaded via AJAX right after the page load.
            await Browser.WaitForIdleAsync(QrzSite.Logbook.Loading, SpinnerAppearWithin);
            return true;
        }

        private async Task<bool> GotoLookupAsync()
        {
            if (await Browser.IdExistsAsync(QrzSite.Lookup.QueryBox))
                return true;

            return await Browser.NavigateAsync(QrzSite.Urls.Lookup)
                   && (Browser.Title == QrzSite.Titles.LookupSearch || Browser.Title == QrzSite.Titles.Home);
        }

        /// <summary>Opens the "add QSO" form for a callsign.</summary>
        private async Task<bool> OpenAddCallAsync(string call)
        {
            return await Browser.NavigateAsync(QrzSite.Urls.AddCall + Uri.EscapeDataString(call))
                   && Browser.Title == QrzSite.Titles.Logbook
                   && await Browser.IdExistsAsync(QrzSite.QsoForm.RstSent);
        }

        #endregion

        #region Lookup

        public async Task<LookupEntry?> LookupAsync(string call)
        {
            call = call.ToUpperInvariant();
            if (!await GotoLookupAsync())
                return null;

            if (Browser.Title == QrzSite.Titles.Home)
                return new LookupEntry { QRZ = call, Name = "Too many lookups" };

            // Mark the current document: a result is accepted only from a new page, or when the
            // callsign box already shows the requested call (so a previous result is never read).
            await Browser.RunAsync("window.__qrzConsoleOldPage = true");
            await Browser.RunAsync($"document.querySelector({Js(QrzSite.Lookup.QueryInput)}).value = {Js(call)}");
            await Browser.ClickAsync(QrzSite.Lookup.Submit);

            string callData = $"document.getElementById({Js(QrzSite.Lookup.CallDataId)})";
            string resultArea = $"document.getElementById({Js(QrzSite.Lookup.ResultArea)})";
            string noResults = $"({resultArea} !== null && {resultArea}.innerText.includes({Js("no results for " + call)}))";
            string showsCall = $"({callData} !== null && {callData}.children.length > 0 && " +
                               $"{callData}.children[0].innerText.toUpperCase().replace(/Ø/g, '0').includes({Js(call)}))";
            string newPage = $"(window.__qrzConsoleOldPage !== true && {resultArea} !== null && ({callData} !== null || {noResults}))";

            if (!await Browser.WaitForAsync($"{newPage} || {showsCall} || {noResults}", $"lookup result for {call}"))
                return null;

            if (!await Browser.TestAsync($"{callData} !== null"))
                return new LookupEntry { QRZ = call, Name = "No result found!" };

            // Reveal the e-mail address and give it a moment to appear (not every operator has one).
            await Browser.RunAsync(QrzSite.Lookup.ShowEmailScript);
            await Browser.WaitUntilAsync(() => Browser.TestAsync($"{callData}.children[5].innerText.includes('@')"),
                                         "e-mail address", OptionalContentWait, reportTimeout: false);

            RawLookup? raw = await Browser.EvalAsync<RawLookup>(ExtractLookupScript);
            if (raw == null)
            {
                Browser.Report("Lookup: unable to read the callsign data box");
                return null;
            }

            return QrzPageParser.MapLookup(raw);
        }

        public async Task<string> CheckWorkedRawAsync(string call)
        {
            if (!await OpenAddCallAsync(call.ToUpperInvariant()))
                return string.Empty;

            // The "seen before" table is there only if the call was worked; give it time to load.
            string table = $"document.querySelector({Js(QrzSite.Logbook.SeenBeforeTable)})";
            await Browser.WaitUntilAsync(() => Browser.TestAsync($"{table} !== null"), "seen before table",
                                         OptionalContentWait, reportTimeout: false);

            List<List<string>>? rows = await Browser.EvalAsync<List<List<string>>>(
                $"(t => t ? Array.from(t.querySelectorAll('tr')).map(r => Array.from(r.querySelectorAll('td')).map(c => c.innerText)) : null)({table})");

            return rows == null ? string.Empty : QrzPageParser.FormatAllRows(rows);
        }

        #endregion

        #region Logbook pages

        public async Task<int> GetQsoCountAsync()
        {
            if (!await GotoLogbookAsync() || !await Browser.WaitForElementAsync(QrzSite.Logbook.QsoCount))
                return -1;

            return QrzPageParser.ParseInt(await Browser.TextAsync($"document.querySelector({Js(QrzSite.Logbook.QsoCount)}).innerText"));
        }

        public async Task<int> GetLogbookPagesAsync()
        {
            if (!await GotoLogbookAsync() || !await Browser.WaitForElementAsync(QrzSite.Logbook.PageCountLabel))
                return -1;

            return QrzPageParser.ParseInt(await Browser.TextAsync($"document.querySelector({Js(QrzSite.Logbook.PageCountLabel)}).innerText"));
        }

        public async Task<int> GetCurrentLogbookPageAsync()
        {
            if (!await GotoLogbookAsync() || !await Browser.IdExistsAsync(QrzSite.Logbook.PageMarker))
                return -1;

            int page = QrzPageParser.ParseInt(await Browser.GetValueAttributeAsync(QrzSite.Logbook.PageInput));
            return page > 0 ? page : -1;
        }

        public async Task<int> GotoLogbookPageAsync(int page)
        {
            if (!await GotoLogbookAsync() || !await Browser.IdExistsAsync(QrzSite.Logbook.PageInput))
                return -1;

            if (await Browser.GetValueAttributeAsync(QrzSite.Logbook.PageInput) == page.ToString(CultureInfo.InvariantCulture))
                return page;

            await Browser.SetValueAsync(QrzSite.Logbook.PageInput, page.ToString(CultureInfo.InvariantCulture));
            await Browser.RunAsync(QrzSite.Logbook.GotoPageScript);

            if (!await Browser.WaitForSpinnerAsync(QrzSite.Logbook.Loading, SpinnerAppearWithin))
                return -1;

            return await GetCurrentLogbookPageAsync();
        }

        public async Task<int> GetEntriesForPageAsync()
        {
            if (!await GotoLogbookAsync())
                return -1;

            return QrzPageParser.ParseInt(await Browser.GetValueAsync(QrzSite.Logbook.RowsPerPage));
        }

        public async Task<int> SetEntriesForPageAsync(int entries)
        {
            ErrorMessage = string.Empty;
            if (!ValidEntriesPerPage.Contains(entries))
            {
                Fail("Invalid parameter! Enter one of the following values: 5, 10, 15, 20, 25, 50, 100, 200");
                return -1;
            }

            int current = await GetEntriesForPageAsync();
            if (current <= 0)
                return -1;
            if (current == entries)
                return entries;

            await Browser.SetValueAsync(QrzSite.Logbook.RowsPerPage, entries.ToString(CultureInfo.InvariantCulture));
            await Browser.RunAsync(QrzSite.Logbook.RowsPerPageScript);

            if (!await Browser.WaitForSpinnerAsync(QrzSite.Logbook.Loading, SpinnerAppearWithin))
                return -1;

            return await GotoLogbookAsync(forceReload: true) ? await GetEntriesForPageAsync() : -1;
        }

        public async Task<string> GetLogbookPageContentRawAsync(int page)
        {
            if (await GotoLogbookPageAsync(page) != page
                || !await Browser.WaitForAsync($"{FindTableExpression} !== null", "logbook table"))
                return string.Empty;

            List<List<string>>? rows = await Browser.EvalAsync<List<List<string>>>(
                $"(t => t ? Array.from(t.querySelectorAll('tr')).map(r => Array.from(r.querySelectorAll('td')).map(c => c.innerText)) : null)" +
                $"({FindTableExpression})");

            return rows == null ? string.Empty : QrzPageParser.FormatRawRows(rows);
        }

        public async Task<IReadOnlyList<LogbookEntry>> GetLogbookPageContentAsync(int page)
        {
            if (await GotoLogbookPageAsync(page) != page)
                return Array.Empty<LogbookEntry>();

            RawLogbookTable? table = await ReadLogbookTableAsync();
            return table == null ? Array.Empty<LogbookEntry>() : QrzPageParser.MapPage(table);
        }

        public async Task<IReadOnlyList<LogbookEntry>> GetLogbookEntriesByRangeAsync(int start, int end)
        {
            var result = new List<LogbookEntry>();
            if (start < 1 || start > end)
                return result;

            int perPage = await GetEntriesForPageAsync();
            if (perPage <= 0)
                return result;

            int firstPage = QrzPageParser.PageOfPosition(start, perPage);
            int lastPage = QrzPageParser.PageOfPosition(end, perPage);

            for (int page = firstPage; page <= lastPage; page++)
            {
                if (await GotoLogbookPageAsync(page) != page)
                    break;

                RawLogbookTable? table = await ReadLogbookTableAsync();
                if (table == null)
                    break;

                result.AddRange(QrzPageParser.MapRange(table, start, end));
            }

            return result;
        }

        public async Task<bool> SetLogbookDateOrderAsync(bool descending)
        {
            if (!await GotoLogbookAsync() || !await Browser.WaitForElementAsync(QrzSite.Logbook.DateHeader))
                return false;

            // As in the original logic: to sort ascending the header must end up with class
            // sortDesc (and vice versa); clicking it switches the order.
            string toClick = descending ? QrzSite.Logbook.SortDescClass : QrzSite.Logbook.SortAscClass;
            string expected = descending ? QrzSite.Logbook.SortAscClass : QrzSite.Logbook.SortDescClass;
            string className = $"document.getElementById({Js(QrzSite.Logbook.DateHeaderId)}).className";

            if (await Browser.TestAsync($"{className}.includes({Js(toClick)})"))
            {
                await Browser.ClickAsync(QrzSite.Logbook.DateHeaderId);
                await Task.Delay(ShortWait);
                return await Browser.WaitForAsync($"{className}.includes({Js(expected)})", "date order change");
            }

            return await Browser.TestAsync($"{className}.includes({Js(expected)})");
        }

        private const string FindTableExpression =
            "(document.querySelector('" + QrzSite.Logbook.Table + "') || document.getElementById('" + QrzSite.Logbook.TableId + "'))";

        /// <summary>Waits for the logbook table and reads header ids and rows.</summary>
        private async Task<RawLogbookTable?> ReadLogbookTableAsync()
        {
            if (!await Browser.WaitForAsync($"{FindTableExpression} !== null", "logbook table"))
                return null;

            RawLogbookTable? table = await Browser.EvalAsync<RawLogbookTable>(ExtractTableScript);
            if (table == null || table.Headers.Count == 0)
            {
                Browser.Report("Logbook table found but its header could not be read");
                return null;
            }
            return table;
        }

        #endregion

        #region Add / edit / delete QSO

        public async Task<bool> AddQsoAsync(QsoData qso)
        {
            ErrorMessage = string.Empty;
            if (!await OpenAddCallAsync(qso.Call))
                return Fail("Unable to open the add QSO form.");

            if (!await FillQsoFormAsync(qso))
                return false;

            if (await Browser.IdExistsAsync(QrzSite.QsoForm.Save))
                await Browser.ClickAsync(QrzSite.QsoForm.Save);

            return await Browser.WaitForSpinnerAsync(QrzSite.Logbook.Loading, SpinnerAppearWithin);
        }

        public async Task<bool> EditQsoAsync(int position, QsoData qso)
        {
            ErrorMessage = string.Empty;
            if (!await OpenRecordAsync(position))
                return false;

            bool editFormOpen = await Browser.WaitForNavigationAsync(() => Browser.RunAsync(QrzSite.Logbook.EditScript), "QSO edit form")
                                && Browser.Title == QrzSite.Titles.Logbook
                                && await Browser.WaitForElementAsync("#" + QrzSite.QsoForm.StartDate);
            if (!editFormOpen)
                return Fail("Unable to open the edit form.");

            if (!await FillQsoFormAsync(qso))
                return false;

            if (await Browser.IdExistsAsync(QrzSite.QsoForm.Save))
                await Browser.ClickAsync(QrzSite.QsoForm.Save);

            await Task.Delay(ShortWait);
            return await Browser.WaitUntilAsync(async () => Browser.Title == QrzSite.Titles.Logbook && await IsQsoDetailShownAsync(),
                                                "QSO detail after saving");
        }

        public async Task<bool> DeleteQsoAsync(int position)
        {
            ErrorMessage = string.Empty;
            if (!await OpenRecordAsync(position))
                return false;

            string form = $"document.getElementById({Js(QrzSite.Logbook.RecordForm)})";
            await Browser.RunAsync($"{form}.getElementsByTagName('input').namedItem('op').value = {Js(QrzSite.Logbook.DeleteOperation)}");

            return await Browser.WaitForNavigationAsync(() => Browser.RunAsync($"{form}.submit()"), "logbook after delete")
                   && Browser.Title == QrzSite.Titles.Logbook
                   && await Browser.WaitForElementAsync("#" + QrzSite.Logbook.AddCallBox);
        }

        /// <summary>Opens the detail page of the QSO at a logbook position (1-based).</summary>
        private async Task<bool> OpenRecordAsync(int position)
        {
            int perPage = await GetEntriesForPageAsync();
            int page = QrzPageParser.PageOfPosition(position, perPage);
            if (page < 1 || await GotoLogbookPageAsync(page) != page)
                return Fail($"Unable to show the logbook page with position {position}.");

            RawLogbookTable? table = await ReadLogbookTableAsync();
            RawLogbookRow? row = table?.Rows.FirstOrDefault(r => QrzPageParser.RowPosition(r) == position);
            if (row == null)
                return Fail($"QSO at position {position} not found.");

            string form = $"document.getElementById({Js(QrzSite.Logbook.RecordForm)})";
            await Browser.RunAsync($"{form}.getElementsByTagName('input').namedItem('bookid').value = {Js(row.BookId)}");
            await Browser.RunAsync($"{form}.getElementsByTagName('input').namedItem('logpos').value = {Js(row.Pos)}");

            bool opened = await Browser.WaitForNavigationAsync(() => Browser.RunAsync($"{form}.submit()"), $"QSO detail at position {position}")
                          && Browser.Title == QrzSite.Titles.Logbook
                          && await Browser.WaitUntilAsync(IsQsoDetailShownAsync, "QSO detail");

            return opened || Fail($"Unable to open the QSO at position {position}.");
        }

        private Task<bool> IsQsoDetailShownAsync()
        {
            return Browser.TestAsync($"(b => b !== null && b.innerText.includes({Js(QrzSite.Logbook.QsoDetailText)}))" +
                                     $"(document.getElementById({Js(QrzSite.Logbook.Body)}))");
        }

        /// <summary>Fills date, time, frequency/band, mode, RST and comment of the add/edit form.</summary>
        private async Task<bool> FillQsoFormAsync(QsoData qso)
        {
            if (!DateTime.TryParse(qso.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                return Fail($"Invalid date \"{qso.Date}\" (use yyyy-MM-dd).");

            if (!TimeSpan.TryParse(qso.Time, CultureInfo.InvariantCulture, out _))
                return Fail($"Invalid time \"{qso.Time}\" (use HH:mm).");

            if (!HamData.TryParseMHz(qso.Frequency, out double mhz))
                return Fail($"Invalid frequency \"{qso.Frequency}\".");

            string? band = HamData.FindBand(mhz);
            if (band == null)
                return Fail($"Frequency {qso.Frequency} MHz is outside the amateur bands.");

            if (!HamData.IsValidMode(qso.Mode))
                return Fail($"Mode \"{qso.Mode}\" is not accepted by the QRZ logbook.");

            // Fields the old code required (the others are set only when present).
            string[] required =
            {
                QrzSite.QsoForm.StartDate, QrzSite.QsoForm.EndDate, QrzSite.QsoForm.StartTime, QrzSite.QsoForm.EndTime,
                QrzSite.QsoForm.Freq2, QrzSite.QsoForm.Band2, QrzSite.QsoForm.Band1,
                QrzSite.QsoForm.RstSent, QrzSite.QsoForm.RstReceived
            };
            foreach (string id in required)
            {
                if (!await Browser.IdExistsAsync(id))
                    return Fail($"Field #{id} not found in the QSO form.");
            }

            await Browser.SetValueAsync(QrzSite.QsoForm.StartDate, qso.Date);
            await Browser.SetValueAsync(QrzSite.QsoForm.EndDate, qso.Date);
            await Browser.SetValueAsync(QrzSite.QsoForm.StartTime, qso.Time);
            await Browser.SetValueAsync(QrzSite.QsoForm.EndTime, qso.Time);

            // The old code also called the site's setFreq()/setMode() with malformed JavaScript
            // (setFreq('2, '0')), so they never ran: band and mode are set directly, as before.
            await Browser.SetValueAsync(QrzSite.QsoForm.Freq2, qso.Frequency);
            await Browser.SetValueAsync(QrzSite.QsoForm.Band2, band);
            await SetIfPresentAsync(QrzSite.QsoForm.Freq1, qso.Frequency);
            await Browser.SetValueAsync(QrzSite.QsoForm.Band1, band);
            await SetIfPresentAsync(QrzSite.QsoForm.Mode2, qso.Mode);
            await SetIfPresentAsync(QrzSite.QsoForm.Mode1, qso.Mode);
            await Browser.SetValueAsync(QrzSite.QsoForm.RstSent, QrzSite.QsoForm.DefaultRst);
            await Browser.SetValueAsync(QrzSite.QsoForm.RstReceived, QrzSite.QsoForm.DefaultRst);

            if (qso.Comment.Length > 0)
            {
                bool commentSet = await Browser.ExistsAsync(QrzSite.QsoForm.Comments)
                                  && await Browser.SetValueBySelectorAsync(QrzSite.QsoForm.Comments, qso.Comment);
                if (!commentSet)
                    Browser.Report($"Comment not saved: field {QrzSite.QsoForm.Comments} not found (use \"bf comment\" on the add QSO page)");
            }

            return true;
        }

        private async Task<bool> SetIfPresentAsync(string id, string value)
        {
            return await Browser.IdExistsAsync(id) && await Browser.SetValueAsync(id, value);
        }

        private bool Fail(string message)
        {
            ErrorMessage = message;
            return false;
        }

        #endregion

        #region Diagnostics

        public async Task<IReadOnlyList<string>> DescribeFormFieldsAsync()
        {
            List<string>? fields = await Browser.EvalAsync<List<string>>(DescribeFieldsScript);
            return fields ?? new List<string>();
        }

        public Task<string> DescribeCurrentPageAsync() => Task.FromResult($"Page: \"{Browser.Title}\" {Browser.Url}");

        #endregion

        #region Extraction scripts

        /// <summary>Reads #lbtab: header cell ids and, for each body row, its data-* attributes and cell texts.</summary>
        private const string ExtractTableScript = @"(t => {
  if (!t) return null;
  const thead = t.querySelector('thead');
  const tbody = t.querySelector('tbody');
  if (!thead || !tbody) return null;
  return {
    headers: Array.from(thead.querySelectorAll('th')).map(th => th.id || ''),
    rows: Array.from(tbody.querySelectorAll('tr')).map(tr => ({
      pos: tr.getAttribute('data-pos') || '',
      rowNum: tr.getAttribute('data-rownum') || '',
      bookId: tr.getAttribute('data-bookid') || '',
      className: tr.className || '',
      cells: Array.from(tr.querySelectorAll('td')).map(td => td.innerText || '')
    }))
  };
})(" + FindTableExpression + ")";

        /// <summary>Reads the callsign box (#csdata children) and the detail table (label/value pairs).</summary>
        private const string ExtractLookupScript = @"(() => {
  const cs = document.querySelector('" + QrzSite.Lookup.CallData + @"') || document.getElementById('" + QrzSite.Lookup.CallDataId + @"');
  if (!cs) return null;
  const c = cs.children;
  const text = e => e ? (e.innerText || '') : '';
  const dt = document.querySelector('" + QrzSite.Lookup.Details + @"') || document.getElementById('" + QrzSite.Lookup.DetailsId + @"');
  const details = [];
  if (dt) {
    for (const td of dt.querySelectorAll('td')) {
      details.push([text(td), text(td.nextElementSibling)]);
    }
  }
  return {
    call: text(c[0]),
    countryHtml: c[1] ? c[1].outerHTML : '',
    country: text(c[1]),
    name: c[3] && c[3].children.length > 0 ? text(c[3].children[0]) : '',
    nameAndAddress: text(c[3]),
    email: text(c[5]),
    details: details
  };
})()";

        /// <summary>One line per form field: tag, type, id, name, visibility, label and value (passwords hidden).</summary>
        private const string DescribeFieldsScript = @"Array.from(document.querySelectorAll('input, select, textarea'))
  .filter(e => (e.id || e.name) && !(e.id || '').startsWith('fc-') && !e.closest('.fc-consent-root, .fc-dialog-container'))
  .map(e => {
  const type = (e.type || '').toLowerCase();
  const visible = !!(e.offsetWidth || e.offsetHeight || e.getClientRects().length);
  let value = type === 'password' ? '***' : String(e.value || '');
  if (value.length > 40) value = value.substring(0, 37) + '...';
  const label = e.labels && e.labels.length > 0 ? e.labels[0].innerText.trim() : '';
  return e.tagName.toLowerCase() + (type ? '[' + type + ']' : '') +
         ' id=' + (e.id || '-') + ' name=' + (e.name || '-') +
         (visible ? '' : ' (hidden)') + (label ? ' label=""' + label + '""' : '') +
         ' value=""' + value + '""';
})";

        #endregion
    }
}
