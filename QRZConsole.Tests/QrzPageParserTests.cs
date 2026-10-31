using QRZConsole.Core.Models;
using QRZConsole.Core.Scraping;

namespace QRZConsole.Tests
{
    public class QrzPageParserTests
    {
        private static readonly List<string> Headers = new()
        {
            "th_lnum", "th_date", "th_time", "th_call2", "th_band1", "th_freq2", "th_mode2",
            "th_grid2", "th_country2", "th_name2", "th_comments", "th_status"
        };

        private static RawLogbookRow Row(int dataPos, int number, string call, string status = "", string css = "")
        {
            return new RawLogbookRow
            {
                Pos = dataPos.ToString(),
                BookId = "123",
                ClassName = css,
                Cells = new List<string>
                {
                    $" {number} ", "2026-10-31", "14:05", call, "40m", "7.076", "FT8",
                    "JN70", "Italy", "Mario Rossi", "nice qso", status
                }
            };
        }

        [Fact]
        public void Maps_a_row_using_the_header_ids()
        {
            LogbookEntry e = QrzPageParser.MapRow(Headers, Row(4, 5, "IUØABC", "Confirmed (LoTW)", "lotw_sent odd"));

            Assert.Equal(5, e.Position);
            Assert.Equal(new DateTime(2026, 10, 31, 14, 5, 0), e.QSODateTime);
            Assert.Equal("IU0ABC", e.Call); // slashed zero normalized
            Assert.Equal("40m", e.Band);
            Assert.Equal("7.076", e.Frequency);
            Assert.Equal("FT8", e.Mode);
            Assert.Equal("JN70", e.GridLocator);
            Assert.Equal("Italy", e.Country);
            Assert.Equal("248", e.DXCC);
            Assert.Equal("Mario Rossi", e.OperatorName);
            Assert.Equal("nice qso", e.Comments);
            Assert.True(e.Confirmed);
            Assert.True(e.LoTWSent);
        }

        [Fact]
        public void Columns_are_matched_by_id_not_by_order()
        {
            var headers = new List<string> { "th_call2", "th_lnum" };
            var row = new RawLogbookRow { Cells = new List<string> { "K1ABC", "7" } };

            LogbookEntry e = QrzPageParser.MapRow(headers, row);

            Assert.Equal("K1ABC", e.Call);
            Assert.Equal(7, e.Position);
        }

        [Fact]
        public void Page_skips_rows_without_a_number()
        {
            var table = new RawLogbookTable { Headers = Headers, Rows = { Row(0, 1, "A1A"), new RawLogbookRow { Cells = { "Total" } }, Row(1, 2, "B1B") } };

            List<LogbookEntry> page = QrzPageParser.MapPage(table);

            Assert.Equal(new[] { "A1A", "B1B" }, page.Select(e => e.Call));
        }

        [Fact]
        public void Range_uses_data_pos_plus_one_as_position()
        {
            var table = new RawLogbookTable { Headers = Headers, Rows = { Row(9, 99, "A"), Row(10, 99, "B"), Row(11, 99, "C"), Row(12, 99, "D") } };

            List<LogbookEntry> range = QrzPageParser.MapRange(table, 11, 12);

            Assert.Equal(new[] { "B", "C" }, range.Select(e => e.Call));
            Assert.Equal(new[] { 11, 12 }, range.Select(e => e.Position));
        }

        [Theory]
        [InlineData(1, 25, 1)]
        [InlineData(25, 25, 1)]
        [InlineData(26, 25, 2)]
        [InlineData(200, 50, 4)]
        [InlineData(0, 25, 0)]
        public void Page_of_position(int position, int perPage, int expected)
        {
            Assert.Equal(expected, QrzPageParser.PageOfPosition(position, perPage));
        }

        [Fact]
        public void Raw_rows_keep_only_numbered_rows_tab_separated()
        {
            var rows = new List<List<string>>
            {
                new() { "Num", "Call" },
                new() { " 1 ", "IU0ABC" },
                new(),
                new() { "2", "K1ABC" }
            };

            Assert.Equal(" 1 \tIU0ABC\t\r\n2\tK1ABC\t\r\n", QrzPageParser.FormatRawRows(rows));
        }

        [Fact]
        public void Lookup_maps_box_children_and_detail_labels()
        {
            var raw = new RawLookup
            {
                Call = " IU8NQI ",
                CountryHtml = "<a href=\"/atlas?dxcc=248\">Italy</a>",
                Country = "Italy",
                Name = "Aniello",
                NameAndAddress = "Aniello\r\nVia Roma 1\n\n80100 Napoli\nItaly",
                Email = "Email: test@example.com",
                Details = new List<List<string>>
                {
                    new() { "Grid Square", " JN70ft " },
                    new() { "Lookups", "1234" },
                    new() { "Distance", "10 km" },
                    new() { "Something else", "x" },
                    new() { "lonely" }
                }
            };

            LookupEntry e = QrzPageParser.MapLookup(raw);

            Assert.Equal("IU8NQI", e.QRZ);
            Assert.Equal("248", e.DXCC);
            Assert.Equal("Italy", e.Country);
            Assert.Equal("Aniello", e.Name);
            Assert.Equal("Via Roma 1", e.Address1);
            Assert.Equal("80100 Napoli", e.Address2);
            Assert.Equal("Italy", e.Address3);
            Assert.Equal("test@example.com", e.Email);
            Assert.Equal("JN70ft", e.GridSquare);
            Assert.Equal("1234", e.Lookups);
            Assert.Equal("10 km", e.Distance);
        }

        [Fact]
        public void Lookup_without_dxcc_link_leaves_dxcc_empty()
        {
            Assert.Equal("", QrzPageParser.MapLookup(new RawLookup { CountryHtml = "<span>Italy</span>" }).DXCC);
        }

        [Theory]
        [InlineData("of 12", 12)]
        [InlineData(" 7 ", 7)]
        [InlineData("", 0)]
        [InlineData("n/a", 0)]
        public void ParseInt_handles_page_labels(string text, int expected)
        {
            Assert.Equal(expected, QrzPageParser.ParseInt(text));
        }
    }
}
