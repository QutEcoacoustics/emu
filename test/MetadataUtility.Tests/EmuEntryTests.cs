// <copyright file="EmuEntryTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests
{
    using System;
    using MetadataUtility.Tests.TestHelpers;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class EmuEntryTests : IClassFixture<FixtureHelper.FixtureData>
    {
        private readonly FixtureHelper.FixtureData data;

        public EmuEntryTests(FixtureHelper.FixtureData data)
        {
            this.data = data;
        }

        [Fact]
        public async void EmuWorks()
        {
            var testFile = this.data[FixtureModel.ShortFile];

            var result = await EmuEntry.Main(
                new[]
                {
                    testFile.AbsoluteFixturePath,
                });

            Assert.Equal(0, result);
        }

        [Fact]
        public async void EmuFixCheckWorks()
        {

            var result = await EmuEntry.Main(
              @"fix check C:\Work\Github\metadata-utility\test\Fixtures\FL_BAR_LT\3.17_Duration\*.flac -f FL010".Split(' '));

            Assert.Equal(0, result);
        }

        [Theory]
        [InlineData("", LogLevel.Information)]
        [InlineData("-v", LogLevel.Debug)]
        [InlineData("-vv", LogLevel.Trace)]
        [InlineData("--verbose", LogLevel.Debug)]
        [InlineData("--verbose --verbose", LogLevel.Trace)]
        [InlineData("-l 0", LogLevel.Trace)]
        [InlineData("-l 1", LogLevel.Debug)]
        [InlineData("-l 2", LogLevel.Information)]
        [InlineData("-l 3", LogLevel.Warning)]
        [InlineData("-l 4", LogLevel.Error)]
        [InlineData("-l 5", LogLevel.Critical)]
        [InlineData("-l 6", LogLevel.None)]
        public void ProcessArgumentsVerbosity(string arg, LogLevel expectedValue)
        {
            //var (app, mainArgs) = EmuEntry.ProcessArguments(arg.Split(' '));

            //Assert.Equal(expectedValue, mainArgs.Verbosity);
            throw new NotImplementedException();
        }

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
        public void ProcessArgumentsUtcOffset(string test, int expectedSeconds)
        {
            //var (app, mainArgs) = EmuEntry.ProcessArguments(
            //    new[]
            //    {
            //        "blah",
            //        "--utc-offset-hint",
            //        test,
            //    });

            //Assert.Equal(expectedSeconds, mainArgs.UtcOffsetHint.Value.Seconds);
            throw new NotImplementedException();
        }
    }
}
