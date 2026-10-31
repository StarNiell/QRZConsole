#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using QRZConsole.Core.Cluster;
using QRZConsole.Core.Commands;
using QRZConsole.Core.Services;
using QRZConsole.Core.Session;
using QRZConsole.Core.Settings;
using QRZConsole.UI;
using QRZLibrary;
using QRZNetwork;

namespace QRZConsole
{
    /// <summary>
    /// Main window: the console (command typed in line, after the prompt), the ribbon and the
    /// status bar. All the logic lives in the commands (QRZConsole.Core); ribbon buttons run the
    /// equivalent command, echoed in the console so that the commands can be learned by using the ribbon.
    /// </summary>
    public partial class frmMain : Form, IConsoleHost
    {
        private readonly RegistrySettingsStore _store;
        private readonly AppSettings _settings;
        private readonly SessionState _session;
        private readonly ILogbookService _logbook;
        private readonly DxClusterService _cluster;
        private readonly CommandProcessor _processor;
        private ConsoleTheme _theme;
        private string? _runningCommand;

        public frmMain()
        {
            InitializeComponent();
            ClientSize = new System.Drawing.Size(LogicalToDeviceUnits(1100), LogicalToDeviceUnits(720));
            MinimumSize = new System.Drawing.Size(LogicalToDeviceUnits(800), LogicalToDeviceUnits(500));

            _store = new RegistrySettingsStore(Application.ProductName ?? "QRZ Console");
            _settings = new AppSettings(_store);
            _session = new SessionState();
            _logbook = new Logbook();
            _cluster = new DxClusterService(this); // created on the UI thread: cluster output arrives on the UI thread
            _processor = new CommandProcessor(new CommandContext(this, _logbook, _session, _settings, _cluster, CommandCatalog.CreateDefault()));

            _theme = ConsoleTheme.Load(_store);
            ApplyThemeToWindow(_theme);

            BuildRibbon();

            // Timeouts and script errors from the scraper (every step after "bd").
            _logbook.Diagnostic += message => WriteLine($"  [browser] {message}");

            _session.PropertyChanged += Session_PropertyChanged;
            console.CommandSubmitted += (s, line) => RunCommand(line);
            console.InputChanged += Console_InputChanged;
            ribbon.CollapsedChanged += (s, e) => UpdateStatus();
            statusTimer.Tick += (s, e) => UpdateStatus();
            Shown += (s, e) => console.FocusInput();
        }

        #region IConsoleHost

        public void WriteLine(string text = "")
        {
            if (InvokeRequired) { BeginInvoke(new Action<string>(WriteLine), text); return; }
            console.WriteLine(text);
        }

        public void Write(string text)
        {
            if (InvokeRequired) { BeginInvoke(new Action<string>(Write), text); return; }
            console.Write(text, ConsoleTextKind.Normal);
        }

        public void Clear() => console.ClearOutput();

        public void ToggleHeader()
        {
            ribbon.Collapsed = !ribbon.Collapsed;
            WriteLine(ribbon.Collapsed ? "Ribbon collapsed (sw or CTRL+F1 to show it again)" : "Ribbon shown");
        }

        public void ToggleFullScreen()
        {
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            WriteLine(WindowState == FormWindowState.Maximized ? "Console switched to full screen" : "Console switched to normal screen");
        }

        public void ToggleCheckIsLoggedAtStartup()
        {
            _settings.CheckIsLoggedAtStartup = !_settings.CheckIsLoggedAtStartup;
            WriteLine($"Check Is Logged at startup = {(_settings.CheckIsLoggedAtStartup ? "enabled" : "disabled")}");
            UpdateStatus();
        }

        public void Quit() => Close();

        public IReadOnlyList<string> ThemeNames => ConsoleTheme.Presets.Select(p => p.Name).ToList();

        public bool ApplyTheme(string name)
        {
            ConsoleTheme? preset = ConsoleTheme.FindPreset(name);
            if (preset == null) return false;

            // A preset changes the colors; the font chosen by the user is kept.
            SetTheme(preset with { FontName = _theme.FontName, FontSize = _theme.FontSize }, save: true);
            return true;
        }

