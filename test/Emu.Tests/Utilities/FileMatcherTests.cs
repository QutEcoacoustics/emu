// <copyright file="FileMatcherTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Utilities
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using Xunit;
    using static Emu.Tests.TestHelpers.Helpers;

    public class FileMatcherTests : IClassFixture<FileMatcherTests.FileMatcherFixture>
    {
        private readonly FileMatcherFixture fileMatcherFixture;
        private readonly FileMatcher matcher;

        public FileMatcherTests(FileMatcherFixture fileMatcherFixture)
        {
            this.fileMatcherFixture = fileMatcherFixture;
            this.matcher = new FileMatcher(NullLogger<FileMatcher>(), this.fileMatcherFixture.FileSystem);
        }

        // I'm using the '@' character to denote an absolute path to the fixtures directory
        [Theory]
        [InlineData("@/a.wav", "@/a.wav", "@")]
        [InlineData("k/../a.wav", "@/a.wav", "@")]
        [InlineData("a.wav", "@/a.wav", "@")]
        [InlineData("@/*.wav", "@/a.wav;@/b.wav;@/c.wav", "@")]
        [InlineData("*.wav", "@/a.wav;@/b.wav;@/c.wav", "@")]
        [InlineData("d/**/*.wav", "@/d/e/f/g.wav;@/d/e/f/h.wav;@/d/e/f/i.wav", "@/d")]
        [InlineData("@/d/**/*.wav", "@/d/e/f/g.wav;@/d/e/f/h.wav;@/d/e/f/i.wav", "@/d")]
        [InlineData("d/../**/*.flac", "@/j.flac;@/k/l.flac;@/k/m.flac", "@")]
        [InlineData("@/d/../**/*.flac", "@/j.flac;@/k/l.flac;@/k/m.flac", "@")]
        [InlineData("k", "@/k/l.flac;@/k/m.flac", "@/k")]
        [InlineData("*", "@/a.wav;@/b.wav;@/c.wav;@/j.flac", "@")]
        [InlineData("@/*", "@/a.wav;@/b.wav;@/c.wav;@/j.flac", "@")]
        public void TestExpansion(string glob, string expected, string expectedBase)
        {
            var fullGlob = this.fileMatcherFixture.Resolve(glob);
            var expectedPaths = this.fileMatcherFixture
                .Resolve(expected)
                .Split(";", StringSplitOptions.RemoveEmptyEntries);
            expectedBase = this.fileMatcherFixture
                .Resolve(expectedBase);

            var actualPaths = this.matcher
                .ExpandMatches(this.fileMatcherFixture.TempDir, fullGlob.AsEnumerable())
                .ToArray();

            actualPaths.Select(x => x.File).Should().BeEquivalentTo(expectedPaths);
            actualPaths.Select(x => x.Base).Should().AllBe(expectedBase);
        }

        [Fact]
        public void TestNoResults()
        {
            var actual = this.matcher.ExpandMatches(this.fileMatcherFixture.TempDir, "Z".AsEnumerable());

            actual.Should().BeEmpty();
        }

        public class FileMatcherFixture : TestTempDir
        {
            public FileMatcherFixture()
            {
                this.MockFiles = new[]
                {
                    "a.wav",
                    "b.wav",
                    "c.wav",
                    "d/e/f/g.wav",
                    "d/e/f/h.wav",
                    "d/e/f/i.wav",
                    "j.flac",
                    "k/l.flac",
                    "k/m.flac",
                }
                    .Select(file => this.FileSystem.Path.Join(this.TempDir, file).Touch(this.FileSystem))
                    .ToArray();
            }

            public string[] MockFiles { get; }

            public FileSystem FileSystem { get; } = new FileSystem();

            public string Resolve(string path)
            {
                return path.Replace("@", this.TempDir).Replace('/', this.FileSystem.Path.DirectorySeparatorChar);
            }
        }
    }
}
