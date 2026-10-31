#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QRZConsole.UI
{
    /// <summary>How a piece of output is colored.</summary>
    internal enum ConsoleTextKind
    {
        Normal,
        /// <summary>Prompt, typed commands and command echoes.</summary>
        Accent,
        Error,
        /// <summary>Secondary information (browser diagnostics).</summary>
        Dim
    }

    /// <summary>
    /// A terminal-like console: output on top, and the command typed "in line" after the prompt
    /// on the last line. Everything before the prompt is read-only (it can be selected and copied);
    /// output that arrives while the user is typing is inserted above the prompt line.
    /// </summary>
    /// <remarks>
    /// Keys: Enter runs the line, Up/Down browse the history, Esc clears the line,
    /// Tab / Shift+Tab select the next / previous parameter, Home goes to the start of the command.
    /// Writes are buffered and flushed once per UI cycle, so long outputs stay fast.
    /// </remarks>
    internal sealed class ConsoleView : RichTextBox
    {
        private readonly List<(string Text, ConsoleTextKind Kind)> _pending = new();
        private readonly List<string> _history = new();
        private int _historyIndex;
        private int _promptStart;
        private bool _flushScheduled;
        private bool _internalChange;
        private ConsoleTheme _theme = ConsoleTheme.Classic;

        public ConsoleView()
        {
            Multiline = true;
            WordWrap = false;
            ScrollBars = RichTextBoxScrollBars.Both;
            DetectUrls = false;
            BorderStyle = BorderStyle.None;
            AcceptsTab = true;
            HideSelection = false;
            EnableAutoDragDrop = false;
            AllowDrop = false;
            ShortcutsEnabled = false; // copy/paste/select-all are handled here, formatting shortcuts are blocked

            ContextMenuStrip = BuildContextMenu();
            ApplyTheme(_theme);
        }

        /// <summary>A command line was entered (Enter key). The text does not include the prompt.</summary>
        public event EventHandler<string>? CommandSubmitted;

        /// <summary>The user changed the command being typed.</summary>
        public event EventHandler? InputChanged;

        public string Prompt { get; } = "QRZ> ";

        public ConsoleTheme Theme => _theme;

        private int InputStart => _promptStart + Prompt.Length;

        /// <summary>The command currently typed after the prompt.</summary>
        public string InputText => GetTextRange(InputStart, TextLength - InputStart);

        /// <summary>Caret position inside the command (0 = right after the prompt), -1 when outside.</summary>
        public int InputCaret => SelectionStart >= InputStart ? SelectionStart - InputStart : -1;

        #region Output

        /// <summary>Writes a line; the color is chosen from its content (see <see cref="Classify"/>).</summary>
        public void WriteLine(string text) => Write(text + "\n", Classify(text));

        public void Write(string text, ConsoleTextKind kind)
        {
            string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
            if (normalized.Length == 0) return;

            _pending.Add((normalized, kind));
            ScheduleFlush();
        }

        /// <summary>Shows a command run from the ribbon as if it had been typed.</summary>
        public void EchoCommand(string commandLine) => Write(Prompt + commandLine + "\n", ConsoleTextKind.Accent);

        /// <summary>Clears all the output, keeping the command being typed.</summary>
        public void ClearOutput()
        {
            _pending.Clear();
            string input = InputText;

            _internalChange = true;
            Clear();
            _promptStart = 0;
            AppendPrompt();
            _internalChange = false;

            SetInput(input);
        }

        /// <summary>Color category of an output line.</summary>
        public static ConsoleTextKind Classify(string line)
        {
            if (line.StartsWith("#Error", StringComparison.Ordinal) || line.StartsWith("#Warning", StringComparison.Ordinal))
                return ConsoleTextKind.Error;
            if (line.StartsWith("  [browser]", StringComparison.Ordinal))
                return ConsoleTextKind.Dim;
            if (line.StartsWith(">", StringComparison.Ordinal))
                return ConsoleTextKind.Accent;
            return ConsoleTextKind.Normal;
        }

        private void ScheduleFlush()
        {
            if (_flushScheduled || !IsHandleCreated) return;
            _flushScheduled = true;
            BeginInvoke(new Action(Flush));
        }

        /// <summary>Inserts the buffered output above the prompt line, keeping the user's caret and selection.</summary>
        private void Flush()
        {
            _flushScheduled = false;
            if (_pending.Count == 0 || IsDisposed) return;

            int selStart = SelectionStart;
            int selLength = SelectionLength;
            bool inCommandLine = selStart >= _promptStart;
            int offset = selStart - _promptStart;

            SuspendPainting();
            _internalChange = true;
            try
            {
                int pos = _promptStart;
                foreach ((string text, ConsoleTextKind kind) in MergePending())
                {
                    Select(pos, 0);
                    SelectionColor = ColorOf(kind);
                    SelectedText = text;
                    pos += text.Length;
                }
                _promptStart = pos;

                Select(inCommandLine ? _promptStart + offset : selStart, selLength);
            }
            finally
            {
                _internalChange = false;
                ResumePainting();
            }

            if (inCommandLine)
                ScrollToCaret();
        }

        private IEnumerable<(string, ConsoleTextKind)> MergePending()
        {
            var merged = new List<(string, ConsoleTextKind)>();
            var current = new System.Text.StringBuilder();
            ConsoleTextKind currentKind = _pending[0].Kind;

            foreach ((string text, ConsoleTextKind kind) in _pending)
            {
                if (kind != currentKind && current.Length > 0)
                {
                    merged.Add((current.ToString(), currentKind));
                    current.Clear();
                }
                currentKind = kind;
                current.Append(text);
            }
            if (current.Length > 0)
                merged.Add((current.ToString(), currentKind));

            _pending.Clear();
            return merged;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (TextLength == 0)
            {
                _internalChange = true;
                AppendPrompt();
                _internalChange = false;
            }
            if (_pending.Count > 0)
                ScheduleFlush();
        }

        private void AppendPrompt()
        {
            Select(TextLength, 0);
            SelectionColor = _theme.Accent;
            SelectedText = Prompt;
            Select(TextLength, 0);
            SelectionColor = _theme.Accent;
        }

        #endregion

        #region Command line

        /// <summary>Replaces the command being typed and optionally selects part of it.</summary>
        public void SetInput(string text, int selectionStart = -1, int selectionLength = 0)
        {
            Flush();
            _internalChange = true;
            try
            {
                Select(InputStart, TextLength - InputStart);
                SelectionColor = _theme.Accent;
                SelectedText = text.Replace("\r", "").Replace("\n", " ");
                int start = selectionStart < 0 ? TextLength : InputStart + Math.Min(selectionStart, TextLength - InputStart);
                Select(start, Math.Max(0, Math.Min(selectionLength, TextLength - start)));
            }
            finally
            {
                _internalChange = false;
            }
            ScrollToCaret();
        }

        /// <summary>Puts the caret at the end of the command line.</summary>
        public void FocusInput()
        {
            Focus();
            Select(TextLength, 0);
            ScrollToCaret();
        }

        private void Submit()
        {
            Flush();
            string command = InputText;

            _internalChange = true;
            try
            {
                Select(TextLength, 0);
                SelectedText = "\n";
                _promptStart = TextLength;
                AppendPrompt();
            }
            finally
            {
                _internalChange = false;
            }
            ScrollToCaret();

            if (command.Trim().Length > 0 && (_history.Count == 0 || _history[^1] != command))
                _history.Add(command);
            _historyIndex = _history.Count;

            CommandSubmitted?.Invoke(this, command);
        }

        private void HistoryMove(int delta)
        {
            if (_history.Count == 0) return;

            _historyIndex = Math.Clamp(_historyIndex + delta, 0, _history.Count);
            SetInput(_historyIndex < _history.Count ? _history[_historyIndex] : string.Empty);
        }

        /// <summary>Tab: selects the next parameter. Shift+Tab: the previous one.</summary>
        private void SelectParameter(bool forward)
        {
            string input = InputText;
            var tokens = new List<(int Start, int End)>();
            for (int i = 0; i < input.Length;)
            {
                while (i < input.Length && input[i] == ' ') i++;
                int start = i;
                while (i < input.Length && input[i] != ' ') i++;
                if (i > start) tokens.Add((start, i));
            }

            int selStart = Math.Max(0, SelectionStart - InputStart);
            int selEnd = selStart + SelectionLength;

            // Forward: first parameter starting after the selection. Backward: last one ending before it.
            (int Start, int End)? target = null;
            if (forward)
            {
                foreach (var t in tokens)
                {
                    if (t.Start >= selEnd) { target = t; break; }
                }
            }
            else
            {
                for (int k = tokens.Count - 1; k >= 0; k--)
                {
                    if (tokens[k].End <= selStart) { target = tokens[k]; break; }
                }
            }

            if (target.HasValue)
                Select(InputStart + target.Value.Start, target.Value.End - target.Value.Start);
            else if (forward)
                Select(TextLength, 0);
        }

        #endregion

        #region Keyboard and clipboard

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            Keys mods = keyData & Keys.Modifiers;

            switch (keyData)
            {
                case Keys.Enter:
                case Keys.Shift | Keys.Enter:
                    Submit();
                    return true;
                case Keys.Up:
                    HistoryMove(-1);
                    return true;
                case Keys.Down:
                    HistoryMove(+1);
                    return true;
                case Keys.Escape:
                    SetInput(string.Empty);
                    return true;
                case Keys.Tab:
                    SelectParameter(forward: true);
                    return true;
                case Keys.Shift | Keys.Tab:
                    SelectParameter(forward: false);
                    return true;
                case Keys.Home when SelectionStart >= _promptStart:
                    Select(InputStart, 0);
                    return true;
                case Keys.Shift | Keys.Home when SelectionStart >= InputStart:
                    Select(InputStart, SelectionStart + SelectionLength - InputStart);
                    return true;
                case Keys.Left when SelectionLength == 0 && SelectionStart == InputStart:
                    return true;
                case Keys.Back:
                case Keys.Delete:
                    return !PrepareDelete(key == Keys.Back);
                case Keys.Control | Keys.C:
                case Keys.Control | Keys.Insert:
                    if (SelectionLength > 0) Copy();
                    return true;
                case Keys.Control | Keys.X:
                    if (SelectionLength > 0) Copy();
                    if (SelectionStart >= InputStart && SelectionLength > 0) SelectedText = string.Empty;
                    return true;
                case Keys.Control | Keys.V:
                case Keys.Shift | Keys.Insert:
                    PastePlainText();
                    return true;
                case Keys.Control | Keys.A:
                    SelectAll();
                    return true;
            }

            // Let the form handle its own shortcuts (zoom, F2, Ctrl+PgUp/PgDn...).
            if (base.ProcessCmdKey(ref msg, keyData))
                return true;

            // Block the RichTextBox formatting shortcuts (alignment, line spacing, font size...).
            if ((mods & Keys.Control) != 0 && key != Keys.Left && key != Keys.Right && key != Keys.Home && key != Keys.End
                && key != Keys.PageUp && key != Keys.PageDown && key != Keys.ControlKey && key != Keys.ShiftKey)
                return true;

            return false;
        }

        /// <summary>
        /// Limits Backspace/Delete to the command line. Returns true when the key may proceed
        /// (selection adjusted if needed), false to swallow it.
        /// </summary>
        private bool PrepareDelete(bool backspace)
        {
            int start = SelectionStart;
            int end = start + SelectionLength;

            if (end <= InputStart && SelectionLength > 0)
                return false; // selection entirely in the output
            if (SelectionLength == 0)
                return backspace ? start > InputStart : start >= InputStart;

            if (start < InputStart)
                Select(InputStart, end - InputStart);
            return true;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar))
                MoveSelectionIntoCommandLine();
            base.OnKeyPress(e);
        }

        private void MoveSelectionIntoCommandLine()
        {
            int start = SelectionStart;
            int end = start + SelectionLength;
            if (start >= InputStart)
                return;

            if (end > InputStart)
                Select(InputStart, end - InputStart);
            else
                Select(TextLength, 0);
        }

        private void PastePlainText()
        {
            if (!Clipboard.ContainsText()) return;

            string text = Clipboard.GetText();
            int newLine = text.IndexOfAny(new[] { '\r', '\n' });
            if (newLine >= 0) text = text.Substring(0, newLine); // one command at a time

            MoveSelectionIntoCommandLine();
            SelectionColor = _theme.Accent;
            SelectedText = text;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_CUT = 0x0300, WM_PASTE = 0x0302, WM_CLEAR = 0x0303, WM_UNDO = 0x0304;
            switch (m.Msg)
            {
                case WM_PASTE:
                    PastePlainText();
                    return;
                case WM_UNDO:
                    return;
                case WM_CUT:
                case WM_CLEAR:
                    if (SelectionStart < InputStart) return;
                    break;
            }
            base.WndProc(ref m);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (!_internalChange)
                InputChanged?.Invoke(this, EventArgs.Empty);
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            var copy = new ToolStripMenuItem("Copy", null, (s, e) => { if (SelectionLength > 0) Copy(); }) { ShortcutKeyDisplayString = "Ctrl+C" };
            var paste = new ToolStripMenuItem("Paste", null, (s, e) => PastePlainText()) { ShortcutKeyDisplayString = "Ctrl+V" };
            var selectAll = new ToolStripMenuItem("Select all", null, (s, e) => SelectAll()) { ShortcutKeyDisplayString = "Ctrl+A" };
            var clear = new ToolStripMenuItem("Clear console", null, (s, e) => ClearOutput());
            menu.Items.AddRange(new ToolStripItem[] { copy, paste, new ToolStripSeparator(), selectAll, new ToolStripSeparator(), clear });
            menu.Opening += (s, e) =>
            {
                copy.Enabled = SelectionLength > 0;
                paste.Enabled = Clipboard.ContainsText();
            };
            return menu;
        }

        #endregion

        #region Theme

        public void ApplyTheme(ConsoleTheme theme)
        {
            Flush();
            _theme = theme;

            int selStart = SelectionStart;
            int selLength = SelectionLength;

            SuspendPainting();
            _internalChange = true;
            try
            {
                BackColor = theme.Background;
                ForeColor = theme.Text;
                Font = theme.CreateFont();
                RecolorAll();
                Select(selStart, selLength);
                SelectionColor = theme.Accent;
            }
            finally
            {
                _internalChange = false;
                ResumePainting();
            }
        }

        /// <summary>Re-applies the colors to the existing text, classifying each line again.</summary>
        private void RecolorAll()
        {
            string[] lines = Lines;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length == 0) continue;
                int start = GetFirstCharIndexFromLine(i);
                if (start < 0) continue;

                ConsoleTextKind kind = lines[i].StartsWith(Prompt, StringComparison.Ordinal) ? ConsoleTextKind.Accent : Classify(lines[i]);
                Select(start, lines[i].Length);
                SelectionColor = ColorOf(kind);
            }
        }

        private Color ColorOf(ConsoleTextKind kind) => kind switch
        {
            ConsoleTextKind.Accent => _theme.Accent,
            ConsoleTextKind.Error => _theme.Error,
            ConsoleTextKind.Dim => _theme.Dim,
            _ => _theme.Text
        };

        #endregion

        #region Native helpers

        private const int WM_SETREDRAW = 0x000B;
        private const int EM_GETTEXTRANGE = 0x044B;

        [StructLayout(LayoutKind.Sequential)]
        private struct CharRange
        {
            public int Min;
            public int Max;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TextRange
        {
            public CharRange Range;
            public IntPtr Text;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref TextRange lParam);

        /// <summary>Reads part of the text without copying the whole content (Text would).</summary>
        private string GetTextRange(int start, int length)
        {
            if (length <= 0 || !IsHandleCreated) return string.Empty;

            IntPtr buffer = Marshal.AllocHGlobal((length + 1) * 2);
            try
            {
                var range = new TextRange { Range = new CharRange { Min = start, Max = start + length }, Text = buffer };
                int copied = SendMessage(Handle, EM_GETTEXTRANGE, IntPtr.Zero, ref range).ToInt32();
                return Marshal.PtrToStringUni(buffer, copied);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private void SuspendPainting()
        {
            if (IsHandleCreated) SendMessage(Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        }

        private void ResumePainting()
        {
            if (!IsHandleCreated) return;
            SendMessage(Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            Invalidate();
        }

        #endregion
    }
}
