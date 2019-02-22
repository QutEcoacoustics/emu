// <copyright file="ParsingTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests
{
    using MetadataUtility.Dates;
    using Xunit;

    public class ParsingTests
    {
        [Theory]
        [InlineData("+10:00", 10 * 3600)]
        [InlineData("10:00", 10 * 3600)]
        [InlineData("-10:00", -10 * 3600)]
        [InlineData("00:00", 0)]
        [InlineData("+00:00", 0)]
        [InlineData("-00:00", 0)]
        [InlineData("Z", 0)]
        [InlineData("+09:30", 9.5 * 3600)]
        [InlineData("+10", 10 * 3600)]
        [InlineData("-10", -10 * 3600)]
        [InlineData("1030", 10.5 * 3600)]
        [InlineData("-1030", -10.5 * 3600)]
        public void TestTryParseOffset(string test, int expectedSeconds)
        {
            Assert.True(Parsing.TryParseOffset(test, out var offset));
            Assert.Equal(expectedSeconds, offset.Seconds);
        }
    }
}
