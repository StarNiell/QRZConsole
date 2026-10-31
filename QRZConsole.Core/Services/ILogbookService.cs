using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QRZConsole.Core.Models;

namespace QRZConsole.Core.Services
{
    /// <summary>
    /// Operations on the QRZ.com website (account, lookup, logbook).
    /// Implemented by the WebView2 scraper in QRZLibrary; commands only see this interface.
    /// </summary>
    public interface ILogbookService
    {
        /// <summary>Callsign used to log in.</summary>
        string Qrz { get; set; }

        /// <summary>Password used to log in.</summary>
        string Password { get; set; }

        /// <summary>Reason of the last failure, when the site gave one (e.g. "Invalid username or password!").</summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Messages about what the scraper is doing: timeouts and script errors always,
        /// every step when <see cref="Verbose"/> is on. Raised on the UI thread.
        /// </summary>
        event Action<string>? Diagnostic;

        /// <summary>Reports every navigation and wait through <see cref="Diagnostic"/>.</summary>
        bool Verbose { get; set; }

        Task InitializeAsync();

        Task<bool> IsLoggedAsync();
        Task<bool> LoginAsync();
        Task<bool> LogOutAsync();
        Task<bool> GotoQrzHomeAsync();
        Task<bool> GotoLogbookAsync();

        /// <summary>Looks up a callsign. Returns null when the lookup page could not be used.</summary>
        Task<LookupEntry?> LookupAsync(string call);

        /// <summary>The "seen before" table for a callsign, tab-separated, one QSO per line.</summary>
        Task<string> CheckWorkedRawAsync(string call);

        Task<int> GetQsoCountAsync();
        Task<int> GetLogbookPagesAsync();
        Task<int> GetCurrentLogbookPageAsync();

        /// <summary>Moves to a logbook page and returns the page actually shown (-1 on failure).</summary>
        Task<int> GotoLogbookPageAsync(int page);

        Task<int> GetEntriesForPageAsync();

        /// <summary>Sets QSOs per page (5, 10, 15, 20, 25, 50, 100, 200) and returns the value now in use (-1 on failure).</summary>
        Task<int> SetEntriesForPageAsync(int entries);

        Task<string> GetLogbookPageContentRawAsync(int page);
        Task<IReadOnlyList<LogbookEntry>> GetLogbookPageContentAsync(int page);
        Task<IReadOnlyList<LogbookEntry>> GetLogbookEntriesByRangeAsync(int start, int end);

        /// <summary>Sorts the logbook by date. Returns true when the requested order is in effect.</summary>
        Task<bool> SetLogbookDateOrderAsync(bool descending);

        Task<bool> AddQsoAsync(QsoData qso);
        Task<bool> EditQsoAsync(int position, QsoData qso);
        Task<bool> DeleteQsoAsync(int position);

        /// <summary>
        /// Describes the form fields (input, select, textarea) of the page currently loaded:
        /// one line per field. Used to find the right element when the site changes.
        /// </summary>
        Task<IReadOnlyList<string>> DescribeFormFieldsAsync();

        /// <summary>Title and address of the page currently loaded.</summary>
        Task<string> DescribeCurrentPageAsync();
    }
}
