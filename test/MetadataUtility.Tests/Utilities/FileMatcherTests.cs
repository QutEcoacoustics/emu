// <copyright file="FileMatcherTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using MetadataUtility.Tests.TestHelpers;
    using MetadataUtility.Utilities;
    using Xunit;

    public class FileMatcherTests : IClassFixture<FileMatcherTests.FileMatcherFixture>
    {
        private readonly FileMatcherFixture fileMatcherFixture;
        private readonly FileMatcher matcher;

        public FileMatcherTests(FileMatcherFixture fileMatcherFixture)
        {
            this.fileMatcherFixture = fileMatcherFixture;
            this.matcher = new FileMatcher(null);
        }

        // I'm using the '@' character to denote an absolute path to the fixtures directory
        [Theory]
        [InlineData("@/a.wav", "@/a.wav")]
        [InlineData("k/../a.wav", "@/a.wav")]
        [InlineData("a.wav", "@/a.wav")]
        [InlineData("@/*.wav", "@/a.wav;@/b.wav;@/c.wav")]
        [InlineData("*.wav", "@/a.wav;@/b.wav;@/c.wav")]
        [InlineData("d/**/*.wav", "@/d/e/f/g.wav;@/d/e/f/h.wav;@/d/e/f/i.wav")]
        [InlineData("@/d/**/*.wav", "@/d/e/f/g.wav;@/d/e/f/h.wav;@/d/e/f/i.wav")]
        [InlineData("d/../**/*.flac", "@/j.flac;@/k/l.flac;@/k/m.flac")]
        [InlineData("@/d/../**/*.flac", "@/j.flac;@/k/l.flac;@/k/m.flac")]
        public void TestExpansion(string glob, string expected)
        {
            var fullGlob = glob.Replace("@", this.fileMatcherFixture.TempDir);
            var expectedPaths = expected.Replace("@", this.fileMatcherFixture.TempDir).Split(";");

            var actualPaths = this.matcher
                .ExpandMatches(this.fileMatcherFixture.TempDir, fullGlob.AsSequence())
                .ToArray();

            Assert.Equal(expectedPaths, actualPaths);
        }

        public class FileMatcherFixture : FixtureHelper.TempDirFixture
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
                };

                foreach (var file in this.MockFiles)
                {
                    var path = Path.Join(this.TempDir, file);
                    var directory = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(directory);
                    File.Create(path).Close();
                }
            }

            public string[] MockFiles { get; }
        }
    }
}
