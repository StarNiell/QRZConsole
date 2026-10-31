using QRZConsole.Core.Formatting;
using QRZConsole.Core.Models;

namespace QRZConsole.Tests
{
    public class LogbookFormatterTests
    {
        private static readonly LogbookEntry Entry = new()
        {
            Position = 3,
            QSODateTime = new DateTime(2026, 10, 31, 14, 5, 0),
            Call = "IU0ABC",
            Band = "40m",
            Frequency = "7.076",
            Mode = "FT8",
            DXCC = "248",
            Country = "ITALY",
            OperatorName = "Mario",
            GridLocator = "JN70",
            Comments = "73",
            Confirmed = true
        };

        [Fact]
        public void Adif_record_has_length_prefixed_fields()
        {
            string record = LogbookFormatter.Adif(new[] { Entry }).Single();

            Assert.StartsWith("<CALL:6>IU0ABC", record);
            Assert.Contains("<QSO_DATE:8>20261031", record);
            Assert.Contains("<TIME_ON:6>140500", record);
            Assert.Contains("<FREQ:5>7.076", record);
            Assert.Contains("<COMMENT:2>73", record);
            Assert.EndsWith("<EOR>", record);
        }

        [Fact]
        public void Tables_have_fixed_width_rows()
        {
            List<string> page = LogbookFormatter.PageTable(new[] { Entry }).ToList();
            List<string> range = LogbookFormatter.RangeTable(new[] { Entry }).ToList();

            Assert.Equal(5, page.Count); // line, header, line, row, line
            Assert.Equal(page[1].Length, page[3].Length);
            Assert.Equal(range[1].Length, range[3].Length);
            Assert.Contains("| IU0ABC        |", page[3]);
            Assert.Contains(" * ", page[3]); // QSL confirmed
        }

        [Fact]
        public void Tab_separated_has_header_and_one_row_per_entry()
        {
            List<string> lines = LogbookFormatter.TabSeparated(new[] { Entry }).ToList();

            Assert.Equal(2, lines.Count);
            Assert.StartsWith("Num.\t", lines[0]);
            Assert.StartsWith("3\t2026-10-31 14:05\tIU0ABC\t", lines[1]);
        }

        [Fact]
        public void Xml_keeps_the_position_element_name()
        {
            string xml = LogbookFormatter.Xml(new[] { Entry });

            Assert.Contains("<position>3</position>", xml);
            Assert.Contains("<Call>IU0ABC</Call>", xml);
        }

        [Fact]
        public void Fixed_pads_and_truncates()
        {
            Assert.Equal("ab   ", Text.Fixed("ab", 5));
            Assert.Equal("abcde", Text.Fixed("abcdefg", 5));
            Assert.Equal("   ", Text.Fixed(null, 3));
        }
    }
}
