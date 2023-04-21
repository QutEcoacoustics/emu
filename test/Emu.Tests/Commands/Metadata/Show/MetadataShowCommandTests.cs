// <copyright file="MetadataShowCommandTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands.Metadata.Show
{
    using System.CommandLine.Parsing;
    using Emu.Cli;
    using Emu.Commands;
    using Emu.Commands.Metadata.Show;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Xunit.Abstractions;
    using static Emu.EmuCommand;

    public class MetadataShowCommandTests : TestBase, IClassFixture<FixtureData>
    {
        private readonly FixtureData data;

        public MetadataShowCommandTests(ITestOutputHelper output, FixtureData data)
            : base(output, realFileSystem: true, outputFormat: OutputFormat.Default)
        {
            this.data = data;
        }

        [Fact]
        public void MetadataShowIsAnAliasForMetadata()
        {
            var command = "metadata **/*.wav --no-checksum";
            var showCommand = "metadata show **/*.wav --no-checksum";

            var result1 = this.CliParser.Parse(command);
            var result2 = this.CliParser.Parse(showCommand);

            Assert.True(result1.Errors.Count == 0);
            Assert.True(result2.Errors.Count == 0);

            var command1 = result1.CommandResult.Command;
            var command2 = result2.CommandResult.Command;

            Assert.IsType<MetadataCommand>(command1);
            Assert.IsType<MetadataShowCommand>(command2);

            Assert.Equal(
                result1.FindResultFor(Common.Targets).GetValueOrDefault<string[]>(),
                result2.FindResultFor(Common.Targets).GetValueOrDefault<string[]>());

            result1.CommandResult.FindResultFor(MetadataCommand.NoChecksumOption).GetValueOrDefault<bool>().Should().BeTrue();
            result2.CommandResult.FindResultFor(MetadataCommand.NoChecksumOption).GetValueOrDefault<bool>().Should().BeTrue();
        }

        [Fact]
        public void ItAlsoRuns()
        {
            var fixture = this.data[FixtureModel.Firmware330];
            var command = $"metadata --no-checksum {fixture.AbsoluteFixturePath}";

            var parseResult = this.CliParser.Parse(command);

            using var console = ConsoleRedirector.Create();

            var result = parseResult.Invoke();

            result.Should().Be(ExitCodes.Success);

            var output = console.NewOut.ToString();

            var consoleWidth = 80;
            var a = fixture.AbsoluteFixturePath[0..consoleWidth];
            var b = fixture.AbsoluteFixturePath[consoleWidth..^1];

            output.Should().Contain(a);
            output.Should().Contain(b);

            var formatted = ((decimal)fixture.Record.DurationSeconds).ToString("G");

            // ansi codes are included between key and value
            output.Should().MatchRegex($"DurationSeconds.*=.*{formatted}");
        }
    }
}
