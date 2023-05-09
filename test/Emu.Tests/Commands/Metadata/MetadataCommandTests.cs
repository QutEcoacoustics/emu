// <copyright file="MetadataCommandTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands.Metadata
{
    using System;
    using System.CommandLine.Parsing;
    using System.Linq;
    using System.Threading.Tasks;
    using Emu.Cli.ObjectFormatters;
    using Emu.Commands;
    using Emu.Commands.Metadata;
    using Emu.Commands.Metadata.Dump;
    using Emu.Metadata;
    using Emu.Metadata.SupportFiles;
    using Emu.Models;
    using Emu.Serialization;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    using static Emu.EmuCommand;

    public partial class MetadataCommandTests
        : TestBase
    {
        private readonly Metadata command;
        private readonly JsonLinesSerializer serializer;

        public MetadataCommandTests(ITestOutputHelper output)
            : base(output, realFileSystem: false, outputFormat: OutputFormat.JSONL)
        {
            this.command = new Metadata(
                this.BuildLogger<Metadata>(),
                this.TestFiles,
                new FileMatcher(this.BuildLogger<FileMatcher>(), this.TestFiles),
                new SupportFileFinder(this.BuildLogger<SupportFileFinder>(), this.TestFiles),
                this.GetOutputRecordWriter(),
                new MetadataRegister(this.ServiceProvider),
                new PrettyFormatter(),
                new CompactFormatter())
            {
                Targets = "/".AsArray(),
            };

            this.serializer = this.ServiceProvider.GetRequiredService<JsonLinesSerializer>();
        }

        [Fact]
        public void SupportsTargets()
        {
            var command = "metadata **/*.wav";

            var result = this.CliParser.Parse(command);

            Assert.True(result.Errors.Count == 0);

            var commandResult = result.CommandResult.Command;

            Assert.IsType<MetadataCommand>(commandResult);

            Assert.Equal(
                result.FindResultFor(Common.Targets).GetValueOrDefault<string[]>(),
                "**/*.wav".AsArray());

            result.UnmatchedTokens.Should().BeEmpty();
            result.UnparsedTokens.Should().BeEmpty();
        }

        [Fact]
        public async Task CanBeInvoked()
        {
            var command = "metadata **/*.wav";

            var parseResult = this.CliParser.Parse(command);

            Assert.True(parseResult.Errors.Count == 0);

            var result = await parseResult.InvokeAsync();
            result.Should().Be(0);
        }

        [Fact]
        public void HasAMetadataCommandThatComplainsIfNoArgumentsAreGiven()
        {
            var command = "metadata";

            var parser = this.CliParser;
            var result = parser.Parse(command);

            Assert.Equal(1, result.Errors.Count);
            Assert.Equal(
                "Required argument missing for command: metadata",
                result.Errors.First().Message);

            var errorLevel = result.Invoke();

            Assert.Equal(1, errorLevel);
        }

        [Fact]
        public async Task MetadataCommandOutputsOneRecordPerFile()
        {
            this.TestFiles.AddEmptyFile("/a.WAV");
            this.TestFiles.AddEmptyFile("/b.WAV");
            this.TestFiles.AddEmptyFile("/c.WAV");

            var result = await this.command.InvokeAsync(null);

            result.Should().Be(0);

            var recordings = this.serializer.Deserialize<Recording>(this.GetAllOutputReader()).ToArray();

            Assert.Equal(3, recordings.Length());
            Assert.Contains("a.WAV", recordings[0].Name);
            Assert.Contains("b.WAV", recordings[1].Name);
            Assert.Contains("c.WAV", recordings[2].Name);
        }

        [Fact]
        public async Task NoChecksumWorks()
        {
            this.TestFiles.AddEmptyFile("/a.WAV");
            this.TestFiles.AddEmptyFile("/b.WAV");

            this.command.NoChecksum = true;
            await this.command.InvokeAsync(null);

            var recordings = this.serializer.Deserialize<Recording>(this.GetAllOutputReader());

            foreach (var recording in recordings)
            {
                Assert.Null(recording.CalculatedChecksum);
            }
        }

        public partial class SmokeTest : TestBase, IClassFixture<FixtureData>
        {
            private readonly FixtureData data;

            public SmokeTest(ITestOutputHelper output, FixtureData data)
                : base(output, true, OutputFormat.Default)
            {
                this.data = data;
            }

            [Theory]
            [InlineData(OutputFormat.Default, FixtureModel.NormalFile)]
            [InlineData(OutputFormat.CSV, FixtureModel.NormalFile)]
            [InlineData(OutputFormat.Compact, FixtureModel.NormalFile)]
            [InlineData(OutputFormat.JSON, FixtureModel.NormalFile)]
            [InlineData(OutputFormat.JSONL, FixtureModel.NormalFile)]
            [InlineData(OutputFormat.Default, FixtureModel.NormalSm3)]
            [InlineData(OutputFormat.CSV, FixtureModel.NormalSm3)]
            [InlineData(OutputFormat.Compact, FixtureModel.NormalSm3)]
            [InlineData(OutputFormat.JSON, FixtureModel.NormalSm3)]
            [InlineData(OutputFormat.JSONL, FixtureModel.NormalSm3)]
            [InlineData(OutputFormat.Default, FixtureModel.Sm4HighPrecision)]
            [InlineData(OutputFormat.CSV, FixtureModel.Sm4HighPrecision)]
            [InlineData(OutputFormat.Compact, FixtureModel.Sm4HighPrecision)]
            [InlineData(OutputFormat.JSON, FixtureModel.Sm4HighPrecision)]
            [InlineData(OutputFormat.JSONL, FixtureModel.Sm4HighPrecision)]
            [InlineData(OutputFormat.Default, FixtureModel.Audiomoth180)]
            [InlineData(OutputFormat.CSV, FixtureModel.Audiomoth180)]
            [InlineData(OutputFormat.Compact, FixtureModel.Audiomoth180)]
            [InlineData(OutputFormat.JSON, FixtureModel.Audiomoth180)]
            [InlineData(OutputFormat.JSONL, FixtureModel.Audiomoth180)]
            public async Task EachFormatterWorks(OutputFormat format, string fixtureName)
            {
                var command = new Metadata(
                    this.BuildLogger<Metadata>(),
                    this.CurrentFileSystem,
                    new FileMatcher(this.BuildLogger<FileMatcher>(), this.CurrentFileSystem),
                    new SupportFileFinder(this.BuildLogger<SupportFileFinder>(), this.CurrentFileSystem),
                    new OutputRecordWriter(
                        this.Sink,
                        OutputRecordWriter.ChooseFormatter(this.ServiceProvider, format),
                        new Lazy<OutputFormat>(format)),
                    new MetadataRegister(this.ServiceProvider),
                    new PrettyFormatter(),
                    new CompactFormatter())
                {
                };

                var fixture = this.data[fixtureName];
                command.Targets = fixture.AbsoluteFixturePath.AsArray();
                command.NoChecksum = true;

                var result = await command.InvokeAsync(null);

                result.Should().Be(0);

                var output = this.AllOutput;

                var path = format is OutputFormat.JSON or OutputFormat.JSONL ? fixture.EscapedAbsoluteFixturePath : fixture.AbsoluteFixtureDirectory;
                output.Should().Contain(path);

                var formatted = ((decimal)fixture.Record.DurationSeconds).ToString("G");
                var expected = format switch
                {
                    OutputFormat.Default => $"DurationSeconds = {formatted}",
                    OutputFormat.Compact => $"DurationSeconds={formatted};",
                    OutputFormat.CSV => $"{formatted}",
                    OutputFormat.JSON => $"\"DurationSeconds\": {formatted}",
                    OutputFormat.JSONL => $"\"DurationSeconds\":{formatted}",
                    _ => throw new NotImplementedException(),
                };

                output.Should().Contain(expected);
            }
        }
    }
}
