// <copyright file="Extensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System.IO.Abstractions.TestingHelpers;

    public static class Extensions
    {
        public static void AddEmptyFile(this MockFileSystem fileSystem, string path)
        {
            fileSystem.AddFile(path, string.Empty);
        }
    }
}
