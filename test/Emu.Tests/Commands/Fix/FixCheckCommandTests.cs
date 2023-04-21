// <copyright file="FixCheckCommandTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands.Fix
{
    using System.CommandLine.Parsing;
    using Emu.Commands;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class FixCheckCommandTests : TestBase
    {
        public FixCheckCommandTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void FixOptionDoesNotBundleArgument()
        {
            var optionFirst = "fix check B:\\Marina\\**\\*.flac -f FL010";
            var optionSecond = "fix check -f FL010 B:\\Marina\\**\\*.flac";

            var parser = this.CliParser;
            var result1 = parser.Parse(optionFirst);
            var result2 = parser.Parse(optionSecond);

            Assert.True(result1.Errors.Count == 0);
            Assert.True(result2.Errors.Count == 0);
            Assert.Equal(
                result1.CommandResult.FindResultFor(Common.Fixes).GetValueOrDefault<string[]>(),
                result2.CommandResult.FindResultFor(Common.Fixes).GetValueOrDefault<string[]>());

            Assert.All(
                new string[][]
                {
                    result1.CommandResult.FindResultFor(Common.Targets).GetValueOrDefault<string[]>(),
                    result2.CommandResult.FindResultFor(Common.Targets).GetValueOrDefault<string[]>(),
                },
                (value) => Assert.Equal(new string[] { "B:\\Marina\\**\\*.flac" }, value));
        }

        [Theory]
        [InlineData("fix check B:\\Marina\\**\\*.flac -f FL010,FL020")]
        [InlineData("fix check B:\\Marina\\**\\*.flac -f=FL010,FL020")]
        [InlineData("fix check B:\\Marina\\**\\*.flac --fix FL010,FL020")]
        [InlineData("fix check B:\\Marina\\**\\*.flac --fix=FL010,FL020")]
        [InlineData("fix check  -f FL010,FL020 B:\\Marina\\**\\*.flac")]
        [InlineData("fix check  -f=FL010,FL020 B:\\Marina\\**\\*.flac")]
        [InlineData("fix check  --fix FL010,FL020 B:\\Marina\\**\\*.flac")]
        [InlineData("fix check  --fix=FL010,FL020 B:\\Marina\\**\\*.flac")]
        public void FixOptionSupportsCommaDelimitter(string command)
        {
            var parser = this.CliParser;
            var result = parser.Parse(command);

            Assert.True(result.Errors.Count == 0);

            Assert.Equal(
                new string[] { "FL010", "FL020" },
                result.CommandResult.FindResultFor(Common.Fixes).GetValueOrDefault<string[]>());
        }

        [Fact]
        public void EmuFixCheckWorks()
        {
            var command = @"fix check C:\Work\Github\metadata-utility\test\Fixtures\FL_BAR_LT\3.17_Duration\*.flac -f FL010";
            var parser = this.CliParser;

            var result = parser.Parse(command);

            Assert.True(result.Errors.Count == 0);

            var commandResult = result.CommandResult;

            commandResult
                .FindResultFor(Common.Fixes)
                .GetValueOrDefault<string[]>()
                .Should()
                .BeEquivalentTo(new string[] { "FL010" });

            commandResult
                .FindResultFor(Common.Targets)
                .GetValueOrDefault<string[]>()
                .Should()
                .BeEquivalentTo(new string[] { "C:\\Work\\Github\\metadata-utility\\test\\Fixtures\\FL_BAR_LT\\3.17_Duration\\*.flac" });
        }
    }
}
