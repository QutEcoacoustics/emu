// <copyright file="TempFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
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

        public static TempFile DuplicateExisting(string path, string newName = null)
        {
            if (IO.File.Exists(path))
            {
                var basename = IO.Path.GetFileNameWithoutExtension(newName ?? path);
                var extension = IO.Path.GetExtension(newName ?? path);

                var temp = new TempFile(basename, extension);

                IO.File.Copy(path, temp.Path);

                return temp;
            }

            throw new ArgumentException("path must exist", nameof(path));
        }

        public static TempFile DuplicateExistingDirectory(string path)
        {
            var temp = DuplicateExisting(path);

            var fixtureDirectory = IO.Path.GetDirectoryName(path);
            var files = IO.Directory.EnumerateFiles(fixtureDirectory);

            foreach (var file in files)
            {
                if (file == path)
                {
                    // don't copy the source file which was already copied
                    // in DuplicateExisting
                    continue;
                }

                var dest = IO.Path.Combine(temp.directory, IO.Path.GetFileName(file));
                IO.File.Copy(file, dest);
            }

            return temp;
        }

        public void Dispose()
        {
            try
            {
                IO.File.Delete(this.Path);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("TEMP FILE DISPOSE: " + ex.ToString());
            }

            try
            {
                // this is important - especially on our CI server where space matters.
                // this will clean up any extra files in the directory and the directory itself.
                // TODO: myabe this was useful for local debugging? Maybe a keep temp files switch
                // might be useful?
                IO.Directory.Delete(this.directory, recursive: true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("TEMP FILE DISPOSE: " + ex.ToString());
            }
        }
    }
}
