#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QRZConsole.UI
{
    /// <summary>
    /// Edits the console theme: preset, five colors, monospace font and size, with a live preview.
    /// "Apply" shows the theme on the console without closing.
    /// </summary>
    internal sealed class ThemeDialog : Form
    {
        private readonly ComboBox _preset = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
        private readonly ComboBox _font = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
        private readonly NumericUpDown _size = new() { Minimum = (decimal)ConsoleTheme.MinFontSize, Maximum = (decimal)ConsoleTheme.MaxFontSize, DecimalPlaces = 2, Increment = 0.5m, Width = 80 };
        private readonly RichTextBox _preview = new() { ReadOnly = true, BorderStyle = BorderStyle.FixedSingle, WordWrap = false, ScrollBars = RichTextBoxScrollBars.None, Height = 150, Dock = DockStyle.Fill, TabStop = false };
        private readonly Dictionary<string, Button> _swatches = new();
        private readonly Action<ConsoleTheme> _apply;
        private ConsoleTheme _theme;
        private bool _loading;

        public ThemeDialog(ConsoleTheme current, Action<ConsoleTheme> apply)
        {
            _theme = current;
            _apply = apply;

            Text = "Console theme";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(12);

            var grid = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Dock = DockStyle.Top };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            AddRow(grid, "Preset", _preset);
            AddRow(grid, "Background", Swatch("Background"));
            AddRow(grid, "Text", Swatch("Text"));
            AddRow(grid, "Commands and prompt", Swatch("Accent"));
            AddRow(grid, "Errors", Swatch("Error"));
            AddRow(grid, "Diagnostics", Swatch("Dim"));
            AddRow(grid, "Font (monospace)", _font);
            AddRow(grid, "Size", _size);

            var previewBox = new GroupBox { Text = "Preview", Dock = DockStyle.Top, Height = 180, Padding = new Padding(8) };
            previewBox.Controls.Add(_preview);

            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 90 };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90 };
            var applyButton = new Button { Text = "Apply", Width = 90 };
            applyButton.Click += (s, e) => _apply(_theme);
            ok.Click += (s, e) => _apply(_theme);
            AcceptButton = ok;
            CancelButton = cancel;

            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 10, 0, 0) };
            buttons.Controls.AddRange(new Control[] { cancel, ok, applyButton });

            var layout = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, Dock = DockStyle.Fill };
            layout.Controls.Add(grid);
            layout.Controls.Add(previewBox);
            layout.Controls.Add(buttons);
            previewBox.Width = 460;
            Controls.Add(layout);

            _preset.Items.AddRange(ConsoleTheme.Presets.Select(p => (object)p.Name).Append(ConsoleTheme.CustomName).ToArray());
            _font.Items.AddRange(MonospaceFonts().Cast<object>().ToArray());

            _preset.SelectedIndexChanged += (s, e) =>
            {
                if (_loading) return;
                ConsoleTheme? preset = ConsoleTheme.FindPreset(_preset.SelectedItem?.ToString() ?? "");
                if (preset != null) LoadTheme(preset with { FontName = _theme.FontName, FontSize = _theme.FontSize });
            };
            _font.SelectedIndexChanged += (s, e) => Change(_theme with { FontName = _font.SelectedItem?.ToString() ?? _theme.FontName });
            _size.ValueChanged += (s, e) => Change(_theme with { FontSize = (float)_size.Value });

            LoadTheme(current);
        }

        /// <summary>The theme chosen (valid after OK).</summary>
        public ConsoleTheme Theme => _theme;

        private static void AddRow(TableLayoutPanel grid, string label, Control control)
        {
            grid.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 6) });
            control.Margin = new Padding(0, 3, 0, 3);
            grid.Controls.Add(control);
        }

        private Button Swatch(string name)
        {
            var button = new Button { Width = 120, Height = 24, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleRight };
            button.Click += (s, e) =>
            {
                using var dialog = new ColorDialog { FullOpen = true, Color = GetColor(name) };
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    Change(SetColor(name, dialog.Color), custom: true);
            };
            _swatches[name] = button;
            return button;
        }

        private Color GetColor(string name) => name switch
        {
            "Background" => _theme.Background,
            "Text" => _theme.Text,
            "Accent" => _theme.Accent,
            "Error" => _theme.Error,
            _ => _theme.Dim
        };

        private ConsoleTheme SetColor(string name, Color color) => name switch
        {
            "Background" => _theme with { Background = color },
            "Text" => _theme with { Text = color },
            "Accent" => _theme with { Accent = color },
            "Error" => _theme with { Error = color },
            _ => _theme with { Dim = color }
        };

        private void Change(ConsoleTheme theme, bool custom = false)
        {
            if (_loading) return;
            _theme = custom ? theme with { Name = ConsoleTheme.CustomName } : theme;
            if (custom)
            {
                _loading = true;
                _preset.SelectedItem = ConsoleTheme.CustomName;
                _loading = false;
            }
            UpdatePreview(_theme);
        }

        private void LoadTheme(ConsoleTheme theme)
        {
            _loading = true;
            _theme = theme;
            _preset.SelectedItem = ConsoleTheme.FindPreset(theme.Name)?.Name ?? ConsoleTheme.CustomName;
            if (!_font.Items.Contains(theme.FontName))
                _font.Items.Insert(0, theme.FontName);
            _font.SelectedItem = theme.FontName;
            _size.Value = (decimal)Math.Clamp(theme.FontSize, ConsoleTheme.MinFontSize, ConsoleTheme.MaxFontSize);
            _loading = false;
            UpdatePreview(theme);
        }

        private void UpdatePreview(ConsoleTheme theme)
        {
            foreach ((string name, Button button) in _swatches)
            {
                Color c = GetColor(name);
                button.BackColor = c;
                button.ForeColor = c.GetBrightness() > 0.5f ? Color.Black : Color.White;
                button.Text = ConsoleTheme.ToHex(c);
            }

            _preview.BackColor = theme.Background;
            _preview.Font = theme.CreateFont();
            _preview.Clear();
            AppendPreview("QRZ> lu IU8NQI\n", theme.Accent);
            AppendPreview("Lookup IU8NQI...\n   Grid Locator = JN70bv\n", theme.Text);
            AppendPreview("  [browser] wait for element #csdata\n", theme.Dim);
            AppendPreview("#Error in \"aq\": invalid frequency\n", theme.Error);
            AppendPreview("QRZ> ", theme.Accent);
        }

        private void AppendPreview(string text, Color color)
        {
            _preview.Select(_preview.TextLength, 0);
            _preview.SelectionColor = color;
            _preview.SelectedText = text;
        }

        /// <summary>Installed font families whose characters all have the same width.</summary>
        public static IReadOnlyList<string> MonospaceFonts()
        {
            var result = new List<string>();
            foreach (FontFamily family in FontFamily.Families)
            {
                if (family.Name.StartsWith("@", StringComparison.Ordinal) || !family.IsStyleAvailable(FontStyle.Regular))
                    continue;
                try
                {
                    using var font = new Font(family, 10f, FontStyle.Regular);
                    int narrow = TextRenderer.MeasureText("iiiiiiiiii", font, Size.Empty, TextFormatFlags.NoPadding).Width;
                    int wide = TextRenderer.MeasureText("WWWWWWWWWW", font, Size.Empty, TextFormatFlags.NoPadding).Width;
                    int digits = TextRenderer.MeasureText("0123456789", font, Size.Empty, TextFormatFlags.NoPadding).Width;
                    if (Math.Abs(narrow - wide) <= 1 && Math.Abs(narrow - digits) <= 1)
                        result.Add(family.Name);
                }
                catch (ArgumentException)
                {
                    // font that GDI+ cannot open
                }
            }
            return result;
        }
    }
}
