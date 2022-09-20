// <copyright file="FileMatcher.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Utilities
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Runtime.InteropServices;
    using Emu.Utilities.FileSystem;
    using Microsoft.Extensions.FileSystemGlobbing;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Processes input arguments to match audio recordings.
    /// </summary>
    public class FileMatcher
    {
        public const string DefaultPatternString = "**/*.flac **/*.wav **/*.mp3 **/data";
        public static readonly Matcher DefaultPattern = new Matcher()
            .AddInclude("**/*.flac")
            .AddInclude("**/*.wav")
            .AddInclude("**/*.mp3")
            .AddInclude("**/data");

        private readonly ILogger<FileMatcher> logger;
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMatcher"/> class.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="fileSystem">The file system to work on.</param>
        public FileMatcher(ILogger<FileMatcher> logger, IFileSystem fileSystem)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Expands a series of paths or globs into all matching files.
        /// </summary>
        /// <param name="baseDir">The directory to scan if the path/glob is not fully qualified.</param>
        /// <param name="patterns">The paths/globs to process.</param>
        /// <returns>A series of paths along with the base directory where the search started.</returns>
        public IEnumerable<(string Base, string File)> ExpandMatches(string baseDir, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                Matcher glob;
                string currentBase = baseDir;

                // first check if any of the given patterns are valid paths themselves
                var checkPath = this.fileSystem.Path.GetFullPath(pattern, baseDir);
                if (this.fileSystem.File.Exists(checkPath))
                {
                    this.logger.LogTrace("File {pattern} exists was returned without expanding", pattern);
                    yield return (this.fileSystem.Path.GetDirectoryName(checkPath), checkPath);
                    continue;
                }
                else if (this.fileSystem.Directory.Exists(checkPath))
                {
                    this.logger.LogInformation("No wild card was provided, using the default {defaultPattern}", DefaultPatternString);

                    glob = DefaultPattern;
                    currentBase = checkPath;
                }
                else
                {
                    // now assume it is a glob

                    // check if the glob is rooted
                    // build up longest possible literal path and then check if it is a directory
                    var split = pattern.Split(this.fileSystem.Path.DirectorySeparatorChar);

                    // and special case for unix platforms since the leading '/' is removed by the path.split
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && split.First() == string.Empty)
                    {
                        // restore root as the first path checked
                        split[0] = this.fileSystem.Path.DirectorySeparatorChar.ToString();
                    }

                    var fragments = new Queue<string>(split);

                    // if the glob is rooted, treat it as the whole path
                    // otherwise assume the current base directory is the starting point
                    // and the glob is a relative expression
                    string lastValid = this.fileSystem.Path.IsPathRooted(pattern) ? null : currentBase;

                    do
                    {
                        var next = fragments.Peek();

                        // cheap shortcut - other glob patterns could be detected but I figure this is the common case
                        // and regardless, they'll fail the exists check below
                        if (next.StartsWith("*") || next == "**")
                        {
                            break;
                        }

                        string lastChecked = lastValid == null ? next : this.fileSystem.Path.Join(lastValid, next);
                        this.logger.LogTrace("Does {lastChecked} exist?", lastChecked);
                        if (!this.fileSystem.Directory.Exists(lastChecked))
                        {
                            break;
                        }

                        fragments.Dequeue();
                        lastValid = lastChecked;
                    }
                    while (fragments.Count > 0);

                    var remaining = string.Join(Path.DirectorySeparatorChar, fragments);
                    this.logger.LogTrace("Remaining: {remaining}", remaining);
                    glob = new Matcher();
                    glob.AddInclude(remaining);
                    currentBase = lastValid?.Length > 0 ? lastValid : baseDir;
                }

                currentBase = this.fileSystem.Path.GetFullPath(currentBase);

                this.logger.LogTrace("Glob {pattern} parsed as {glob} in {base}", pattern, glob, currentBase);

                // finally start enumerating the directory
                foreach (var path in glob.GetResultsInFullPath(this.fileSystem, currentBase))
                {
                    this.logger.LogTrace("Path matched via glob {path}", path);
                    yield return (currentBase, path);
                }
            }

            this.logger.LogTrace("Finished scanning files");
        }
    }
}
