using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using QRZConsole.Core.Models;

namespace QRZConsole.Core.Formatting
{
    /// <summary>Renders logbook entries in the console formats (table, tab-separated, ADIF, XML).</summary>
    public static class LogbookFormatter
    {
        /// <summary>Fixed-width table used for a logbook page (tt): QSL column after Mode.</summary>
        public static IEnumerable<string> PageTable(IReadOnlyList<LogbookEntry> entries)
        {
            string line = new string('-', 223);
            yield return line;
            yield return "| " + Text.Fixed("Num.", 5) + " | " + Text.Fixed("Date - Time", 16) + " | " + Text.Fixed("Call", 13)
                       + " | " + Text.Fixed("Band", 5) + " | " + Text.Fixed("Freq.", 7) + " | " + Text.Fixed("Mode", 6)
                       + " | QSL | " + Text.Fixed("GridLoc", 8) + " | " + Text.Fixed("Country", 20)
                       + " | " + Text.Fixed("Operator Name", 50) + " | " + Text.Fixed("Comments", 50) + " |LoTW | ";
            yield return line;

            foreach (LogbookEntry e in entries)
            {
                yield return "| " + Text.Fixed(e.Position.ToString().PadLeft(5), 5)
                           + " | " + Text.Fixed(e.QSODateTime.ToString("yyyy-MM-dd HH:mm"), 16)
                           + " | " + Text.Fixed(e.Call, 13)
                           + " | " + Text.Fixed(e.Band, 5)
                           + " | " + Text.Fixed(e.Frequency, 7)
                           + " | " + Text.Fixed(e.Mode, 6)
                           + " | " + (e.Confirmed ? " * " : "   ")
                           + " | " + Text.Fixed(e.GridLocator, 8)
                           + " | " + Text.Fixed(e.Country, 20)
                           + " | " + Text.Fixed(e.OperatorName, 50)
                           + " | " + Text.Fixed(e.Comments, 50)
                           + " |" + (e.LoTWSent ? "  * " : "    ") + " | ";
            }

            yield return line;
        }

        /// <summary>Fixed-width table used for a range of QSOs (qt): Confirmed and LoTW at the end.</summary>
        public static IEnumerable<string> RangeTable(IReadOnlyList<LogbookEntry> entries)
        {
            string line = new string('-', 229);
            yield return line;
            yield return "| " + Text.Fixed("Num.", 5) + " | " + Text.Fixed("Date - Time", 16) + " | " + Text.Fixed("Call", 13)
                       + " | " + Text.Fixed("Band", 5) + " | " + Text.Fixed("Freq.", 7) + " | " + Text.Fixed("Mode", 6)
                       + " | " + Text.Fixed("GridLoc", 8) + " | " + Text.Fixed("Country", 20)
                       + " | " + Text.Fixed("Operator Name", 50) + " | " + Text.Fixed("Comments", 50) + " |Confirmed | LoTw | ";
            yield return line;

            foreach (LogbookEntry e in entries)
            {
                yield return "| " + Text.Fixed(e.Position.ToString().PadLeft(5), 5)
                           + " | " + Text.Fixed(e.QSODateTime.ToString("yyyy-MM-dd HH:mm"), 16)
                           + " | " + Text.Fixed(e.Call, 13)
                           + " | " + Text.Fixed(e.Band, 5)
                           + " | " + Text.Fixed(e.Frequency, 7)
                           + " | " + Text.Fixed(e.Mode, 6)
                           + " | " + Text.Fixed(e.GridLocator, 8)
                           + " | " + Text.Fixed(e.Country, 20)
                           + " | " + Text.Fixed(e.OperatorName, 50)
                           + " | " + Text.Fixed(e.Comments, 50)
                           + " |" + (e.Confirmed ? "    *    " : "         ")
                           + " | " + (e.LoTWSent ? "  * " : "    ") + " | ";
            }

            yield return line;
        }

        /// <summary>Tab-separated rows with a header (qr).</summary>
        public static IEnumerable<string> TabSeparated(IReadOnlyList<LogbookEntry> entries)
        {
            yield return string.Join("\t", "Num.", "Date Time", "Call", "Band", "Frequency", "Mode", "GridLocator",
                                     "Country", "Operator Name", "Comments", "Confirmed", "LoTW");
            foreach (LogbookEntry e in entries)
            {
                yield return string.Join("\t", e.Position, e.QSODateTime.ToString("yyyy-MM-dd HH:mm"), e.Call, e.Band,
                                         e.Frequency, e.Mode, e.GridLocator, e.Country, e.OperatorName, e.Comments,
                                         e.Confirmed ? "*" : "", e.LoTWSent ? "*" : "");
            }
        }

        /// <summary>One ADIF record per entry (multi-line string each).</summary>
        public static IEnumerable<string> Adif(IReadOnlyList<LogbookEntry> entries)
        {
            foreach (LogbookEntry e in entries)
            {
                string date = e.QSODateTime.ToString("yyyyMMdd");
                string time = e.QSODateTime.ToString("HHmmss");
                var sb = new StringBuilder();
                sb.Append(Field("CALL", e.Call, ""));
                sb.Append(Field("BAND", e.Band));
                sb.Append(Field("QSO_DATE", date));
                sb.Append(Field("TIME_ON", time));
                sb.Append(Field("FREQ", e.Frequency));
                sb.Append(Field("MODE", e.Mode));
                sb.Append(Field("DXCC", e.DXCC));
                sb.Append(Field("COUNTRY", e.Country));
                sb.Append(Field("NAME", e.OperatorName));
                sb.Append(Field("GRIDSQUARE", e.GridLocator));
                sb.Append(Field("COMMENT", e.Comments));
                sb.Append("<EOR>");
                yield return sb.ToString();
            }
        }

        /// <summary>XML serialization of the entries (tx).</summary>
        public static string Xml(IReadOnlyList<LogbookEntry> entries)
        {
            var serializer = new XmlSerializer(typeof(List<LogbookEntry>));
            using var writer = new StringWriter();
            serializer.Serialize(writer, new List<LogbookEntry>(entries));
            return writer.ToString();
        }

        private static string Field(string name, string? value, string indent = "   ")
        {
            value ??= string.Empty;
            return $"{indent}<{name}:{value.Length}>{value}{Environment.NewLine}";
        }
    }
}
