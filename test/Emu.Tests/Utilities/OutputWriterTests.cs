// <copyright file="OutputWriterTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Utilities
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Emu.Models;
    using Emu.Serialization;
    using Emu.Tests.TestHelpers.Fakes;
    using Emu.Utilities;
    using Xunit;

    public class OutputWriterTests : IClassFixture<Fakes>
    {
        private readonly Fakes fakes;

        public OutputWriterTests(Fakes fakes)
        {
            this.fakes = fakes;
        }

        [Fact]
        public void OutputWriterWriteJson()
        {
            var stringBuilder = new StringBuilder(4);
            using var stringWriter = new StringWriter(stringBuilder);
            var jsonSerializer = new JsonSerializer();

            var output = new OutputRecordWriter(stringWriter, jsonSerializer);

            // generate a fake and write it to the stream
            var a = this.fakes.Recording.Generate();
            output.Write(a);

            // check the header was written
            var actual = stringBuilder.ToString();
            Assert.StartsWith($"[{Environment.NewLine}", actual);
            Assert.EndsWith("}", actual);

            // generate and write another fake
            var b = this.fakes.Recording.Generate();
            output.Write(b);

            // finish writing
            output.Dispose();

            actual = stringBuilder.ToString();
            Assert.StartsWith($"[{Environment.NewLine}", actual);
            Assert.EndsWith("]", actual);

            var records = jsonSerializer.Deserialize<Recording>(new StringReader(actual)).ToArray();

            Assert.Equal(records[0].SourcePath, a.SourcePath);
            Assert.Equal(records[1].SourcePath, b.SourcePath);
        }

        [Fact]
        public void OutputWriterWriteCsv()
        {
            var stringBuilder = new StringBuilder(4);
            using var stringWriter = new StringWriter(stringBuilder);
            var csvSerializer = new CsvSerializer();

            var output = new OutputRecordWriter(stringWriter, csvSerializer);

            // generate a fake and write it to the stream
            var a = this.fakes.Recording.Generate();
            output.Write(a);

            // check the header was written
            var actual = stringBuilder.ToString();
            Assert.StartsWith($"{nameof(Recording.SourcePath)},", actual);
            Assert.Contains(a.ExpectedDurationSeconds?.TotalSeconds.ToString(), actual);

            // generate and write another fake
            var b = this.fakes.Recording.Generate();
            output.Write(b);

            // finish writing
            output.Dispose();

            actual = stringBuilder.ToString();
            Assert.StartsWith($"{nameof(Recording.SourcePath)},", actual);
            Assert.Single(Regex.Matches(actual, $"{nameof(Recording.SourcePath)},"));
            Assert.Contains(a.ExpectedDurationSeconds?.TotalSeconds.ToString(), actual);

            var records = csvSerializer.Deserialize<Recording>(new StringReader(actual)).ToArray();

            Assert.Equal(records[0].SourcePath, a.SourcePath);
            Assert.Equal(records[1].SourcePath, b.SourcePath);
        }
    }
}
