// <copyright file="OutputSinkTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Cli
{
    using MetadataUtility.Utilities;
    using MetadataUtility.Serialization;
    using System;
    using System.IO;
    using System.Text;
    using TestHelpers;
    using Xunit;
    using Xunit.Abstractions;

    public class OutputSinkTests : TestBase
    {

        private readonly MetadataUtility.Commands.Version.Version command;
        private StringWriter sw;

        public OutputSinkTests(ITestOutputHelper output)
            : base(output)
        {
            sw = new StringWriter();

            this.command = new MetadataUtility.Commands.Version.Version(
                new OutputRecordWriter(sw, new ToStringFormatter(this.BuildLogger<ToStringFormatter>())));

        }

        [Fact]
        public async void testOutput()
        {

            FixtureHelper.TestTempDir tempDir = new FixtureHelper.TestTempDir();
            string path = Path.Join(tempDir.TempDir, "out.txt");

            var result = await EmuEntry.Main(new string[] { "version", "-O", path });

            Assert.Equal(result, 0);

            await this.command.InvokeAsync(null);

            Assert.Equal(sw.ToString(), File.ReadAllText(path));

            tempDir.Dispose();
        }

        [Fact]
        public async void testOutputOverwriting()
        {

            FixtureHelper.TestTempDir tempDir = new FixtureHelper.TestTempDir();
            string path = Path.Join(tempDir.TempDir, "out.txt");

            File.WriteAllLines(path, new string[] { "test" });

            var result = await EmuEntry.Main(new string[] { "version", "-O", path });

            Assert.Equal(result, 1);

            result = await EmuEntry.Main(new string[] { "version", "-O", path, "-C" });

            Assert.Equal(result, 0);

            tempDir.Dispose();
        }

    }
}
