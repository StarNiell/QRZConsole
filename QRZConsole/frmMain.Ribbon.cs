#nullable enable
using System.Windows.Forms;
using QRZConsole.Core.Session;
using QRZConsole.UI;

namespace QRZConsole
{
    /// <summary>
    /// Ribbon content. Every button runs a console command (shown in its tooltip), so what can be
    /// done with the mouse can also be typed.
    /// </summary>
    public partial class frmMain
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private TextBox txtCallsign = null!;
        private TextBox txtPage = null!;
        private TextBox txtBand = null!;

        private RibbonButton btnViewRaw = null!;
        private RibbonButton btnViewText = null!;
        private RibbonButton btnViewAdif = null!;
        private RibbonButton btnClusterDebug = null!;
        private RibbonButton btnCheckAtStartup = null!;
        private RibbonButton btnRibbonAtStartup = null!;
        private RibbonButton btnBrowserDebug = null!;

        private void BuildRibbon()
        {
            // ---------------------------------------------------------------- Home
            RibbonTab home = ribbon.AddTab("Home");

            RibbonGroup account = home.AddGroup("Account");
            txtUsername = account.AddField("Callsign", 90, "QRZ.com username (your callsign)");
            txtPassword = account.AddField("Password", 90, "QRZ.com password (saved encrypted)", password: true);
            account.AddSmall(Glyphs.Refresh, "Check login", "Is logged? (il)", () => RunFromRibbon("il"));
            account.AddLarge(Glyphs.Contact, "Log in", "Log in with callsign and password (li)", () => RunFromRibbon("li"));
            account.AddLarge(Glyphs.Power, "Log out", "Log out (lo)", () => RunFromRibbon("lo"));

            RibbonGroup lookup = home.AddGroup("Lookup");
            txtCallsign = lookup.AddField("Call", 100, "Callsign to look up (ENTER = lookup and check worked)");
            lookup.AddSmall(Glyphs.Search, "Lookup", "Lookup (lu)", () => RunFromRibbon($"lu {txtCallsign.Text.Trim()}"));
            lookup.AddSmall(Glyphs.History, "Worked before", "Check worked (cw)", () => RunFromRibbon($"cw {txtCallsign.Text.Trim()}"));
            lookup.AddLarge(Glyphs.Accept, "Lookup + worked", "Lookup and check worked (lw)", () => RunFromRibbon($"lw {txtCallsign.Text.Trim()}"));

            RibbonGroup qso = home.AddGroup("QSO");
            qso.AddLarge(Glyphs.Calendar, "Add QSO", "Add QSO: prepares \"aq\" with last call, frequency, mode and UTC time", () => PrepareCommand("aq "));
            qso.AddSmall(Glyphs.Document, "Edit QSO", "Edit QSO (eq [position] ...)", () => PrepareCommand("eq "));
            qso.AddSmall(Glyphs.Delete, "Delete QSO", "Delete QSO (dq [position] [position] [position])", () => PrepareCommand("dq "));
            qso.AddSmall(Glyphs.Info, "Last data", "Last call, frequency and mode in memory (ld)", () => RunFromRibbon("ld"));

            RibbonGroup consoleGroup = home.AddGroup("Console");
            consoleGroup.AddLarge(Glyphs.List, "Commands", "Command list (cm)", () => RunFromRibbon("cm"));
            consoleGroup.AddSmall(Glyphs.Delete, "Clear", "Clear the console (cl)", () => RunFromRibbon("cl"));
            consoleGroup.AddSmall(Glyphs.Keyboard, "Shortcuts", "Keyboard shortcuts (sl)", () => RunFromRibbon("sl"));
            consoleGroup.AddSmall(Glyphs.Search, "Find DXCC", "Find DXCC by country: prepares \"fc\"", () => PrepareCommand("fc "));

            // ---------------------------------------------------------------- Logbook
            RibbonTab logbook = ribbon.AddTab("Logbook");

            RibbonGroup open = logbook.AddGroup("Logbook");
            open.AddLarge(Glyphs.Library, "Open logbook", "Open the logbook and show the current page (lb)", () => RunFromRibbon("lb"));
            open.AddSmall(Glyphs.Info, "QSO count", "Number of QSOs (qc)", () => RunFromRibbon("qc"));
            open.AddSmall(Glyphs.Page, "Pages", "Number of pages (lp)", () => RunFromRibbon("lp"));
            open.AddSmall(Glyphs.Settings, "QSO per page", "QSOs per page (qp)", () => RunFromRibbon("qp"));

            RibbonGroup navigate = logbook.AddGroup("Navigate");
            navigate.AddLarge(Glyphs.Back, "Previous page", "Previous page (pd) - CTRL+PgDn", () => RunFromRibbon("pd"));
            txtPage = navigate.AddField("Page", 45, "Logbook page (ENTER = go to page)");
            navigate.AddSmall(Glyphs.Forward, "Go to page", "Go to page (gp)", () => RunFromRibbon($"gp {PageFromField()}"));
            navigate.AddSmall(Glyphs.Refresh, "Current page", "Current page (cp)", () => RunFromRibbon("cp"));
            navigate.AddLarge(Glyphs.Forward, "Next page", "Next page (pu) - CTRL+PgUp", () => RunFromRibbon("pu"));

            RibbonGroup viewMode = logbook.AddGroup("View mode");
            btnViewRaw = viewMode.AddToggle(Glyphs.Document, "Raw", "Pages shown as raw text (cv raw)", () => RunFromRibbon("cv raw"));
            btnViewText = viewMode.AddToggle(Glyphs.View, "Table", "Pages shown as table (cv text)", () => RunFromRibbon("cv text"));
            btnViewAdif = viewMode.AddToggle(Glyphs.Page, "ADIF", "Pages shown as ADIF (cv adif)", () => RunFromRibbon("cv adif"));

            RibbonGroup show = logbook.AddGroup("Show page");
            show.AddSmall(Glyphs.View, "Table", "Page as table (tt)", () => RunFromRibbon($"tt {PageFromField()}"));
            show.AddSmall(Glyphs.Document, "Raw", "Page as raw text (tr)", () => RunFromRibbon($"tr {PageFromField()}"));
            show.AddSmall(Glyphs.Page, "XML", "Page as XML (tx)", () => RunFromRibbon($"tx {PageFromField()}"));
            show.AddLarge(Glyphs.Filter, "QSO range", "QSOs by position: prepares \"qt [start] [end]\"", () => PrepareCommand("qt "));

            RibbonGroup sort = logbook.AddGroup("Sort");
            sort.AddSmall(Glyphs.Sort, "Date ascending", "Order by date ascending (da)", () => RunFromRibbon("da"));
            sort.AddSmall(Glyphs.Sort, "Date descending", "Order by date descending (dd)", () => RunFromRibbon("dd"));

            // ---------------------------------------------------------------- Cluster
            RibbonTab cluster = ribbon.AddTab("Cluster DX");

            RibbonGroup connection = cluster.AddGroup("Connection");
            connection.AddLarge(Glyphs.Link, "Connect", "Connect and log in to the Cluster DX (dx)", () => RunFromRibbon("dx"));
            connection.AddLarge(Glyphs.Cancel, "Disconnect", "Close the Cluster DX (cc)", () => RunFromRibbon("cc"));

            RibbonGroup spots = cluster.AddGroup("Spots");
            txtBand = spots.AddField("Band", 55, "Band for \"Show on band\": 40m, 20m, hf, vhf... (ENTER = show)");
            spots.AddSmall(Glyphs.Globe, "Show on band", "Spots on band (sb)", () => RunFromRibbon($"sb {txtBand.Text.Trim()}".TrimEnd()));
            spots.AddSmall(Glyphs.Filter, "Show on frequency", "Spots on the current frequency (sf)", () => RunFromRibbon("sf"));
            spots.AddSmall(Glyphs.Search, "Search current call", "Spots of the current QRZ (ds)", () => RunFromRibbon($"ds {_session.CurrentQrz}".TrimEnd()));
            spots.AddLarge(Glyphs.Send, "Send spot", "Send a spot: prepares \"ss\" with the current call and frequency", () => PrepareCommand("ss "));

            RibbonGroup clusterOptions = cluster.AddGroup("Options");
            clusterOptions.AddSmall(Glyphs.Settings, "DXSpider command", "Raw DXSpider command: prepares \"dc\"", () => PrepareCommand("dc "));
            btnClusterDebug = clusterOptions.AddToggle(Glyphs.Diagnostic, "Debug", "Show everything the cluster sends (cd)", () => RunFromRibbon("cd"));

            // ---------------------------------------------------------------- View
            RibbonTab view = ribbon.AddTab("View");

            RibbonGroup appearance = view.AddGroup("Appearance");
            appearance.AddLarge(Glyphs.Color, "Theme", "Colors and font of the console (th)", () => RunFromRibbon("th"));
            appearance.AddSmall(Glyphs.ZoomIn, "Zoom in", "Bigger text - CTRL++", () => Zoom(+1));
            appearance.AddSmall(Glyphs.ZoomOut, "Zoom out", "Smaller text - CTRL+-", () => Zoom(-1));
            appearance.AddSmall(Glyphs.Refresh, "Reset zoom", "Default text size - CTRL+0", ZoomReset);
            appearance.AddLarge(Glyphs.FullScreen, "Full screen", "Maximize / restore (fs)", () => RunFromRibbon("fs"));

            RibbonGroup startup = view.AddGroup("Startup");
            btnCheckAtStartup = startup.AddToggle(Glyphs.CheckMark, "Check login at startup", "Check the login when the program starts (sc)", () => RunFromRibbon("sc"));
            btnRibbonAtStartup = startup.AddToggle(Glyphs.View, "Show ribbon at startup", "Ribbon expanded when the program starts", ToggleRibbonAtStartup);

            RibbonGroup diagnostics = view.AddGroup("Diagnostics");
            btnBrowserDebug = diagnostics.AddToggle(Glyphs.Diagnostic, "Browser debug", "Show every step of the QRZ.com browser (bd)", () => RunFromRibbon("bd"));
            diagnostics.AddSmall(Glyphs.List, "Form fields", "Fields of the page loaded in the browser (bf)", () => RunFromRibbon("bf"));

            // ENTER in the ribbon fields runs the matching command.
            OnEnter(txtUsername, () => RunFromRibbon("li"));
            OnEnter(txtPassword, () => RunFromRibbon("li"));
            OnEnter(txtCallsign, () => RunFromRibbon($"lw {txtCallsign.Text.Trim()}"));
            OnEnter(txtPage, () => RunFromRibbon($"gp {PageFromField()}"));
            OnEnter(txtBand, () => RunFromRibbon($"sb {txtBand.Text.Trim()}".TrimEnd()));

            // Fields -> session (the session is what the commands use).
            txtUsername.TextChanged += (s, e) => _session.Username = txtUsername.Text;
            txtPassword.TextChanged += (s, e) => _session.Password = txtPassword.Text;
            txtPage.TextChanged += (s, e) =>
            {
                if (int.TryParse(txtPage.Text, out int page) && page > 0)
                    _session.CurrentPage = page;
            };
        }

        private static void OnEnter(TextBox box, System.Action action)
        {
            box.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                action();
            };
        }

        private string PageFromField() => int.TryParse(txtPage.Text, out int page) && page > 0 ? page.ToString() : "1";

        private void ToggleRibbonAtStartup()
        {
            _settings.ShowHeader = !_settings.ShowHeader;
            WriteLine($"Show ribbon at startup = {(_settings.ShowHeader ? "enabled" : "disabled")}");
            UpdateStatus();
        }

        /// <summary>Check state of the toggle buttons.</summary>
        private void UpdateRibbonState()
        {
            if (btnViewRaw == null) return;
            btnViewRaw.Checked = _session.ViewMode == ViewMode.Raw;
            btnViewText.Checked = _session.ViewMode == ViewMode.Text;
            btnViewAdif.Checked = _session.ViewMode == ViewMode.Adif;
            btnClusterDebug.Checked = _cluster.Debug;
            btnCheckAtStartup.Checked = _settings.CheckIsLoggedAtStartup;
            btnRibbonAtStartup.Checked = _settings.ShowHeader;
            btnBrowserDebug.Checked = _logbook.Verbose;
        }
    }
}
