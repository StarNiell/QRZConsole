using System;
using System.Collections.Generic;
using System.Globalization;

namespace QRZConsole.Core.Commands
{
    /// <summary>
    /// A command line split into the command name and its arguments.
    /// Arguments are separated by one or more spaces; <see cref="RestFrom"/> returns the
    /// original text from a given argument to the end (for free text such as comments).
    /// </summary>
    public sealed class CommandLine
    {
        private readonly List<int> _argStarts;

        private CommandLine(string raw, string name, List<string> args, List<int> argStarts)
        {
            Raw = raw;
            Name = name;
            Args = args;
            _argStarts = argStarts;
        }

        /// <summary>The line exactly as typed.</summary>
        public string Raw { get; }

        /// <summary>Command name, lower case.</summary>
        public string Name { get; }

        /// <summary>Arguments after the command name.</summary>
        public IReadOnlyList<string> Args { get; }

        public bool IsEmpty => Name.Length == 0;

        /// <summary>Argument at index (0 = first after the command), or empty string.</summary>
        public string Arg(int index) => index >= 0 && index < Args.Count ? Args[index] : string.Empty;

        /// <summary>Argument parsed as integer, or 0 when missing or not a number.</summary>
        public int IntArg(int index)
        {
            return int.TryParse(Arg(index), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;
        }

        /// <summary>Original text from argument <paramref name="index"/> to the end of the line (empty when missing).</summary>
        public string RestFrom(int index)
        {
            if (index < 0 || index >= _argStarts.Count) return string.Empty;
            return Raw.Substring(_argStarts[index]).TrimEnd();
        }

        public static CommandLine Parse(string? line)
        {
            string raw = line ?? string.Empty;
            var tokens = new List<string>();
            var starts = new List<int>();

            int i = 0;
            while (i < raw.Length)
            {
                while (i < raw.Length && char.IsWhiteSpace(raw[i])) i++;
                if (i >= raw.Length) break;

                int start = i;
                while (i < raw.Length && !char.IsWhiteSpace(raw[i])) i++;
                tokens.Add(raw.Substring(start, i - start));
                starts.Add(start);
            }

            if (tokens.Count == 0)
                return new CommandLine(raw, string.Empty, new List<string>(), new List<int>());

            string name = tokens[0].ToLowerInvariant();
            tokens.RemoveAt(0);
            starts.RemoveAt(0);
            return new CommandLine(raw, name, tokens, starts);
        }

        public override string ToString() => Raw;
    }
}
