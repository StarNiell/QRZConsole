using System.Collections.Generic;

namespace QRZConsole.Core.Commands
{
    /// <summary>Where command output goes.</summary>
    public interface IConsoleOutput
    {
        /// <summary>Writes text followed by a new line.</summary>
        void WriteLine(string text = "");

        /// <summary>Writes text without a new line.</summary>
        void Write(string text);
    }

    /// <summary>The window hosting the console: output plus the few UI-only actions commands can ask for.</summary>
    public interface IConsoleHost : IConsoleOutput
    {
        void Clear();

        /// <summary>Shows/hides the header panel with buttons.</summary>
        void ToggleHeader();

        void ToggleFullScreen();

        /// <summary>Flips the "check is logged at startup" option (and its check box).</summary>
        void ToggleCheckIsLoggedAtStartup();

        void Quit();

        /// <summary>Names of the predefined console themes.</summary>
        IReadOnlyList<string> ThemeNames { get; }

        /// <summary>Applies and saves a predefined theme. False when the name is unknown.</summary>
        bool ApplyTheme(string name);

        /// <summary>Opens the theme editor (colors and font).</summary>
        void ShowThemeEditor();
    }
}
