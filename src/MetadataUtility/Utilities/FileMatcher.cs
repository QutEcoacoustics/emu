// <copyright file="FileMatcher.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using DotNet.Globbing;
    using DotNet.Globbing.Token;
    using Microsoft.Extensions.Logging;

    public interface IFileMatcher
    {
        IEnumerable<string> ExpandMatches(string baseDir, IEnumerable<string> patterns);
    }

    /// <summary>
    /// Processes input arguments to match audio recordings.
    /// </summary>
    public class FileMatcher : IFileMatcher
    {
        private readonly ILogger<FileMatcher> logger;

        private static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

        private readonly EnumerationOptions enumerationOptions = new EnumerationOptions()
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true,
            ReturnSpecialDirectories = false,
        };

        public FileMatcher(ILogger<FileMatcher> logger)
        {
            this.logger = logger;
        }

        public IEnumerable<string> ExpandMatches(string baseDir, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                // first check if any of the given patterns are valid paths themselves
                if (File.Exists(pattern))
                {
                    yield return pattern;
                    continue;
                }

                // now assume it is a glob
                var glob = Glob.Parse(pattern);

                // check if the glob is rooted
                // build up longest possible literal path and then check if it is a directory
                var globRoot = glob
                    .Tokens
                    .TakeWhile(token => token is LiteralToken || token is PathSeperatorToken)
                    .Aggregate(
                        string.Empty,
                        (path, token) =>
                            path + (token is LiteralToken literalToken ? literalToken.Value : DirectorySeparator));

                var currentBase = Directory.Exists(globRoot) ? globRoot : baseDir;

                // finally start enumerating the directory
                foreach (var path in Directory.EnumerateFiles(currentBase, "*", this.enumerationOptions))
                {
                    if (glob.IsMatch(path))
                    {
                        yield return path;
                    }
                }
            }
        }
    }
}
