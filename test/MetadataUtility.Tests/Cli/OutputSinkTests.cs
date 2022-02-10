// <copyright file="OutputSinkTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Cli
{
    using MetadataUtility.Commands.Version;
    using MetadataUtility.Utilities;
    using MetadataUtility.Serialization;
    using System.IO;
    using TestHelpers;
    using Xunit;
    using Xunit.Abstractions;

    public class OutputSinkTests : TestBase
    {

        private readonly Version command;
        private readonly StringWriter writer;

        public OutputSinkTests(ITestOutputHelper output)
            : base(output)
        {
            writer = new StringWriter();

            this.command = new Version(
                new OutputRecordWriter(writer, new ToStringFormatter(this.BuildLogger<ToStringFormatter>())));

        }

        [Fact]
        public async void TestOutput()
        {

            using var tempDir = new FixtureHelper.TestTempDir();
            string path = Path.Join(tempDir.TempDir, "out.txt");

            var result = await EmuEntry.Main(new string[] { "version", "-O", path });

            Assert.Equal(result, 0);

            await this.command.InvokeAsync(null);

            Assert.Equal(writer.ToString(), File.ReadAllText(path));
        }

        [Fact]
        public async void TestOutputOverwriting()
        {

            using var tempDir = new FixtureHelper.TestTempDir();
            string path = Path.Join(tempDir.TempDir, "out.txt");

            File.WriteAllText(path, "test");

            var result = await EmuEntry.Main(new string[] { "version", "-O", path });

            Assert.Equal(result, 1);

            result = await EmuEntry.Main(new string[] { "version", "-O", path, "-C" });

            Assert.Equal(result, 0);
        }

    }
}
