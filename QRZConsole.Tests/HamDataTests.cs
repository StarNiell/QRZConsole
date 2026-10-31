using QRZConsole.Core.Data;

namespace QRZConsole.Tests
{
    public class HamDataTests
    {
        [Theory]
        [InlineData(7.076, "40m")]
        [InlineData(14.2, "20m")]
        [InlineData(144.3, "2m")]
        [InlineData(0.475, "630m")]
        [InlineData(28.0, "10m")]
        public void Finds_the_band_of_a_frequency(double mhz, string band)
        {
            Assert.Equal(band, HamData.FindBand(mhz));
        }

        [Fact]
        public void Frequency_outside_bands_has_no_band()
        {
            Assert.Null(HamData.FindBand(11.0));
        }

        [Theory]
        [InlineData("7.076", 7.076)]
        [InlineData("7,076", 7.076)]
        [InlineData(" 14.07415 ", 14.07415)]
        public void Parses_MHz_with_dot_or_comma_whatever_the_culture(string text, double expected)
        {
            var previous = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("it-IT");
                Assert.True(HamData.TryParseMHz(text, out double it));
                Assert.Equal(expected, it, 6);

                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                Assert.True(HamData.TryParseMHz(text, out double us));
                Assert.Equal(expected, us, 6);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }

        [Fact]
        public void Modes_are_validated_case_insensitively()
        {
            Assert.True(HamData.IsValidMode("FT8"));
            Assert.True(HamData.IsValidMode("ssb"));
            Assert.False(HamData.IsValidMode("NOTAMODE"));
        }

        [Fact]
        public void Dxcc_lookup_by_exact_country_and_search_by_text()
        {
            Assert.Equal("248", HamData.GetDxccByCountry("Italy"));
            Assert.Equal("", HamData.GetDxccByCountry("Atlantis"));
            Assert.Contains(HamData.SearchDxcc("ital"), x => x.Key == 248);
        }

        [Fact]
        public void Reference_data_is_complete()
        {
            Assert.Equal(32, HamData.Bands.Count);
            Assert.Equal(225, HamData.Modes.Count);
            Assert.Equal(402, HamData.Dxcc.Count);
        }
    }
}
