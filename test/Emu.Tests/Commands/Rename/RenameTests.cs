// <copyright file="RenameTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands.Rename
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Emu.Commands.Rename;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class RenameTests : TestBase
    {
        private readonly Rename command;

        public RenameTests(ITestOutputHelper output)
            : base(output)
        {
            this.command = new Rename(
                this.BuildLogger<Rename>(),
                this.DryRunFactory,
                this.TestFiles,
                new FileMatcher(this.BuildLogger<FileMatcher>(), this.TestFiles),
                this.GetOutputRecordWriter(),
                this.FilenameParser);

            this.command.Targets = "/".AsArray();
        }

        [Fact]
        public async Task AutomaticallyUsesNewDateFormat()
        {
            this.TestFiles.AddEmptyFile("/5B07FAC0.WAV");

            var result = await this.command.InvokeAsync(null);

            result.Should().Be(0);

            this.TestFiles.AllFiles
                .Select(NormalizePath)
                .Should()
                .BeEquivalentTo(
                    this.ResolvePaths("/20180525T120000Z.WAV"));
        }

        [Fact]
        public async Task DryRunDoesNothing()
        {
            this.TestFiles.AddEmptyFile("/5B07FAC0.WAV");

            this.command.DryRun = true;

            var result = await this.command.InvokeAsync(null);

            result.Should().Be(0);

            this.TestFiles.AllFiles
                .Select(NormalizePath)
                .Should()
                .BeEquivalentTo(
                    this.ResolvePaths("/5B07FAC0.WAV"));
        }

        [Fact]
        public async Task AddOffsetToLocalDatestamp()
        {
            this.TestFiles.AddEmptyFile("/PILLIGA_20121204_234600.wav");

            this.command.Offset = Offset.FromHours(11);

            var result = await this.command.InvokeAsync(null);

            result.Should().Be(0);

            this.TestFiles.AllFiles
                .Select(NormalizePath)
                .Should()
                .BeEquivalentTo(
                    this.ResolvePaths("/PILLIGA_20121204T234600+1100.wav"));
        }

        [Fact]
        public async Task ChangingOffsets()
        {
            this.TestFiles.AddEmptyFile("/20210617T080000+0000_Rec2_-18.2656+144.5564.flac");
            this.TestFiles.AddEmptyFile("/20211004T200000+0000_Rec2_-18.1883+144.5414.flac");
            this.TestFiles.AddEmptyFile("/5B07FAC0.WAV");
            this.TestFiles.AddEmptyFile("/PILLIGA_20121204T234600+1100.wav");

            this.command.NewOffset = Offset.FromHours(10);

            var result = await this.command.InvokeAsync(null);

            result.Should().Be(0);

            this.TestFiles.AllFiles.Count().Should().Be(4);

            this.TestFiles.FileExists(this.ResolvePath("/20210617T180000+1000_Rec2_-18.2656+144.5564.flac")).Should().BeTrue();
            this.TestFiles.FileExists(this.ResolvePath("/20211005T060000+1000_Rec2_-18.1883+144.5414.flac")).Should().BeTrue();
            this.TestFiles.FileExists(this.ResolvePath("/20180525T220000+1000.WAV")).Should().BeTrue();
            this.TestFiles.FileExists(this.ResolvePath("/PILLIGA_20121204T224600+1000.wav")).Should().BeTrue();
        }

        [Fact]
        public async Task CopyTo()
        {
            this.TestFiles.AddEmptyFile("/PILLIGA_20121204_234600.wav");

            this.command.Offset = Offset.FromHours(11);
            this.command.CopyTo = this.ResolvePath("/copy-dest").ToDirectory();

            var result = await this.command.InvokeAsync(null);

            result.Should().Be(0);

            // original file not modified
            this.TestFiles.AllFiles
                .Select(NormalizePath)
                .Should()
                .BeEquivalentTo(
                    this.ResolvePaths(
                        "/PILLIGA_20121204_234600.wav",
                        "/copy-dest/PILLIGA_20121204T234600+1100.wav"));
        }

        [Fact]
        public async Task CopyToDryRun()
        {
            this.TestFiles.AddEmptyFile("/PILLIGA_20121204_234600.wav");

            this.command.Offset = Offset.FromHours(11);
            this.command.CopyTo = this.ResolvePath("/copy-dest").ToDirectory();
            this.command.DryRun = true;

            var result = await this.command.InvokeAsync(null);

            result.Should().Be(0);

            // original file not modified, no changes made
            this.TestFiles.AllFiles
                .Select(NormalizePath)
                .Should()
                .BeEquivalentTo(
                    this.ResolvePaths(
                        "/PILLIGA_20121204_234600.wav"));
        }

        [Fact]
        public async Task CopyToPreservesDirectoryStructure()
        {
            this.TestFiles.AddEmptyFile("/PILLIGA_20121204_234600.wav");
            this.TestFiles.AddEmptyFile("/a/PILLIGA_20121205_234600.wav");
            this.TestFiles.AddEmptyFile("/a/b/PILLIGA_20121206_234600.wav");
            this.TestFiles.AddEmptyFile("/z/PILLIGA_20121207_234600.wav");

            this.command.Offset = Offset.FromHours(11);
            this.command.CopyTo = this.ResolvePath("/copy-dest").ToDirectory();

            var result = await this.command.InvokeAsync(null);

            result.Should().Be(0);

            // original file not modified
            this.TestFiles.AllFiles
                .Select(NormalizePath)
                .OrderBy(x => x)
                .Should()
                .BeEquivalentTo(
                    this.ResolvePaths(
                        "/PILLIGA_20121204_234600.wav",
                        "/a/PILLIGA_20121205_234600.wav",
                        "/a/b/PILLIGA_20121206_234600.wav",
                        "/z/PILLIGA_20121207_234600.wav",
                        "/copy-dest/PILLIGA_20121204T234600+1100.wav",
                        "/copy-dest/a/PILLIGA_20121205T234600+1100.wav",
                        "/copy-dest/a/b/PILLIGA_20121206T234600+1100.wav",
                        "/copy-dest/z/PILLIGA_20121207T234600+1100.wav")
                    .OrderBy(x => x));
        }

        [Fact]
        public async Task Flatten()
        {
            this.TestFiles.AddEmptyFile("/PILLIGA_20121204_234600.wav");
            this.TestFiles.AddEmptyFile("/a/PILLIGA_20121205_234600.wav");
            this.TestFiles.AddEmptyFile("/a/b/PILLIGA_20121206_234600.wav");
            this.TestFiles.AddEmptyFile("/z/PILLIGA_20121207_234600.wav");

            this.command.Offset = Offset.FromHours(11);
            this.command.Flatten = true;

            var result = await this.command.InvokeAsync(null);

            result.Should().Be(0);

            // original file not modified
            this.TestFiles.AllFiles
                .Select(NormalizePath)
                .Should()
                .BeEquivalentTo(
                    this.ResolvePaths(
                        "/PILLIGA_20121204T234600+1100.wav",
                        "/PILLIGA_20121205T234600+1100.wav",
                        "/PILLIGA_20121206T234600+1100.wav",
                        "/PILLIGA_20121207T234600+1100.wav"));
        }

        [Fact]
        public async Task FlattenWontClobberFiles()
        {
            this.TestFiles.AddEmptyFile("/PILLIGA_20121204_234600.wav");
            this.TestFiles.AddEmptyFile("/a/PILLIGA_20121204_234600.wav");
            this.TestFiles.AddEmptyFile("/a/b/PILLIGA_20121204_234600.wav");
            this.TestFiles.AddEmptyFile("/z/PILLIGA_20121204_234600.wav");

            this.command.Offset = Offset.FromHours(11);
            this.command.Flatten = true;

            var result = await this.command.InvokeAsync(null);

            result.Should().Be(1);

            // original file not modified
            this.TestFiles.AllFiles
                .Select(NormalizePath)
                .Should()
                .BeEquivalentTo(
                    this.ResolvePaths(
                       "/PILLIGA_20121204_234600.wav",
                       "/a/PILLIGA_20121204_234600.wav",
                       "/a/b/PILLIGA_20121204_234600.wav",
                       "/z/PILLIGA_20121204_234600.wav"));
        }
    }
}
