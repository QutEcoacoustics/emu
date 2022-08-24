// <copyright file="MetadataCommandTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands.Metadata
{
    using System;
    using System.CommandLine.Parsing;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Emu.Commands.Metadata;
    using Emu.Metadata;
    using Emu.Models;
    using Emu.Serialization;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Xunit;
    using Xunit.Abstractions;

    using static Emu.EmuCommand;

    public class MetadataCommandTests : TestBase
    {
        private readonly Metadata command;
        private JsonLinesSerializer serializer;

        public MetadataCommandTests(ITestOutputHelper output)
            : base(output, realFileSystem: false, outputFormat: OutputFormat.JSONL)
        {
            this.command = new Metadata(
                this.BuildLogger<Metadata>(),
                this.TestFiles,
                new FileMatcher(this.BuildLogger<FileMatcher>(), this.TestFiles),
                this.GetOutputRecordWriter(),
                new MetadataRegister(this.ServiceProvider));

            this.command.Targets = "/".AsArray();

            this.serializer = this.ServiceProvider.GetRequiredService<JsonLinesSerializer>();
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

        public class SmokeTest : TestBase
        {
            private readonly Metadata command;

            public SmokeTest(ITestOutputHelper output)
                : base(output, true, OutputFormat.Default)
            {
                this.command = new Metadata(
                   this.BuildLogger<Metadata>(),
                   this.CurrentFileSystem,
                   new FileMatcher(this.BuildLogger<FileMatcher>(), this.CurrentFileSystem),
                   this.GetOutputRecordWriter(),
                   new MetadataRegister(this.ServiceProvider))
                {
                };
            }

            [Fact]
            public async Task TheDefaultFormatterWorks()
            {
                this.command.Targets = FixtureHelper.FixtureData.Get(FixtureModel.NormalFile).AbsoluteFixturePath.AsArray();

                var result = await this.command.InvokeAsync(null);

                result.Should().Be(0);

                var output = this.AllOutput;

                output.Split(Environment.NewLine).Length.Should().BeGreaterThan(20);
                output.Should().MatchRegex(new Regex(".*DurationSeconds.*= 7194.749387755102.*"));
            }
        }
    }
}
