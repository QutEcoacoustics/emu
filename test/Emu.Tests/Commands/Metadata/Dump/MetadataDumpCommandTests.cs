// <copyright file="MetadataDumpCommandTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands.Metadata.Dump
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine.Parsing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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
                result.FindResultFor(Common.Targets).GetValueOrDefault<string[]>(),
                "**/*.wav".AsArray());

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
    }
}
