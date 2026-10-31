#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using QRZConsole.Core.Settings;

namespace QRZConsole.UI
{
    /// <summary>Colors and font of the console.</summary>
    internal sealed record ConsoleTheme(
        string Name,
        Color Background,
        Color Text,
        Color Accent,
        Color Error,
        Color Dim,
        string FontName,
        float FontSize)
    {
        public const string CustomName = "Custom";
        public const float MinFontSize = 6f;
        public const float MaxFontSize = 40f;

        /// <summary>The look of the original QRZ Console (default).</summary>
        public static ConsoleTheme Classic { get; } = new ConsoleTheme("Classic",
            Color.FromArgb(64, 64, 64), Color.FromArgb(255, 255, 128), Color.FromArgb(255, 255, 192),
            Color.FromArgb(255, 140, 120), Color.FromArgb(170, 170, 170), "Consolas", 8.25f);

        public static IReadOnlyList<ConsoleTheme> Presets { get; } = new[]
        {
            Classic,
            new ConsoleTheme("Dark", Hex("#1E1E1E"), Hex("#D4D4D4"), Hex("#569CD6"), Hex("#F48771"), Hex("#808080"), "Consolas", 10f),
            new ConsoleTheme("Light", Hex("#FFFFFF"), Hex("#1E1E1E"), Hex("#0451A5"), Hex("#C72E0F"), Hex("#8A8A8A"), "Consolas", 10f),
            new ConsoleTheme("Green phosphor", Hex("#0C0C0C"), Hex("#33FF66"), Hex("#B4FFC8"), Hex("#FF6E6E"), Hex("#1E8C3C"), "Consolas", 10f),
            new ConsoleTheme("Amber", Hex("#1A1200"), Hex("#FFB000"), Hex("#FFD580"), Hex("#FF6E6E"), Hex("#8A6000"), "Consolas", 10f),
            new ConsoleTheme("Solarized dark", Hex("#002B36"), Hex("#93A1A1"), Hex("#268BD2"), Hex("#DC322F"), Hex("#586E75"), "Consolas", 10f),
        };

        public static ConsoleTheme? FindPreset(string name)
            => Presets.FirstOrDefault(p => string.Equals(p.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));

        public Font CreateFont() => new Font(FontName, FontSize, FontStyle.Regular, GraphicsUnit.Point);

        public ConsoleTheme WithFontSize(float size)
            => this with { FontSize = Math.Clamp(size, MinFontSize, MaxFontSize) };

        #region Persistence (registry, via ISettingsStore)

        public static ConsoleTheme Load(ISettingsStore store)
        {
            ConsoleTheme fallback = FindPreset(store.Get("ThemeName") ?? "") ?? Classic;
            try
            {
                return new ConsoleTheme(
                    store.Get("ThemeName") ?? fallback.Name,
                    ReadColor(store, "ThemeBackground", fallback.Background),
                    ReadColor(store, "ThemeText", fallback.Text),
                    ReadColor(store, "ThemeAccent", fallback.Accent),
                    ReadColor(store, "ThemeError", fallback.Error),
                    ReadColor(store, "ThemeDim", fallback.Dim),
                    NonEmpty(store.Get("ThemeFont"), fallback.FontName),
                    float.TryParse(store.Get("ThemeFontSize"), NumberStyles.Float, CultureInfo.InvariantCulture, out float size)
                        ? Math.Clamp(size, MinFontSize, MaxFontSize) : fallback.FontSize);
            }
            catch (FormatException)
            {
                return fallback;
            }
        }

        public void Save(ISettingsStore store)
        {
            store.Set("ThemeName", Name);
            store.Set("ThemeBackground", ToHex(Background));
            store.Set("ThemeText", ToHex(Text));
            store.Set("ThemeAccent", ToHex(Accent));
            store.Set("ThemeError", ToHex(Error));
            store.Set("ThemeDim", ToHex(Dim));
            store.Set("ThemeFont", FontName);
            store.Set("ThemeFontSize", FontSize.ToString(CultureInfo.InvariantCulture));
        }

        public static string ToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        public static Color Hex(string hex)
        {
            string h = hex.Trim().TrimStart('#');
            if (h.Length != 6) throw new FormatException($"Invalid color \"{hex}\"");
            return Color.FromArgb(int.Parse(h.Substring(0, 2), NumberStyles.HexNumber),
                                  int.Parse(h.Substring(2, 2), NumberStyles.HexNumber),
                                  int.Parse(h.Substring(4, 2), NumberStyles.HexNumber));
        }

        private static Color ReadColor(ISettingsStore store, string key, Color fallback)
        {
            string? value = store.Get(key);
            return string.IsNullOrWhiteSpace(value) ? fallback : Hex(value);
        }

        private static string NonEmpty(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;

        #endregion
    }
}
