// <copyright file="EmuCommandTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands
{
    using System.CommandLine.Parsing;
    using System.Linq;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class EmuCommandTests : TestBase
    {
        public EmuCommandTests(ITestOutputHelper output)
            : base(output, realFileSystem: true)
        {
        }

        [Fact]
        public void OutputWillNotClobber()
        {
            using var temp = new TempFile();

            // touch the file
            temp.File.OpenWrite().Dispose();

            var command = $"version -O {temp.Path}";

            var parser = this.CliParser;

            var result = parser.Parse(command);

            result.Errors.Should().HaveCount(1);
            result.Errors.Single().Message.Should().Be(
                $"Will not overwrite existing output file {temp.File.FullName}, use --clobber option or select a different name");
        }
    }
}
