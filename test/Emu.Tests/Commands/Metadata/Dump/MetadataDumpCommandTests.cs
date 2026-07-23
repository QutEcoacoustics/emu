// <copyright file="MetadataDumpCommandTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands.Metadata.Dump
{
    using System;
    using System.CommandLine.Parsing;
    using System.Linq;
    using Emu.Commands;
    using Emu.Commands.Metadata.Dump;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;

    public class MetadataDumpCommandTests : TestBase
    {
        public MetadataDumpCommandTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void SupportsTargets()
        {
            var command = "metadata dump **/*.wav";

            var result = this.CliParser.Parse(command);

            Assert.True(result.Errors.Count == 0);

            var commandResult = result.CommandResult.Command;

            Assert.IsType<MetadataDumpCommand>(commandResult);

            Assert.Equal(
                result.FindResultFor(MetadataDumpCommand.DumpTargets).GetValueOrDefault<string[]>(),
                "**/*.wav".AsArray());

            result.UnmatchedTokens.Should().BeEmpty();
            result.UnparsedTokens.Should().BeEmpty();
        }

        [Fact]
        public void SupportsBlockFilterOption()
        {
            var command = "metadata dump **/*.wav --blocks GUANO,WAMD";

            var result = this.CliParser.Parse(command);

            Assert.True(result.Errors.Count == 0);
            Assert.Equal(
                result.FindResultFor(MetadataDumpCommand.BlockFilterOption).GetValueOrDefault<string[]>(),
                new[] { "GUANO", "WAMD" });
            result.UnmatchedTokens.Should().BeEmpty();
            result.UnparsedTokens.Should().BeEmpty();
        }

        [Fact]
        public void FailsGracefullyForCsv()
        {
            var command = "metadata dump **/*.wav -F CSV";

            var result = this.CliParser.Parse(command);

            Assert.True(result.Errors.Count == 1);
            result.Errors.First().Message.Contains("CSV output is not supported for this command");
        }

        [Fact]
        public void ParentMetadataTargetsAreInvalidForDump()
        {
            var command = "metadata x dump y";

            var result = this.CliParser.Parse(command);

            Assert.True(result.Errors.Count == 1);
            result.Errors.First().Message.Should().Contain("must be provided after `dump` only");
        }
    }
}
