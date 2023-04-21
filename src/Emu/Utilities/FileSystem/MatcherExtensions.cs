// <copyright file="MatcherExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Utilities.FileSystem
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using Microsoft.Extensions.FileSystemGlobbing;

    public static class MatcherExtensions
    {
        /// <summary>
        /// Searches the directory specified for all files matching patterns added to this instance of <see cref="Matcher" />.
        /// </summary>
        /// <param name="matcher">The matcher.</param>
        /// <param name="fileSystem">The file system to use.</param>
        /// <param name="directoryPath">The root directory for the search.</param>
        /// <returns>Absolute file paths of all files matched. Empty enumerable if no files matched given patterns.</returns>
        /// <remarks>
        /// This is a direct rip off of https://github.com/dotnet/runtime/blob/1d9e50cb4735df46d3de0cee5791e97295eaf588/src/libraries/Microsoft.Extensions.FileSystemGlobbing/src/MatcherExtensions.cs#L52-L58
        /// adapted to work with an IFileSystem.
        /// </remarks>
        public static IEnumerable<string> GetResultsInFullPath(this Matcher matcher, IFileSystem fileSystem, string directoryPath)
        {
            var directory = fileSystem.DirectoryInfo.New(directoryPath);
            var wrapper = new DirectoryInfoBaseAbstractionAdapter(directory);
            IEnumerable<FilePatternMatch> matches = matcher.Execute(wrapper).Files;
            string[] result = matches.Select(match => fileSystem.Path.GetFullPath(fileSystem.Path.Combine(directoryPath, match.Path))).ToArray();

            return result;
        }
    }
}
