// <copyright file="FixtureHelperTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Xunit;

    public class FixtureHelperTests
    {
        [Fact]
        public void ResolvePathWithArchiveFallbackUsesOriginalPathWhenPresent()
        {
            var fileSystem = FixtureHelper.RealFileSystem;
            var root = fileSystem.Path.Combine(Helpers.TestTempRoot, "fixture-helper-tests", Guid.NewGuid().ToString("N"));
            fileSystem.Directory.CreateDirectory(root);

            try
            {
                var expectedPath = fileSystem.Path.Combine(root, "present.wav");
                fileSystem.File.WriteAllBytes(expectedPath, new byte[] { 1, 2, 3 });

                var resolved = FixtureHelper.ResolvePathWithArchiveFallback(expectedPath);

                resolved.Should().Be(expectedPath);
            }
            finally
            {
                fileSystem.Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void ResolvePathWithArchiveFallbackShowsUsefulErrorWhenArchiveExists()
        {
            var fileSystem = FixtureHelper.RealFileSystem;
            var root = fileSystem.Path.Combine(Helpers.TestTempRoot, "fixture-helper-tests", Guid.NewGuid().ToString("N"));
            fileSystem.Directory.CreateDirectory(root);

            try
            {
                var expectedPath = fileSystem.Path.Combine(root, "sample.wav");
                var archivePath = expectedPath + ".zip";
                var originalBytes = new byte[] { 10, 20, 30, 40, 50 };

                using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
                {
                    var entry = archive.CreateEntry("sample.wav");
                    using var stream = entry.Open();
                    stream.Write(originalBytes, 0, originalBytes.Length);
                }

                var action = () => FixtureHelper.ResolvePathWithArchiveFallback(expectedPath);
                var error = action.Should().Throw<FileNotFoundException>().Which;

                using (new AssertionScope())
                {
                    error.Message.Should().Contain(archivePath);
                    error.Message.Should().Contain("dotnet test");
                    error.Message.Should().Contain("no-build");
                }

                fileSystem.File.Exists(expectedPath).Should().BeFalse();
            }
            finally
            {
                fileSystem.Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void ResolvePathWithArchiveFallbackThrowsWhenFileAndArchiveMissing()
        {
            var fileSystem = FixtureHelper.RealFileSystem;
            var root = fileSystem.Path.Combine(Helpers.TestTempRoot, "fixture-helper-tests", Guid.NewGuid().ToString("N"));
            fileSystem.Directory.CreateDirectory(root);

            try
            {
                var expectedPath = fileSystem.Path.Combine(root, "expected.wav");
                var action = () => FixtureHelper.ResolvePathWithArchiveFallback(expectedPath);
                var error = action.Should().Throw<FileNotFoundException>().Which;

                error.Message.Should().Contain(expectedPath);
            }
            finally
            {
                fileSystem.Directory.Delete(root, recursive: true);
            }
        }
    }
}
