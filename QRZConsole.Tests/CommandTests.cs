using QRZConsole.Core.Cluster;
using QRZConsole.Core.Commands;
using QRZConsole.Core.Models;
using QRZConsole.Core.Session;
using QRZConsole.Core.Settings;
using QRZConsole.Tests.Fakes;
using QRZNetwork;

namespace QRZConsole.Tests
{
    public class CommandTests : IDisposable
    {
        private readonly RecordingHost _host = new();
        private readonly FakeLogbookService _logbook = new();
        private readonly SessionState _session = new();
        private readonly InMemorySettingsStore _store = new();
        private readonly AppSettings _settings;
        private readonly DxClusterService _cluster;
        private readonly CommandProcessor _processor;

        public CommandTests()
        {
            _settings = new AppSettings(_store);
            _cluster = new DxClusterService(_host, new WinsockClient(null));
            _processor = new CommandProcessor(new CommandContext(_host, _logbook, _session, _settings, _cluster, CommandCatalog.CreateDefault()));
        }

        public void Dispose() => _cluster.Dispose();

        private Task Run(string line) => _processor.ExecuteAsync(line);

        [Fact]
        public void Catalog_has_unique_names_and_every_command_is_listed()
        {
            CommandCatalog catalog = CommandCatalog.CreateDefault(); // throws on duplicate names
            var host = new RecordingHost();
            catalog.WriteCommandList(host);

            foreach (Command c in catalog.Commands)
                Assert.Contains(host.Lines, l => l.EndsWith(c.Usage, StringComparison.Ordinal));
            Assert.Same(catalog.Find("qi"), catalog.Find("exit"));
        }

        [Fact]
        public async Task Unknown_command_is_reported()
        {
            await Run("zz 1 2");
            Assert.Contains(_host.Lines, l => l.StartsWith("Invalid command: zz 1 2"));
        }

        [Fact]
        public async Task Exceptions_are_printed_not_thrown()
        {
            _logbook.ThrowOnLookup = new InvalidOperationException("boom");

            await Run("lu IU8NQI");

            Assert.Contains(_host.Lines, l => l.Contains("#Error") && l.Contains("boom"));
            await Run("ld"); // processor still usable afterwards
            Assert.Contains(_host.Lines, l => l.StartsWith("Last data in memory"));
        }

        [Fact]
        public async Task Add_qso_normalizes_frequency_and_keeps_free_comment()
        {
            await Run("aq iu0abc 7,076 ft8 2026-10-31 14:05 nice  qso 73");

            QsoData qso = Assert.Single(_logbook.Added);
            Assert.Equal(new QsoData("IU0ABC", "7.076", "FT8", "2026-10-31", "14:05", "nice  qso 73"), qso);
            Assert.Equal("IU0ABC", _session.CurrentQrz);
            Assert.Equal("7076", _session.LastFrequency);
            Assert.Equal("FT8", _session.LastMode);
        }

        [Theory]
        [InlineData("aq 14074 FT8 2026-10-31")]
        [InlineData("aq [CALL] 7076 FT8 2026-10-31 14:05")]
        public async Task Add_qso_with_missing_parts_is_rejected(string line)
        {
            await Run(line);

            Assert.Empty(_logbook.Added);
            Assert.Contains(_host.Lines, l => l.StartsWith("Invalid command!"));
        }

        [Theory]
        [InlineData("7076", "7.076")]
        [InlineData("14074", "14.074")]
        [InlineData("7.076", "7.076")]
        [InlineData("144", "144")]
        public async Task Frequency_in_kHz_becomes_MHz(string typed, string expected)
        {
            await Run($"aq K1ABC {typed} SSB 2026-10-31 14:05");
            Assert.Equal(expected, Assert.Single(_logbook.Added).Frequency);
        }

        [Fact]
        public async Task Delete_requires_three_equal_positions()
        {
            await Run("dq 5 5 6");
            Assert.Empty(_logbook.Deleted);

            await Run("dq 5 5 5");
            Assert.Equal(new[] { 5 }, _logbook.Deleted);
        }

        [Fact]
        public async Task Qso_per_page_accepts_only_site_values()
        {
            await Run("qp 7");
            Assert.Equal(25, _logbook.EntriesPerPage);

            await Run("qp 50");
            Assert.Equal(50, _logbook.EntriesPerPage);
            Assert.Contains("QSO for page = 50", _host.Lines);
        }

