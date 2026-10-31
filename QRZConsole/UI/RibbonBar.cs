#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace QRZConsole.UI
{
    /// <summary>Colors and fonts shared by the ribbon controls (light, Office-like).</summary>
    internal static class RibbonStyle
    {
        public static readonly Color Background = Color.FromArgb(243, 243, 243);
        public static readonly Color Body = Color.FromArgb(250, 250, 250);
        public static readonly Color Border = Color.FromArgb(218, 218, 218);
        public static readonly Color Text = Color.FromArgb(38, 38, 38);
        public static readonly Color TextDisabled = Color.FromArgb(160, 160, 160);
        public static readonly Color Caption = Color.FromArgb(100, 100, 100);
        public static readonly Color Accent = Color.FromArgb(43, 87, 154);
        public static readonly Color Hover = Color.FromArgb(229, 241, 251);
        public static readonly Color HoverBorder = Color.FromArgb(204, 228, 247);
        public static readonly Color Pressed = Color.FromArgb(204, 228, 247);
        public static readonly Color Checked = Color.FromArgb(214, 232, 248);
        public static readonly Color CheckedBorder = Color.FromArgb(150, 196, 236);

        public static readonly Font TextFont = new Font("Segoe UI", 9f);
        public static readonly Font TabFont = new Font("Segoe UI", 9.5f);
        public static readonly Font CaptionFont = new Font("Segoe UI", 8f);

        private static string? _glyphFamily;

        /// <summary>Icon font: Segoe Fluent Icons (Windows 11) or Segoe MDL2 Assets (Windows 10).</summary>
        public static string? GlyphFamily
        {
            get
            {
                if (_glyphFamily == null)
                {
                    using var fonts = new InstalledFontCollection();
                    string[] names = fonts.Families.Select(f => f.Name).ToArray();
                    _glyphFamily = names.Contains("Segoe Fluent Icons") ? "Segoe Fluent Icons"
                                 : names.Contains("Segoe MDL2 Assets") ? "Segoe MDL2 Assets"
                                 : string.Empty;
                }
                return _glyphFamily.Length > 0 ? _glyphFamily : null;
            }
        }
    }

    /// <summary>Icon code points of the Segoe MDL2 / Fluent icon fonts.</summary>
    internal static class Glyphs
    {
        public const char Contact = '';
        public const char Power = '';
        public const char Accept = '';
        public const char Search = '';
        public const char History = '';
        public const char List = '';
        public const char Library = '';
        public const char Back = '';
        public const char Forward = '';
        public const char Page = '';
        public const char Document = '';
        public const char View = '';
        public const char Sort = '';
        public const char Info = '';
        public const char Globe = '';
        public const char Link = '';
        public const char Cancel = '';
        public const char Filter = '';
        public const char Send = '';
        public const char Calendar = '';
        public const char Delete = '';
        public const char Keyboard = '';
        public const char Help = '';
        public const char Color = '';
        public const char ZoomIn = '';
        public const char ZoomOut = '';
        public const char Refresh = '';
        public const char FullScreen = '';
        public const char CheckMark = '';
        public const char Diagnostic = '';
        public const char Home = '';
        public const char ChevronUp = '';
        public const char ChevronDown = '';
        public const char Settings = '';
    }

    /// <summary>A tab of the ribbon: a name and its groups.</summary>
    internal sealed class RibbonTab
    {
        public RibbonTab(string text) => Text = text;

        public string Text { get; }

        public List<RibbonGroup> Groups { get; } = new();

        public RibbonGroup AddGroup(string caption)
        {
            var group = new RibbonGroup(caption);
            Groups.Add(group);
            return group;
        }
    }

    /// <summary>
    /// Office-like ribbon: a strip of tabs and, below it, the groups of the selected tab.
    /// Double-click a tab (or the chevron on the right) to collapse/expand it.
    /// </summary>
    internal sealed class RibbonBar : Control
    {
        private readonly FlowLayoutPanel _body;
        private readonly List<RibbonTab> _tabs = new();
        private readonly List<Rectangle> _tabBounds = new();
        private RibbonTab? _selected;
        private int _hoverTab = -1;
        private bool _hoverChevron;
        private bool _collapsed;

        public RibbonBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = RibbonStyle.Background;
            Font = RibbonStyle.TabFont;
            Dock = DockStyle.Top;

            _body = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                BackColor = RibbonStyle.Body,
                Padding = new Padding(4, 3, 4, 2),
                Margin = Padding.Empty
            };
            Controls.Add(_body);
            UpdateLayoutHeight();
        }

        /// <summary>Raised when the ribbon is collapsed or expanded.</summary>
        public event EventHandler? CollapsedChanged;

        private int StripHeight => TextRenderer.MeasureText("Ag", RibbonStyle.TabFont).Height + 12;

        private int BodyHeight => RibbonGroup.ContentHeight + RibbonGroup.CaptionHeight + 10;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool Collapsed
        {
            get => _collapsed;
            set
            {
                if (_collapsed == value) return;
                _collapsed = value;
                UpdateLayoutHeight();
                Invalidate();
                CollapsedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public RibbonTab AddTab(string text)
        {
            var tab = new RibbonTab(text);
            _tabs.Add(tab);
            if (_selected == null)
                SelectTab(tab);
            Invalidate();
            return tab;
        }

        public void SelectTab(RibbonTab tab)
        {
            _selected = tab;
            _body.SuspendLayout();
            _body.Controls.Clear();
            foreach (RibbonGroup group in tab.Groups)
                _body.Controls.Add(group);
            _body.ResumeLayout();
            Invalidate();
        }

        private void UpdateLayoutHeight()
        {
            int strip = StripHeight;
            _body.Visible = !_collapsed;
            _body.SetBounds(0, strip, Width, BodyHeight);
            Height = strip + (_collapsed ? 0 : BodyHeight) + 1;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _body.Width = Width;
        }

        private Rectangle ChevronBounds => new Rectangle(Width - StripHeight - 4, 0, StripHeight, StripHeight);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int strip = StripHeight;

            using (var border = new Pen(RibbonStyle.Border))
            {
                g.DrawLine(border, 0, strip - 1, Width, strip - 1);
                g.DrawLine(border, 0, Height - 1, Width, Height - 1);
            }

            _tabBounds.Clear();
            int x = 8;
            for (int i = 0; i < _tabs.Count; i++)
            {
                RibbonTab tab = _tabs[i];
                Size size = TextRenderer.MeasureText(tab.Text, RibbonStyle.TabFont);
                var bounds = new Rectangle(x, 0, size.Width + 20, strip - 1);
                _tabBounds.Add(bounds);

                bool selected = tab == _selected && !_collapsed;
                if (i == _hoverTab && !selected)
                {
                    using var hover = new SolidBrush(RibbonStyle.Hover);
                    g.FillRectangle(hover, bounds);
                }

                Color color = tab == _selected ? RibbonStyle.Accent : RibbonStyle.Text;
                TextRenderer.DrawText(g, tab.Text, RibbonStyle.TabFont, bounds, color,
                                      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                if (tab == _selected)
                {
                    using var accent = new SolidBrush(RibbonStyle.Accent);
                    g.FillRectangle(accent, bounds.X + 8, bounds.Bottom - 3, bounds.Width - 16, 3);
                }

                x = bounds.Right + 2;
            }

            // Collapse / expand chevron.
            Rectangle chevron = ChevronBounds;
            if (_hoverChevron)
            {
                using var hover = new SolidBrush(RibbonStyle.Hover);
                g.FillRectangle(hover, chevron);
            }
            RibbonButton.DrawGlyph(g, _collapsed ? Glyphs.ChevronDown : Glyphs.ChevronUp, _collapsed ? "v" : "^",
                                   chevron, 8f, RibbonStyle.Caption);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int hover = _tabBounds.FindIndex(r => r.Contains(e.Location));
            bool chevron = ChevronBounds.Contains(e.Location);
            if (hover != _hoverTab || chevron != _hoverChevron)
            {
                _hoverTab = hover;
                _hoverChevron = chevron;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverTab = -1;
            _hoverChevron = false;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (ChevronBounds.Contains(e.Location))
            {
                Collapsed = !Collapsed;
                return;
            }

            int index = _tabBounds.FindIndex(r => r.Contains(e.Location));
            if (index < 0) return;

            SelectTab(_tabs[index]);
            if (_collapsed)
                Collapsed = false;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (_tabBounds.Any(r => r.Contains(e.Location)))
                Collapsed = !Collapsed;
        }
    }

    /// <summary>
    /// A group of commands with its caption at the bottom. Large buttons take a full column;
    /// small buttons and fields stack three per column. Positions are computed explicitly.
    /// </summary>
    internal sealed class RibbonGroup : Panel
    {
        public const int Rows = 3;
        private const int ColumnGap = 2;
        public static readonly int RowHeight = TextRenderer.MeasureText("Ag", RibbonStyle.TextFont).Height + 9;
        public static int ContentHeight => RowHeight * Rows;
        public static readonly int CaptionHeight = TextRenderer.MeasureText("Ag", RibbonStyle.CaptionFont).Height + 4;

        private readonly List<(Control Control, bool Large)> _items = new();
        private readonly string _caption;
        private readonly ToolTip _toolTip = new() { InitialDelay = 400 };

        public RibbonGroup(string caption)
        {
            _caption = caption;
            DoubleBuffered = true;
            BackColor = RibbonStyle.Body;
            Margin = new Padding(0, 0, 2, 0);
            Arrange();
        }

        public RibbonButton AddLarge(char glyph, string text, string tooltip, Action onClick)
            => Add(new RibbonButton(glyph, text, large: true), tooltip, onClick, large: true);

        public RibbonButton AddSmall(char glyph, string text, string tooltip, Action onClick)
            => Add(new RibbonButton(glyph, text, large: false), tooltip, onClick, large: false);

        public RibbonButton AddToggle(char glyph, string text, string tooltip, Action onClick)
            => Add(new RibbonButton(glyph, text, large: false) { IsToggle = true }, tooltip, onClick, large: false);

        /// <summary>A caption and a text box on one row.</summary>
        public TextBox AddField(string label, int width, string tooltip, bool password = false)
        {
            var caption = new Label
            {
                Text = label,
                Font = RibbonStyle.TextFont,
                ForeColor = RibbonStyle.Text,
                BackColor = RibbonStyle.Body,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(TextRenderer.MeasureText(label, RibbonStyle.TextFont).Width + 4, RowHeight),
                Location = new Point(2, 0)
            };

            var box = new TextBox
            {
                Font = RibbonStyle.TextFont,
                Width = width,
                UseSystemPasswordChar = password,
                BorderStyle = BorderStyle.FixedSingle
            };
            box.Location = new Point(caption.Right + 2, Math.Max(0, (RowHeight - box.Height) / 2));

            var row = new Panel { BackColor = RibbonStyle.Body, Size = new Size(box.Right + 4, RowHeight) };
            row.Controls.Add(caption);
            row.Controls.Add(box);

            _items.Add((row, false));
            Controls.Add(row);
            _toolTip.SetToolTip(box, tooltip);
            _toolTip.SetToolTip(caption, tooltip);
            Arrange();
            return box;
        }

        private RibbonButton Add(RibbonButton button, string tooltip, Action onClick, bool large)
        {
            button.Click += (s, e) => onClick();
            _items.Add((button, large));
            Controls.Add(button);
            _toolTip.SetToolTip(button, tooltip);
            Arrange();
            return button;
        }

        /// <summary>Places the items in columns and sizes the group.</summary>
        private void Arrange()
        {
            int x = 4, row = 0, columnWidth = 0;
            foreach ((Control control, bool large) in _items)
            {
                if (large)
                {
                    if (row > 0) { x += columnWidth + ColumnGap; row = 0; columnWidth = 0; }
                    control.Location = new Point(x, 0);
                    x += control.Width + ColumnGap;
                    continue;
                }

                control.Location = new Point(x, row * RowHeight);
                columnWidth = Math.Max(columnWidth, control.Width);
                if (++row == Rows) { x += columnWidth + ColumnGap; row = 0; columnWidth = 0; }
            }
            if (row > 0) x += columnWidth + ColumnGap;

            int captionWidth = TextRenderer.MeasureText(_caption, RibbonStyle.CaptionFont).Width + 16;
            Size = new Size(Math.Max(x + 6, captionWidth), ContentHeight + CaptionHeight + 2);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var captionBounds = new Rectangle(0, Height - CaptionHeight - 1, Width - 3, CaptionHeight);
            TextRenderer.DrawText(e.Graphics, _caption, RibbonStyle.CaptionFont, captionBounds, RibbonStyle.Caption,
                                  TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            using var border = new Pen(RibbonStyle.Border);
            e.Graphics.DrawLine(border, Width - 1, 4, Width - 1, Height - 6);
        }
    }

    /// <summary>A flat ribbon button with an icon glyph: large (icon above text) or small (icon beside text).</summary>
    internal sealed class RibbonButton : Control
    {
        private readonly char _glyph;
        private readonly bool _large;
        private bool _hover;
        private bool _pressed;
        private bool _checked;

        public RibbonButton(char glyph, string text, bool large)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
                     | ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false); // keep the focus in the console
            _glyph = glyph;
            _large = large;
            Text = text;
            Font = RibbonStyle.TextFont;
            BackColor = RibbonStyle.Body;
            Cursor = Cursors.Hand;
            Margin = new Padding(1, 0, 1, 0);

            Size textSize = TextRenderer.MeasureText(text, RibbonStyle.TextFont);
            if (large)
            {
                int width = Math.Max(52, MeasureLargeTextWidth(text) + 12);
                Size = new Size(width, RibbonGroup.ContentHeight);
            }
            else
            {
                Size = new Size(RibbonGroup.RowHeight + textSize.Width + 6, RibbonGroup.RowHeight);
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool IsToggle { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool Checked
        {
            get => _checked;
            set { _checked = value; Invalidate(); }
        }

        /// <summary>Large buttons wrap their text on two lines at the first space.</summary>
        private static int MeasureLargeTextWidth(string text)
        {
            return SplitLarge(text).Max(part => TextRenderer.MeasureText(part, RibbonStyle.TextFont).Width);
        }

        private static string[] SplitLarge(string text)
        {
            int space = text.IndexOf(' ');
            return text.Length > 10 && space > 0
                ? new[] { text.Substring(0, space), text.Substring(space + 1) }
                : new[] { text };
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = false; _pressed = false; Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if (e.Button == MouseButtons.Left) { _pressed = true; Invalidate(); } }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _pressed = false; Invalidate(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = new Rectangle(0, 1, Width - 1, Height - 3);

            Color? fill = !Enabled ? null : _pressed ? RibbonStyle.Pressed : _checked ? RibbonStyle.Checked : _hover ? RibbonStyle.Hover : null;
            Color? border = !Enabled ? null : _checked ? RibbonStyle.CheckedBorder : _hover ? RibbonStyle.HoverBorder : null;
            if (fill.HasValue)
            {
                using var brush = new SolidBrush(fill.Value);
                g.FillRectangle(brush, bounds);
            }
            if (border.HasValue)
            {
                using var pen = new Pen(border.Value);
                g.DrawRectangle(pen, bounds);
            }

            Color textColor = Enabled ? RibbonStyle.Text : RibbonStyle.TextDisabled;
            Color glyphColor = Enabled ? RibbonStyle.Accent : RibbonStyle.TextDisabled;

            if (_large)
            {
                int glyphBox = RibbonGroup.RowHeight + 6;
                DrawGlyph(g, _glyph, Text.Substring(0, 1), new Rectangle(0, 4, Width, glyphBox), 18f, glyphColor);
                var textBounds = new Rectangle(2, glyphBox + 4, Width - 4, Height - glyphBox - 6);
                TextRenderer.DrawText(g, string.Join("\n", SplitLarge(Text)), Font, textBounds, textColor,
                                      TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
            }
            else
            {
                int box = RibbonGroup.RowHeight;
                DrawGlyph(g, _glyph, Text.Substring(0, 1), new Rectangle(2, 0, box - 2, Height), 10.5f, glyphColor);
                TextRenderer.DrawText(g, Text, Font, new Rectangle(box, 0, Width - box, Height), textColor,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
        }

        /// <summary>Draws an icon glyph centered in the bounds (or a fallback text without the icon font).</summary>
        public static void DrawGlyph(Graphics g, char glyph, string fallback, Rectangle bounds, float size, Color color)
        {
            string? family = RibbonStyle.GlyphFamily;
            using var font = family != null ? new Font(family, size) : new Font("Segoe UI", size * 0.8f, FontStyle.Bold);
            TextRenderer.DrawText(g, family != null ? glyph.ToString() : fallback, font, bounds, color,
                                  TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
