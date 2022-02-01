// <copyright file="MetadataCommandTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Commands.Metadata
{
    using FluentAssertions;
    using MetadataUtility.Commands.Metadata;
    using MetadataUtility.Tests.TestHelpers;
    using MetadataUtility.Utilities;
    using MetadataUtility.Serialization;
    using System;
    using System.IO;
    using System.CommandLine.Parsing;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class MetadataCommandTests : TestBase
    {
        private readonly Metadata command;
        private StringWriter sw;

        public MetadataCommandTests(ITestOutputHelper output)
            : base(output)
        {
            this.sw = new StringWriter();

            this.command = new Metadata(
                this.BuildLogger<Metadata>(),
                this.TestFiles,
                new FileMatcher(this.BuildLogger<FileMatcher>(), this.TestFiles),
                new OutputRecordWriter(sw, new JsonLinesSerializer()));

            this.command.Targets = "/".AsArray();
        }
        [Fact]
        public void HasAMetadataCommandThatComplainsIfNoArgumentsAreGiven()
        {
            var command = "metadata";

            var parser = EmuEntry.BuildCommandLine();
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

            string[] lines = sw.ToString().Split("\n").Where(s => (s.Length() > 0 && s[0] == '{')).ToArray();

            Assert.Equal(lines.Length(), 3);
            Assert.True(lines[0].Contains("a.WAV"));
            Assert.True(lines[1].Contains("b.WAV"));
            Assert.True(lines[2].Contains("c.WAV"));
        }
    }
}
