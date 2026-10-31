using System.Collections.Generic;

namespace QRZConsole.Core.Scraping
{
    // Plain data extracted from QRZ pages by JavaScript (deserialized from JSON).
    // Kept free of any browser type so that the parsing below can be unit-tested.

    /// <summary>The logbook table (#lbtab): column ids from the header and the body rows.</summary>
    public sealed class RawLogbookTable
    {
        /// <summary>Id of each header cell (th_lnum, th_date, th_call2, ...), in column order.</summary>
        public List<string> Headers { get; set; } = new List<string>();

        public List<RawLogbookRow> Rows { get; set; } = new List<RawLogbookRow>();
    }

    public sealed class RawLogbookRow
    {
        /// <summary>data-pos attribute: 0-based position of the QSO in the logbook.</summary>
        public string Pos { get; set; } = string.Empty;

        /// <summary>data-rownum attribute.</summary>
        public string RowNum { get; set; } = string.Empty;

        /// <summary>data-bookid attribute.</summary>
        public string BookId { get; set; } = string.Empty;

        /// <summary>CSS classes of the row (e.g. contains "lotw_sent").</summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>Visible text of each cell, in column order.</summary>
        public List<string> Cells { get; set; } = new List<string>();
    }

    /// <summary>The callsign box (#csdata) and detail table (#dt) of a lookup result.</summary>
    public sealed class RawLookup
    {
        /// <summary>Text of #csdata child 0 (the callsign).</summary>
        public string Call { get; set; } = string.Empty;

        /// <summary>HTML of #csdata child 1 (contains the ?dxcc=NNN link).</summary>
        public string CountryHtml { get; set; } = string.Empty;

        /// <summary>Text of #csdata child 1 (the country).</summary>
        public string Country { get; set; } = string.Empty;

        /// <summary>Text of the first element inside #csdata child 3 (the name).</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Whole text of #csdata child 3: name followed by the address lines.</summary>
        public string NameAndAddress { get; set; } = string.Empty;

        /// <summary>Text of #csdata child 5 ("Email: ...").</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Label/value pairs from the detail table (e.g. "Grid Square" / "JN70").</summary>
        public List<List<string>> Details { get; set; } = new List<List<string>>();
    }
}
