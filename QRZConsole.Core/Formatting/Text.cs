namespace QRZConsole.Core.Formatting
{
    public static class Text
    {
        /// <summary>Pads or truncates to exactly <paramref name="length"/> characters.</summary>
        public static string Fixed(string? text, int length)
        {
            return (text ?? string.Empty).PadRight(length).Substring(0, length);
        }
    }
}
