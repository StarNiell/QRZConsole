using System.Text.Json;
using QRZConsole.Core.Models;
using QRZConsole.Core.Scraping;

namespace QRZConsole.Tests
{
    /// <summary>
    /// The JSON below is what the extraction scripts in QRZLibrary return (captured by running them
    /// in Chromium on a page with the same structure as QRZ.com). It must bind to the Raw* types
    /// with the options used by BrowserSession (case-insensitive property names).
    /// </summary>
    public class BrowserJsonTests
    {
        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

        private const string TableJson = """
            {"headers":["th_lnum","th_date","th_time","th_call2","th_comments","th_status"],
             "rows":[{"pos":"25","rowNum":"1","bookId":"999","className":"lotw_sent",
                      "cells":["26","2026-10-31","14:05","IUØABC","it's \"fine\"","Confirmed"]},
                     {"pos":"26","rowNum":"2","bookId":"999","className":"",
                      "cells":["27","2026-10-30","09:00","K1ABC","",""]}]}
            """;

        private const string LookupJson = """
            {"call":"IU8NQI","countryHtml":"<span><a href=\"/atlas/?dxcc=248\">Italy</a></span>","country":"Italy",
             "name":"Aniello Rossi","nameAndAddress":"Aniello Rossi\nVia Roma 1\n80100 Napoli\nItaly","email":"Email: a@b.it",
             "details":[["Grid Square","JN70ft"],["JN70ft",""],["Lookups","4321"],["4321",""]]}
            """;

        [Fact]
        public void Table_json_maps_to_entries()
        {
            RawLogbookTable table = JsonSerializer.Deserialize<RawLogbookTable>(TableJson, Options)!;

            List<LogbookEntry> page = QrzPageParser.MapPage(table);
            List<LogbookEntry> range = QrzPageParser.MapRange(table, 27, 27);

            Assert.Equal(2, page.Count);
            Assert.Equal("IU0ABC", page[0].Call);
            Assert.Equal("it's \"fine\"", page[0].Comments);
            Assert.True(page[0].LoTWSent && page[0].Confirmed);
            Assert.Equal("K1ABC", Assert.Single(range).Call);
            Assert.Equal("999", table.Rows[0].BookId);
        }

        [Fact]
        public void Lookup_json_maps_to_entry()
        {
            RawLookup raw = JsonSerializer.Deserialize<RawLookup>(LookupJson, Options)!;

            LookupEntry e = QrzPageParser.MapLookup(raw);

            Assert.Equal("IU8NQI", e.QRZ);
            Assert.Equal("248", e.DXCC);
            Assert.Equal("Aniello Rossi", e.Name);
            Assert.Equal("Via Roma 1", e.Address1);
            Assert.Equal("80100 Napoli", e.Address2);
            Assert.Equal("Italy", e.Address3);
            Assert.Equal("a@b.it", e.Email);
            Assert.Equal("JN70ft", e.GridSquare);
            Assert.Equal("4321", e.Lookups);
        }
    }
}
