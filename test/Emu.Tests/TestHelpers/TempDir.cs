// <copyright file="TempDir.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using IO = System.IO;

    public class TempDir : IDisposable
    {
        private static int counter = 0;

        private readonly string path;

        private readonly List<TempFile> knownFiles = new();

        public TempDir()
        {
            var subDirectory = DateTime.Now.ToString("yyyyMMddTHHmmss") + $"_{counter++}";

            this.path = IO.Path.Join(Helpers.TestTempRoot, subDirectory);

            this.Directory.Create();
        }

        public string Path => this.path;

        public IO.DirectoryInfo Directory => new(this.path);

        public static TempDir DuplicateExistingDirectory(string path)
        {
            if (!IO.Directory.Exists(path))
            {
                throw new ArgumentException("directory does not exist", nameof(path));
            }

            var files = IO.Directory.EnumerateFiles(path);

            var dir = new TempDir();
            foreach (var file in files)
            {
                TempFile.DuplicateExisting(file, tempDir: dir);
            }

            return dir;
        }

        public TempFile Add(string basename = null, string extension = null)
        {
           return this.Add(new TempFile(basename, extension, this));
        }

        public TempFile Add(TempFile file)
        {
            if (file.TempDir != this)
            {
                throw new ArgumentException("Cannot add a temp file that does not belong to the same temp dir");
            }

            this.knownFiles.Add(file);

            return file;
        }

        public TempFile CopyInExisiting(string path, string newName = null)
        {
            return TempFile.DuplicateExisting(path, newName, this);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            try
            {
                foreach (var file in this.knownFiles)
                {
                    IO.File.Delete(file.Path);
                }
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
                IO.Directory.Delete(this.path, recursive: true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("TEMP FILE DISPOSE: " + ex.ToString());
            }
        }
    }
}