        public void ShowThemeEditor()
        {
            ConsoleTheme original = _theme;
            using var dialog = new ThemeDialog(_theme, preview => SetTheme(preview, save: false));
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                SetTheme(dialog.Theme, save: true);
                WriteLine($"Theme set: {dialog.Theme.Name} - {dialog.Theme.FontName} {dialog.Theme.FontSize:0.##}pt");
            }
            else
            {
                SetTheme(original, save: false);
            }
            console.FocusInput();
        }

        #endregion

        #region Theme and zoom

        private void SetTheme(ConsoleTheme theme, bool save)
        {
            _theme = theme;
            ApplyThemeToWindow(theme);
            if (save)
                theme.Save(_store);
        }

        private void ApplyThemeToWindow(ConsoleTheme theme)
        {
            consolePanel.BackColor = theme.Background;
            console.ApplyTheme(theme);
        }

        private void Zoom(float delta) => SetTheme(_theme.WithFontSize(_theme.FontSize + delta), save: true);

        private void ZoomReset()
        {
            float size = ConsoleTheme.FindPreset(_theme.Name)?.FontSize ?? ConsoleTheme.Classic.FontSize;
            SetTheme(_theme.WithFontSize(size), save: true);
        }

        #endregion

        #region Startup / shutdown

        private async void frmMain_Load(object? sender, EventArgs e)
        {
            Text = $"{Application.ProductName} v.{Application.ProductVersion}"; // e.g. "QRZ Console v.2.0.0-beta"
            LoadSessionFromSettings();
            ribbon.Collapsed = !_settings.ShowHeader;
            UpdateStatus();
            console.FocusInput();

            string stars = new string('*', 100);
            WriteLine(stars);
            WriteLine($"Welcome to {Text}");
            WriteLine();
            WriteLine("QRZ Console is a \"unofficial command-line interface\" to the QRZ.COM website and require a personal account.");
            WriteLine("For more information about the website, go to: https://www.qrz.com");
            WriteLine();
            WriteLine("Type a command after the prompt and press ENTER (\"cm\" for the command list, \"sl\" for the shortcuts).");
            WriteLine($"Current Logbook View Mode is: {_session.ViewMode.ToKeyword()}");
            WriteLine(stars);
            WriteLine();

            statusBusy.Text = "Starting the browser...";
            try
            {
                await _logbook.InitializeAsync();
            }
            catch (Exception ex)
            {
                WriteLine($"#Error: unable to start the browser (WebView2): {ex.Message}");
                return;
            }

            statusTimer.Start();
            RunCommand(_settings.CheckIsLoggedAtStartup ? "il" : "ls");
        }

        private void LoadSessionFromSettings()
        {
            _session.Username = _settings.Username;
            _session.Password = _settings.Password;
            _session.CurrentQrz = _settings.LastLookup;

            string lastFreq = _settings.LastFrequency;
            if (lastFreq.Length > 0)
                _session.LastFrequency = lastFreq.Replace(".", "");

            if (ViewModeExtensions.TryParse(_settings.ViewMode, out ViewMode mode))
                _session.ViewMode = mode;

            _session.Locators = _settings.GetLocators(_session.Username);

            _logbook.Qrz = _session.Username;
            _logbook.Password = _session.Password;

            SyncFieldsFromSession();
        }

