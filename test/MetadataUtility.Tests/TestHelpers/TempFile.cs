// <copyright file="TempFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System;
    using System.IO;
    using System.Linq;
    using static System.IO.Path;

    public class TempFile : IDisposable
    {
        private readonly string directory;

        public static TempFile FromExisting(string path)
        {
            if (System.IO.File.Exists(path))
            {
                var basename = GetFileNameWithoutExtension(path);
                var extension = GetExtension(path);
                var temp = new TempFile(basename, extension);

                System.IO.File.Copy(path, temp.Path);

                return temp;
            }

            throw new ArgumentException("path must exist", nameof(path));
        }

        public TempFile(string basename = null, string extension = null)
        {
            extension ??= ".tmp";
            basename ??= GetTempFileName();

            // minus extension
            var subDirectory = DateTime.Now.ToString("yyyyMMddTHHmmss");

            this.directory = Join(Helpers.TestTempRoot, subDirectory);
            this.Path = Join(this.directory, basename + extension);

            this.Directory.Create();
        }

        public DirectoryInfo Directory => new(this.directory);

        public FileInfo File => new(this.Path);

        public string Path { get; private set; }

        public void Dispose()
        {
            try
            {
                System.IO.File.Delete(this.Path);
                if (!System.IO.Directory.EnumerateFiles(this.Path).Any())
                {
                    System.IO.Directory.Delete(this.directory);
                }
            }
            catch (DirectoryNotFoundException dnf)
            {
                Console.Error.WriteLine(dnf.ToString());
            }
        }
    }
}
