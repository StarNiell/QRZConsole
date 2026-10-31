using QRZConsole.Core.Commands;

namespace QRZConsole.Tests.Fakes
{
    /// <summary>Console host that records every line written.</summary>
    internal sealed class RecordingHost : IConsoleHost
    {
        private readonly object _sync = new();
        private readonly List<string> _lines = new();

        public IReadOnlyList<string> Lines { get { lock (_sync) return _lines.ToList(); } }

        public string Text => string.Join("\n", Lines);

        public int ClearCount { get; private set; }
        public int ToggleHeaderCount { get; private set; }
        public bool QuitRequested { get; private set; }

        public void WriteLine(string text = "") { lock (_sync) _lines.Add(text); }
        public void Write(string text) { lock (_sync) _lines.Add(text); }
        public void Clear() => ClearCount++;
        public void ToggleHeader() => ToggleHeaderCount++;
        public void ToggleFullScreen() { }
        public void ToggleCheckIsLoggedAtStartup() { }
        public void Quit() => QuitRequested = true;

        public string? AppliedTheme { get; private set; }
        public int ThemeEditorCount { get; private set; }
        public IReadOnlyList<string> ThemeNames { get; } = new[] { "Classic", "Dark" };
        public bool ApplyTheme(string name)
        {
            if (!ThemeNames.Contains(name, StringComparer.OrdinalIgnoreCase)) return false;
            AppliedTheme = name;
            return true;
        }
        public void ShowThemeEditor() => ThemeEditorCount++;

        public void Reset() { lock (_sync) _lines.Clear(); }
    }
}
