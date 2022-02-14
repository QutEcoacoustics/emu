// <copyright file="TempDir.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System;
    using IO = System.IO;

    public class TempDir : IDisposable
    {
        private readonly string directory;

        public TempDir()
        {
            // minus extension
            var subDirectory = DateTime.Now.ToString("yyyyMMddTHHmmss");

            this.directory = IO.Path.Join(Helpers.TestTempRoot, subDirectory);

            this.Directory.Create();
        }

        public IO.DirectoryInfo Directory => new(this.directory);

        public string Path => this.directory;

        public void Dispose()
        {
            try
            {
                IO.Directory.Delete(this.directory, recursive: true);
            }
            catch (IO.DirectoryNotFoundException dnf)
            {
                Console.Error.WriteLine(dnf.ToString());
            }
        }
    }
}
