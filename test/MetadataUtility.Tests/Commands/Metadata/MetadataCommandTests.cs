// <copyright file="MetadataCommandTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Commands.Metadata
{
    using System;
    using System.CommandLine.Parsing;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using MetadataUtility.Commands.Metadata;
    using MetadataUtility.Metadata;
    using MetadataUtility.Serialization;
    using MetadataUtility.Tests.TestHelpers;
    using MetadataUtility.Utilities;
    using Xunit;
    using Xunit.Abstractions;

    public class MetadataCommandTests : TestBase
    {
        private readonly Metadata command;
        private StringWriter writer;

        public MetadataCommandTests(ITestOutputHelper output)
            : base(output)
        {
            this.writer = new StringWriter();

            this.command = new Metadata(
                this.BuildLogger<Metadata>(),
                this.TestFiles,
                new FileMatcher(this.BuildLogger<FileMatcher>(), this.TestFiles),
                new OutputRecordWriter(this.writer, new JsonLinesSerializer()),
                new MetadataRegister(this.ServiceProvider));                 // TODO: BROKEN!

            this.command.Targets = "/".AsArray();
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
        public async void MetadataCommandOutputsOneRecordPerFile()
        {
            this.TestFiles.AddEmptyFile("/a.WAV");
            this.TestFiles.AddEmptyFile("/b.WAV");
            this.TestFiles.AddEmptyFile("/c.WAV");

            var result = await this.command.InvokeAsync(null);

            result.Should().Be(0);

            string[] lines = this.writer.ToString().Split("\n").Where(s => (s.Length() > 0 && s[0] == '{')).ToArray();

            Assert.Equal(3, lines.Length());
            Assert.Contains("a.WAV", lines[0]);
            Assert.Contains("b.WAV", lines[1]);
            Assert.Contains("c.WAV", lines[2]);
        }
    }
}
