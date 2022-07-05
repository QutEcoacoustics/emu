// <copyright file="UtcOffsetOptionTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Cli
{
    using System.CommandLine;
    using Emu.Cli;
    using NodaTime;
    using Xunit;

    public class UtcOffsetOptionTests
    {
        private static readonly Option<Offset?> OffsetOption = new(
            "--offset",
            UtcOffsetOption.Parser);

        [Theory]
        [InlineData("+10:00", 10 * 3600)]
        [InlineData("10:00", 10 * 3600)]
        [InlineData("-10:00", -10 * 3600)]
        [InlineData("+00:00", 0)]
        [InlineData("00:00", 0)]
        [InlineData("-00:00", 0)]
        [InlineData("Z", 0)]
        [InlineData("+09:30", 9.5 * 3600)]
        [InlineData("+10", 10 * 3600)]
        [InlineData("10", 10 * 3600)]
        [InlineData("-10", -10 * 3600)]
        [InlineData("+1030", 10.5 * 3600)]
        [InlineData("-1030", -10.5 * 3600)]
        public void ProcessArgumentsUtcOffset(string test, int expectedSeconds)
        {
            var actual = OffsetOption.Parse($"--offset {test}");

            Assert.Equal(expectedSeconds, actual.ValueForOption(OffsetOption).Value.Seconds);

            Assert.Null(UtcOffsetOption.IsValid(actual.FindResultFor(OffsetOption)));
        }
    }
}