        [Fact]
        public async Task View_mode_is_saved_and_used_by_page_navigation()
        {
            await Run("cv adif");
            Assert.Equal(ViewMode.Adif, _session.ViewMode);
            Assert.Equal("adif", _settings.ViewMode);

            await Run("pu");
            Assert.Equal(2, _session.CurrentPage);
            Assert.Contains(_host.Lines, l => l.StartsWith("Get Table content ADIF of page 2"));

            await Run("cv nonsense");
            Assert.Contains("Invalid View Mode! (mode is [raw], [text] or [adif])", _host.Lines);
        }

        [Fact]
        public async Task Page_down_never_goes_below_one()
        {
            await Run("pd");
            await Run("pd");

            Assert.Equal(new[] { 1, 1 }, _logbook.PagesVisited);
            Assert.Equal(1, _session.CurrentPage);
        }

        [Fact]
        public async Task Range_requires_valid_positions()
        {
            await Run("qt 0");
            Assert.Contains(_host.Lines, l => l.StartsWith("Invalid range!"));

            await Run("qa 3 4");
            Assert.Contains("Found 2 entries", _host.Lines);
        }

        [Fact]
        public async Task Login_without_arguments_uses_session_credentials_and_saves_them()
        {
            _session.Username = "iu8nqi";
            _session.Password = "secret";

            await Run("li");

            Assert.Equal("IU8NQI", _logbook.Qrz);
            Assert.Equal("secret", _logbook.Password);
            Assert.True(_settings.IsLoggedIn);
            Assert.Equal("IU8NQI", _settings.Username);
            Assert.DoesNotContain(_host.Lines, l => l.Contains("secret"));
        }

        [Fact]
        public async Task Login_failure_shows_the_site_reason()
        {
            _logbook.LoginResult = false;
            _logbook.ErrorMessage = "Invalid username or password!";

            await Run("li K1ABC wrong");

            Assert.False(_settings.IsLoggedIn);
            Assert.Contains("Invalid username or password!", _host.Lines);
        }

        [Fact]
        public async Task Logout_clears_credentials()
        {
            _settings.IsLoggedIn = true;
            _settings.Username = "IU8NQI";
            _session.Username = "IU8NQI";
            _session.CurrentPage = 4;

            await Run("lo");

            Assert.False(_settings.IsLoggedIn);
            Assert.Equal("", _settings.Username);
            Assert.Equal("", _session.Username);
            Assert.Equal(1, _session.CurrentPage);
        }

        [Fact]
        public async Task Lookup_marks_worked_locators()
        {
            _session.Locators = new List<KeyValuePair<string, bool>> { new("JN70", true) };

            await Run("lu iu8nqi");

            Assert.Equal("IU8NQI", _session.CurrentQrz);
            Assert.Equal("IU8NQI", _settings.LastLookup);
            Assert.Contains(_host.Lines, l => l.Contains("JN70ab") && l.Contains("Worked and Confirmed"));
        }

        [Fact]
        public async Task Cluster_commands_need_a_connection()
        {
            await Run("sb 40m");
            Assert.Contains(_host.Lines, l => l.StartsWith("Cluster DX not available"));

            await Run("dx");
            Assert.Contains(_host.Lines, l => l.StartsWith("Please! Before connecting"));
        }

        [Fact]
        public async Task Browser_commands_toggle_verbose_and_list_fields()
        {
            await Run("bd");
            Assert.True(_logbook.Verbose);

            await Run("bf");
            Assert.Contains("Found 1 fields", _host.Lines);
        }

        [Fact]
        public async Task Browser_fields_can_be_filtered()
        {
            await Run("bf NAME=A");
            Assert.Contains("Found 1 fields", _host.Lines);

            await Run("bf comment");
            Assert.Contains("Found 0 fields", _host.Lines);
        }

        [Fact]
        public async Task Ui_commands_are_forwarded_to_the_host()
        {
            await Run("cl");
            await Run("sw");
            await Run("exit");

            Assert.Equal(1, _host.ClearCount);
            Assert.Equal(1, _host.ToggleHeaderCount);
            Assert.True(_host.QuitRequested);
        }

        [Fact]
        public async Task Theme_command_opens_editor_lists_and_applies()
        {
            await Run("th");
            Assert.Equal(1, _host.ThemeEditorCount);

            await Run("th list");
            Assert.Contains("Themes: Classic, Dark", _host.Lines);

            await Run("th dark");
            Assert.Equal("dark", _host.AppliedTheme);

            await Run("th nope");
            Assert.Contains(_host.Lines, l => l.StartsWith("Unknown theme"));
        }

        [Fact]
        public async Task Find_dxcc_uses_the_whole_text()
        {
            await Run("fc south sudan");
            Assert.Contains(_host.Lines, l => l.Contains("SOUTH SUDAN"));
        }
    }
}
