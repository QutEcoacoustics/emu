// <copyright file="FileMatcher.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Utilities
{
    using System.Collections.Generic;
    using System.IO;
    using GlobExpressions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Processes input arguments to match audio recordings.
    /// </summary>
    public class FileMatcher
    {
        public static readonly Glob DefaultPattern = new("**/*.{flac,wavmp3}");

        private readonly ILogger<FileMatcher> logger;

        private readonly EnumerationOptions enumerationOptions = new()
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true,
            ReturnSpecialDirectories = false,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMatcher"/> class.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        public FileMatcher(ILogger<FileMatcher> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Expands a series of paths or globs into all matching files.
        /// </summary>
        /// <param name="baseDir">The directory to scan if the path/glob is not fully qualified.</param>
        /// <param name="patterns">The paths/globs to process.</param>
        /// <returns>A series of paths.</returns>
        public IEnumerable<string> ExpandMatches(string baseDir, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                Glob glob;
                string currentBase = baseDir;
                // first check if any of the given patterns are valid paths themselves
                var checkPath = Path.GetFullPath(pattern, baseDir);
                if (File.Exists(checkPath))
                {
                    this.logger.LogTrace("File {pattern} exists was returned without expanding", pattern);
                    yield return checkPath;
                    continue;
                }
                else if (Directory.Exists(checkPath))
                {
                    this.logger.LogTrace("Directory {pattern} exists was converted to the glob {defaultPattern}", pattern, DefaultPattern);

                    glob = DefaultPattern;
                    baseDir = checkPath;
                }
                else
                {
                    // now assume it is a glob

                    // check if the glob is rooted
                    // build up longest possible literal path and then check if it is a directory
                    var fragments = new Queue<string>(pattern.Split(Path.DirectorySeparatorChar));
                    string lastValid = null;
                    do
                    {
                        var next = fragments.Peek();

                        // cheap shortcut - other glob patterns could be detected but I figure this is the common case
                        // and regardless, they'll fail the exists check below
                        if (next.StartsWith("*") || next == "**")
                        {
                            break;
                        }

                        string lastChecked = lastValid == null ? next : Path.Join(lastValid, next);
                        this.logger.LogTrace("Does {lastChecked} exist?", lastChecked);
                        if (!Directory.Exists(lastChecked))
                        {
                            break;
                        }

                        fragments.Dequeue();
                        lastValid = lastChecked;
                    }
                    while (fragments.Count > 0);

                    var remaining = string.Join(Path.DirectorySeparatorChar, fragments);
                    this.logger.LogTrace("Remaining: {remaining}", remaining);
                    glob = new Glob(remaining);
                    currentBase = lastValid?.Length > 0 ? lastValid : baseDir;
                }

                this.logger.LogTrace("Glob {pattern} parsed as {glob} in {base}", pattern, glob.Pattern, currentBase);

                // finally start enumerating the directory
                foreach (var path in Directory.EnumerateFiles(currentBase, "*", this.enumerationOptions))
                {
                    if (glob.IsMatch(path))
                    {
                        this.logger.LogTrace("Path matched via glob {path}", path);
                        yield return path;
                    }
                    else
                    {
                        this.logger.LogTrace("Path NOT matched by glob {path}", path);
                    }
                }
            }

            this.logger.LogTrace("Finished scanning files");
        }
    }
}