        private void frmMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            statusTimer.Stop();
            _cluster.Dispose();
        }

        #endregion

        #region Running commands

        /// <summary>Runs a command typed in the console (already visible after the prompt).</summary>
        private async void RunCommand(string commandLine)
        {
            if (commandLine.Trim().Length == 0)
                return;

            _runningCommand = commandLine.StartsWith("li ", StringComparison.OrdinalIgnoreCase) ? "li" : commandLine;
            UpdateStatus();
            try
            {
                await _processor.ExecuteAsync(commandLine, echo: false);
            }
            catch (Exception ex)
            {
                WriteLine($"#Error: {ex.Message}");
            }
            finally
            {
                _runningCommand = null;
                if (!IsDisposed)
                    UpdateStatus();
            }
        }

        /// <summary>Runs a command from the ribbon, showing it in the console as if it had been typed.</summary>
        private void RunFromRibbon(string commandLine)
        {
            console.EchoCommand(commandLine);
            RunCommand(commandLine);
            console.FocusInput();
        }

        /// <summary>Puts a command in the command line (pre-filled where possible) for the user to complete.</summary>
        private void PrepareCommand(string prefix)
        {
            console.FocusInput();
            console.SetInput(prefix);
            AutoFill();
        }

        private void Console_InputChanged(object? sender, EventArgs e) => AutoFill();

        /// <summary>Typing "aq " or "ss " pre-fills the line with the last call, frequency, mode and UTC date/time.</summary>
        private void AutoFill()
        {
            if (console.InputCaret != 3)
                return;

            string input = console.InputText;
            if (input.Length != 3)
                return;

            string call = _session.CurrentQrz.Length > 0 ? _session.CurrentQrz : "[CALL]";
            string prefix = input.ToLowerInvariant();

            if (prefix == "aq ")
                console.SetInput($"{input}{call} {_session.LastFrequency} {_session.LastMode} {DateTime.UtcNow:yyyy-MM-dd HH:mm} ", 3, call.Length);
            else if (prefix == "ss ")
                console.SetInput($"{input}{call} {_session.LastFrequency} ", 3, call.Length);
        }

        #endregion

        #region Session <-> fields, status bar

        private void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SyncFieldsFromSession();
            UpdateStatus();
        }

        private void SyncFieldsFromSession()
        {
            SetIfDifferent(txtUsername, _session.Username);
            SetIfDifferent(txtPassword, _session.Password);
            SetIfDifferent(txtCallsign, _session.CurrentQrz);
            SetIfDifferent(txtPage, _session.CurrentPage.ToString());
        }

        private static void SetIfDifferent(TextBox box, string value)
        {
            if (box.Text != value) box.Text = value;
        }

        private void UpdateStatus()
        {
            if (IsDisposed) return;

            bool logged = _settings.IsLoggedIn;
            statusLogin.Text = logged && _session.Username.Length > 0 ? $"● {_session.Username} logged in" : "○ Not logged in";
            statusLogin.ForeColor = logged ? System.Drawing.Color.FromArgb(16, 124, 16) : System.Drawing.SystemColors.GrayText;
            statusQrz.Text = $"QRZ: {(_session.CurrentQrz.Length > 0 ? _session.CurrentQrz : "-")}";
            statusPage.Text = $"Page {_session.CurrentPage}";
            statusView.Text = $"View: {_session.ViewMode.ToKeyword()}";
            statusCluster.Text = "Cluster: " + (_cluster.IsReady ? "connected"
                                 : _cluster.State == WinsockState.Connecting || _cluster.State == WinsockState.Connected ? "connecting..."
                                 : "off");
            statusBusy.Text = _runningCommand != null ? $"Running: {_runningCommand}" : "Ready";

            UpdateRibbonState();
        }

        #endregion

        #region Keyboard shortcuts

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F2:
                    console.FocusInput();
                    return true;
                case Keys.Control | Keys.F1:
                    ribbon.Collapsed = !ribbon.Collapsed;
                    return true;
                case Keys.Control | Keys.Add:
                case Keys.Control | Keys.Oemplus:
                case Keys.Control | Keys.Shift | Keys.Oemplus:
                    Zoom(+1);
                    return true;
                case Keys.Control | Keys.Subtract:
                case Keys.Control | Keys.OemMinus:
                    Zoom(-1);
                    return true;
                case Keys.Control | Keys.D0:
                case Keys.Control | Keys.NumPad0:
                    ZoomReset();
                    return true;
                case Keys.Control | Keys.PageUp:
                    RunFromRibbon("pu");
                    return true;
                case Keys.Control | Keys.PageDown:
                    RunFromRibbon("pd");
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion
    }
}
