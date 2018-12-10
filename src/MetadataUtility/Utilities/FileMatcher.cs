// <copyright file="FileMatcher.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Enumeration;
    using System.Linq;
    using System.Text;
    using DotNet.Globbing;
    using DotNet.Globbing.Token;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Processes input arguments to match audio recordings.
    /// </summary>
    public class FileMatcher
    {
        private static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

        private readonly ILogger<FileMatcher> logger;

        private readonly EnumerationOptions enumerationOptions = new EnumerationOptions()
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
                // first check if any of the given patterns are valid paths themselves
                var checkPath = Path.GetFullPath(pattern, baseDir);
                if (File.Exists(checkPath))
                {
                    this.logger.LogTrace("Path {0} exists and returned as {1}", pattern, checkPath);
                    yield return checkPath;
                    continue;
                }

                // now assume it is a glob
                this.logger.LogTrace("Glob {0} parsed as {1}", pattern, checkPath);
                var glob = Glob.Parse(checkPath);

                // check if the glob is rooted
                // build up longest possible literal path and then check if it is a directory
                var globRoot = glob
                    .Tokens
                    .TakeWhile(token => token is LiteralToken || token is PathSeparatorToken)
                    .Aggregate(
                        string.Empty,
                        (path, token) =>
                            path + (token is LiteralToken literalToken ? literalToken.Value : DirectorySeparator));

                var currentBase = Directory.Exists(globRoot) ? globRoot : baseDir;

                // finally start enumerating the directory
                this.logger.LogTrace("Begin recursive scan of {0}", currentBase);

                foreach (var path in Directory.EnumerateFiles(currentBase, "*", this.enumerationOptions))
                {
                    if (glob.IsMatch(path))
                    {
                        this.logger.LogTrace("Path matched via glob {0}", path, checkPath);
                        yield return path;
                    }
                    else
                    {
                        this.logger.LogTrace("Path NOT matched by glob {0}", path);
                    }
                }
            }

            this.logger.LogTrace("Finished scanning files");
        }
    }
}
