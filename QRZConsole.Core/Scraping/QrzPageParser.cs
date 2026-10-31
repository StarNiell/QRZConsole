using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using QRZConsole.Core.Data;
using QRZConsole.Core.Models;

namespace QRZConsole.Core.Scraping
{
    /// <summary>
    /// Turns data extracted from QRZ pages into model objects. When QRZ changes the
    /// meaning of a column or a label, this is the place to adapt (selectors are in QrzSite).
    /// </summary>
    public static class QrzPageParser
    {
        private static readonly string[] DateTimeFormats = { "yyyy-MM-dd HH:mm", "yyyy-MM-dd H:mm", "yyyy-MM-dd HH:mm:ss" };

        /// <summary>Position of a row: data-pos is 0-based, positions shown to the user are 1-based. -1 if missing.</summary>
        public static int RowPosition(RawLogbookRow row)
        {
            return int.TryParse(row.Pos, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pos) && pos >= 0 ? pos + 1 : -1;
        }

        /// <summary>
        /// Maps a table row to an entry using the header ids. Position comes from the
        /// "Num." column (th_lnum) unless <paramref name="position"/> is given.
        /// </summary>
        public static LogbookEntry MapRow(IReadOnlyList<string> headers, RawLogbookRow row, int? position = null)
        {
            var entry = new LogbookEntry { LoTWSent = row.ClassName.Contains("lotw_sent", StringComparison.Ordinal) };
            string date = string.Empty;
            string time = string.Empty;

            for (int i = 0; i < row.Cells.Count && i < headers.Count; i++)
            {
                string text = (row.Cells[i] ?? string.Empty).Trim();
                switch (headers[i])
                {
                    case "th_lnum": entry.Position = ParseInt(text); break;
                    case "th_date": date = text; break;
                    case "th_time": time = text; break;
                    case "th_call2": entry.Call = text.Replace("Ø", "0"); break;
                    case "th_band1": entry.Band = text; break;
                    case "th_freq2": entry.Frequency = text; break;
                    case "th_mode2": entry.Mode = text; break;
                    case "th_grid2": entry.GridLocator = text; break;
                    case "th_country2": entry.Country = text; break;
                    case "th_name2": entry.OperatorName = text; break;
                    case "th_comments": entry.Comments = text; break;
                    case "th_status": entry.Confirmed = text.StartsWith("Confirmed", StringComparison.Ordinal); break;
                }
            }

            if (position.HasValue)
                entry.Position = position.Value;

            entry.QSODateTime = ParseDateTime($"{date} {time}".Trim());
            entry.DXCC = entry.Country.Length > 0 ? HamData.GetDxccByCountry(entry.Country) : string.Empty;
            return entry;
        }

        /// <summary>All the QSO rows of a logbook page (rows without a valid number are skipped).</summary>
        public static List<LogbookEntry> MapPage(RawLogbookTable table)
        {
            return table.Rows.Select(r => MapRow(table.Headers, r))
                             .Where(e => e.Position > 0)
                             .ToList();
        }

        /// <summary>Rows of a page whose position is within [start, end].</summary>
        public static List<LogbookEntry> MapRange(RawLogbookTable table, int start, int end)
        {
            var result = new List<LogbookEntry>();
            foreach (RawLogbookRow row in table.Rows)
            {
                int pos = RowPosition(row);
                if (pos >= start && pos <= end)
                    result.Add(MapRow(table.Headers, row, pos));
            }
            return result;
        }

        /// <summary>
        /// Tab-separated text of the rows whose first cell is a number (the "tr" view):
        /// every cell followed by a tab, every row followed by CR LF.
        /// </summary>
        public static string FormatRawRows(IEnumerable<IReadOnlyList<string>> rows)
        {
            var sb = new StringBuilder();
            foreach (IReadOnlyList<string> cells in rows)
            {
                if (cells.Count == 0 || !int.TryParse((cells[0] ?? string.Empty).Trim(), out _))
                    continue;

                foreach (string cell in cells)
                    sb.Append(cell).Append('\t');
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        /// <summary>Tab-separated text of all rows (the "seen before" table of check worked).</summary>
        public static string FormatAllRows(IEnumerable<IReadOnlyList<string>> rows)
        {
            var sb = new StringBuilder();
            foreach (IReadOnlyList<string> cells in rows)
            {
                foreach (string cell in cells)
                    sb.Append(cell).Append('\t');
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        public static LookupEntry MapLookup(RawLookup raw)
        {
            var entry = new LookupEntry
            {
                QRZ = raw.Call.Trim(),
                Country = raw.Country.Trim(),
                Name = raw.Name.Trim(),
                Email = raw.Email.Replace("Email: ", "").Trim()
            };

            Match dxcc = Regex.Match(raw.CountryHtml ?? string.Empty, @"[?&;]dxcc=(\d+)");
            if (dxcc.Success)
                entry.DXCC = dxcc.Groups[1].Value;

            // First line is the name, then up to three address lines.
            string[] lines = (raw.NameAndAddress ?? string.Empty)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToArray();
            entry.Address1 = lines.Length > 1 ? lines[1] : string.Empty;
            entry.Address2 = lines.Length > 2 ? lines[2] : string.Empty;
            entry.Address3 = lines.Length > 3 ? lines[3] : string.Empty;

            foreach (List<string> pair in raw.Details)
            {
                if (pair.Count < 2) continue;
                string value = (pair[1] ?? string.Empty).Trim();
                switch ((pair[0] ?? string.Empty).Trim())
                {
                    case "Grid Square": entry.GridSquare = value; break;
                    case "Lookups": entry.Lookups = value; break;
                    case "Distance": entry.Distance = value; break;
                    case "US State": entry.UsState = value; break;
                    case "US County": entry.UsCounty = value; break;
                }
            }

            return entry;
        }

        /// <summary>Number of the page (1-based) that contains a position, given the QSOs per page.</summary>
        public static int PageOfPosition(int position, int entriesPerPage)
        {
            if (position < 1 || entriesPerPage < 1) return 0;
            return (position - 1) / entriesPerPage + 1;
        }

        /// <summary>Parses "of 12" (page count label) or plain numbers. 0 when not a number.</summary>
        public static int ParseInt(string? text)
        {
            string t = (text ?? string.Empty).Replace("of ", "").Trim();
            return int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;
        }

        /// <summary>Parses the logbook date and time ("2026-10-31 14:05"); DateTime.MinValue if not valid.</summary>
        public static DateTime ParseDateTime(string text)
        {
            if (DateTime.TryParseExact(text, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime exact))
                return exact;
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime invariant))
                return invariant;
            return DateTime.TryParse(text, out DateTime local) ? local : DateTime.MinValue;
        }
    }
}
