// <copyright file="Extensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System.IO.Abstractions.TestingHelpers;
    using System.Text.RegularExpressions;

    public static class Extensions
    {
        public static void AddEmptyFile(this MockFileSystem fileSystem, string path)
        {
            fileSystem.AddFile(path, string.Empty);
        }

        public static string ToCrLf(string str)
        {
            return Regex.Replace(str, @"\r\n|\n\r|\n|\r", "\r\n");
        }
    }
}
