// <copyright file="OutputSinkTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Cli
{
    using System;
    using System.IO.Abstractions.TestingHelpers;
    using System.Linq;
    using Emu.Cli;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class OutputSinkTests : TestBase
    {
        public OutputSinkTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void TestOutput()
        {
            this.TestFiles.AddDirectory("/abc");
            this.TestFiles.Directory.SetCurrentDirectory(this.ResolvePath("/abc"));

            var sink = new OutputSink(
                this.BuildLogger<OutputSink>(),
                new EmuGlobalOptions()
                {
                    Output = "output.txt",
                },
                this.TestFiles);

            sink.Create();

            // relative paths should resolve to the current working directory
            this.TestFiles.FileExists("/abc/output.txt").Should().BeTrue();
        }

        [Fact]
        public void TestOutputOverwriting()
        {
            this.TestFiles.AddEmptyFile("output.txt");

            var sink = new OutputSink(
                this.BuildLogger<OutputSink>(),
                new EmuGlobalOptions()
                {
                    Output = "output.txt",
                    Clobber = true,
                },
                this.TestFiles);

            sink.Create();

            this.TestFiles.FileExists("/output.txt").Should().BeTrue();
            var expectedPath = this.ResolvePath("/output.txt");
            this.Loggers.Single().Entries.Single().Message.Should().Be($"Overwriting {expectedPath} because --clobber was specified");
        }

        [Fact]
        public void TestOutputOverwritingWithoutClobber()
        {
            this.TestFiles.AddEmptyFile("output.txt");

            var sink = new OutputSink(
                this.BuildLogger<OutputSink>(),
                new EmuGlobalOptions()
                {
                    Output = "output.txt",
                },
                this.TestFiles);

            var action = () => sink.Create();

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("File exists and clobber not specified");
        }

        [Fact]
        public void TestOutputIntermediateDirectoryCreation()
        {
            var sink = new OutputSink(
                this.BuildLogger<OutputSink>(),
                new EmuGlobalOptions()
                {
                    Output = "/subdir/anotherdir/output.txt",
                },
                this.TestFiles);

            var beforeDirs = this.TestFiles.AllDirectories;

            sink.Create();

            var afterDirs = beforeDirs.Concat(new[]
            {
                "/subdir",
                "/subdir/anotherdir",
            }).Select(this.ResolvePath);

            this.TestFiles.AllDirectories.Should().Equal(afterDirs);
        }
    }
}
