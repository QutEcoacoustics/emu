// <copyright file="TestTempDir.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    public class TestTempDir : IDisposable
    {
        [SuppressMessage(
            "System.IO.Abstractions",
            "IO0006:Replace Path class with IFileSystem.Path for improved testability",
            Justification = "Only deals with physical file system")]
        [SuppressMessage(
            "System.IO.Abstractions",
            "IO0003:Replace Directory class with IFileSystem.Directory for improved testability",
            Justification = "Only deals with physical file system")]

        public TestTempDir()
        {
            var basename = Path.GetRandomFileName();

            this.TempDir = Path.Join(Directory.GetCurrentDirectory(), basename);

            Directory.CreateDirectory(this.TempDir);
        }

        ~TestTempDir()
        {
            this.Dispose();
        }

        public string TempDir { get; }

        public void Dispose()
        {
            try
            {
#pragma warning disable IO0003 // Replace Directory class with IFileSystem.Directory for improved testability
                Directory.Delete(this.TempDir, recursive: true);
#pragma warning restore IO0003 // Replace Directory class with IFileSystem.Directory for improved testability
            }
            catch (System.IO.DirectoryNotFoundException dnf)
            {
                Console.Error.WriteLine(dnf.ToString());
            }
        }
    }
}
