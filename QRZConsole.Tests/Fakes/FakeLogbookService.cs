using QRZConsole.Core.Models;
using QRZConsole.Core.Services;

namespace QRZConsole.Tests.Fakes
{
    /// <summary>In-memory stand-in for the QRZ.com scraper.</summary>
    internal sealed class FakeLogbookService : ILogbookService
    {
        public string Qrz { get; set; } = "";
        public string Password { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public bool Verbose { get; set; }
        public event Action<string>? Diagnostic;

        public List<LogbookEntry> Entries { get; } = Enumerable.Range(1, 12).Select(i => new LogbookEntry
        {
            Position = i,
            Call = $"IU0AA{i}",
            QSODateTime = new DateTime(2026, 10, 1, 12, 0, 0).AddDays(i),
            Band = "40m",
            Frequency = "7.076",
            Mode = "FT8",
            Country = "ITALY",
            DXCC = "248"
        }).ToList();

        public List<QsoData> Added { get; } = new();
        public List<int> Deleted { get; } = new();
        public List<int> PagesVisited { get; } = new();
        public bool LoginResult { get; set; } = true;
        public bool LoggedIn { get; set; } = true;
        public Exception? ThrowOnLookup { get; set; }
        public int EntriesPerPage { get; set; } = 25;

        public void Raise(string message) => Diagnostic?.Invoke(message);

        public Task InitializeAsync() => Task.CompletedTask;
        public Task<bool> IsLoggedAsync() => Task.FromResult(LoggedIn);
        public Task<bool> LoginAsync() => Task.FromResult(LoginResult);
        public Task<bool> LogOutAsync() => Task.FromResult(true);
        public Task<bool> GotoQrzHomeAsync() => Task.FromResult(true);
        public Task<bool> GotoLogbookAsync() => Task.FromResult(true);

        public Task<LookupEntry?> LookupAsync(string call)
        {
            if (ThrowOnLookup != null) throw ThrowOnLookup;
            return Task.FromResult<LookupEntry?>(new LookupEntry { QRZ = call, Name = "Test Operator", GridSquare = "JN70ab" });
        }

        public Task<string> CheckWorkedRawAsync(string call) => Task.FromResult("");
        public Task<int> GetQsoCountAsync() => Task.FromResult(Entries.Count);
        public Task<int> GetLogbookPagesAsync() => Task.FromResult(1);
        public Task<int> GetCurrentLogbookPageAsync() => Task.FromResult(1);

        public Task<int> GotoLogbookPageAsync(int page)
        {
            PagesVisited.Add(page);
            return Task.FromResult(page);
        }

        public Task<int> GetEntriesForPageAsync() => Task.FromResult(EntriesPerPage);

        public Task<int> SetEntriesForPageAsync(int entries)
        {
            EntriesPerPage = entries;
            return Task.FromResult(entries);
        }

        public Task<string> GetLogbookPageContentRawAsync(int page) => Task.FromResult("raw page");
        public Task<IReadOnlyList<LogbookEntry>> GetLogbookPageContentAsync(int page) => Task.FromResult<IReadOnlyList<LogbookEntry>>(Entries);

        public Task<IReadOnlyList<LogbookEntry>> GetLogbookEntriesByRangeAsync(int start, int end)
            => Task.FromResult<IReadOnlyList<LogbookEntry>>(Entries.Where(e => e.Position >= start && e.Position <= end).ToList());

        public Task<bool> SetLogbookDateOrderAsync(bool descending) => Task.FromResult(true);

        public Task<bool> AddQsoAsync(QsoData qso)
        {
            Added.Add(qso);
            return Task.FromResult(true);
        }

        public Task<bool> EditQsoAsync(int position, QsoData qso) => Task.FromResult(true);

        public Task<bool> DeleteQsoAsync(int position)
        {
            Deleted.Add(position);
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<string>> DescribeFormFieldsAsync() => Task.FromResult<IReadOnlyList<string>>(new[] { "input[text] id=a name=a value=\"\"" });
        public Task<string> DescribeCurrentPageAsync() => Task.FromResult("Page: \"test\" about:blank");
    }
}
