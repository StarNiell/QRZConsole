using System;

namespace QRZConsole.Core.Session
{
    /// <summary>How logbook pages are printed.</summary>
    public enum ViewMode
    {
        Raw,
        Text,
        Adif
    }

    public static class ViewModeExtensions
    {
        public static bool TryParse(string text, out ViewMode mode)
        {
            switch ((text ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "raw": mode = ViewMode.Raw; return true;
                case "text": mode = ViewMode.Text; return true;
                case "adif": mode = ViewMode.Adif; return true;
                default: mode = ViewMode.Raw; return false;
            }
        }

        public static string ToKeyword(this ViewMode mode) => mode switch
        {
            ViewMode.Text => "text",
            ViewMode.Adif => "adif",
            _ => "raw"
        };
    }
}
