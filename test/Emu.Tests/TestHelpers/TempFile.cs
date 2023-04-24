// <copyright file="TempFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using IO = System.IO;

    public class TempFile : IDisposable
    {
        private readonly TempDir tempDir;

        public TempFile(string basename = null, string extension = null, TempDir directory = null)
        {
            extension ??= ".tmp";
            basename ??= IO.Path.GetFileNameWithoutExtension(IO.Path.GetTempFileName());

            this.tempDir = directory ?? new TempDir();

            this.Name = basename + extension;

            this.tempDir.Add(this);
        }

        public string Name { get; }

        public IO.FileInfo File => new(this.Path);

        public IO.DirectoryInfo Directory => this.TempDir.Directory;

        public string Path => IO.Path.Join(this.TempDir.Path, this.Name);

        public TempDir TempDir => this.tempDir;

        public static TempFile DuplicateExisting(string path, string newName = null, TempDir tempDir = null)
        {
            if (IO.File.Exists(path))
            {
                var basename = IO.Path.GetFileNameWithoutExtension(newName ?? path);
                var extension = IO.Path.GetExtension(newName ?? path);

                var temp = new TempFile(basename, extension, tempDir);

                IO.File.Copy(path, temp.Path);

                return temp;
            }

            throw new ArgumentException("path must exist", nameof(path));
        }

        public void Dispose()
        {
            // temp dir tracks files and will delete all files inside it.
            this.TempDir.Dispose();
        }
    }
}
