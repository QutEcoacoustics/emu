// <copyright file="TempFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using IO = System.IO;

    public class TempFile : IDisposable
    {
        private static int counter = 0;

        private readonly string directory;

        public TempFile(string basename = null, string extension = null)
        {
            extension ??= ".tmp";
            basename ??= IO.Path.GetFileNameWithoutExtension(IO.Path.GetTempFileName());

            // minus extension
            var subDirectory = DateTime.Now.ToString("yyyyMMddTHHmmss") + $"_{counter++}";

            this.directory = IO.Path.Join(Helpers.TestTempRoot, subDirectory);
            this.Path = IO.Path.Join(this.directory, basename + extension);

            this.Directory.Create();
        }

        public IO.DirectoryInfo Directory => new(this.directory);

        public IO.FileInfo File => new(this.Path);

        public string Path { get; private set; }

        public static TempFile DuplicateExisting(string path)
        {
            if (IO.File.Exists(path))
            {
                var basename = IO.Path.GetFileNameWithoutExtension(path);
                var extension = IO.Path.GetExtension(path);
                var temp = new TempFile(basename, extension);

                IO.File.Copy(path, temp.Path);

                return temp;
            }

            throw new ArgumentException("path must exist", nameof(path));
        }

        public void Dispose()
        {
            try
            {
                IO.File.Delete(this.Path);
                if (!IO.Directory.EnumerateFiles(this.Path).Any())
                {
                    IO.Directory.Delete(this.directory);
                }
            }
            catch (IO.DirectoryNotFoundException dnf)
            {
                Console.Error.WriteLine(dnf.ToString());
            }
        }
    }
}
