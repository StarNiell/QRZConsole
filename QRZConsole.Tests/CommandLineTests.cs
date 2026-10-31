using QRZConsole.Core.Commands;

namespace QRZConsole.Tests
{
    public class CommandLineTests
    {
        [Fact]
        public void Splits_name_and_arguments_and_lowercases_name()
        {
            CommandLine line = CommandLine.Parse("LU iu8nqi");

            Assert.Equal("lu", line.Name);
            Assert.Equal(new[] { "iu8nqi" }, line.Args);
        }

        [Fact]
        public void Multiple_spaces_do_not_create_empty_arguments()
        {
            CommandLine line = CommandLine.Parse("  qt   1    5  ");

            Assert.Equal("qt", line.Name);
            Assert.Equal(new[] { "1", "5" }, line.Args);
        }

        [Fact]
        public void RestFrom_keeps_the_original_text_including_inner_spaces()
        {
            CommandLine line = CommandLine.Parse("aq IU0ABC 7.076 FT8 2026-10-31 14:05 nice  QSO, 73!");

            Assert.Equal("nice  QSO, 73!", line.RestFrom(5));
            Assert.Equal("", line.RestFrom(6 + 10));
        }

        [Fact]
        public void Missing_arguments_are_empty_and_non_numbers_are_zero()
        {
            CommandLine line = CommandLine.Parse("gp abc");

            Assert.Equal(0, line.IntArg(0));
            Assert.Equal("", line.Arg(3));
            Assert.Equal(0, line.IntArg(3));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Blank_lines_are_empty(string? text)
        {
            Assert.True(CommandLine.Parse(text).IsEmpty);
        }
    }
}
