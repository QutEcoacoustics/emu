// <copyright file="SupportFileFinder.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.SupportFiles
{
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO.Abstractions;
    using Emu.Metadata.SupportFiles.FrontierLabs;
    using Emu.Metadata.SupportFiles.OpenAcousticDevices;
    using LanguageExt;
    using LanguageExt.Common;
    using LanguageExt.UnsafeValueAccess;
    using Microsoft.Extensions.Logging;
    using MoreLinq.Extensions;
    using YamlDotNet.Core.Tokens;
    using static LanguageExt.Prelude;
    using Error = LanguageExt.Common.Error;

    public class SupportFileFinder
    {
        // Each potential support file pattern to search for
        // and the functions used to correlate a specific type of support file to targets
        private static readonly IEnumerable<SupportFileType> SupportFileFinders = new SupportFileType[]
        {
            // Frontier Labs
            new(LogFile.LogFileKey, LogFile.LogFilePattern, LogFile.ChooseLogFile, LogFile.Create),

            // Open Acoustic Devices
            new(ConfigFile.Key, ConfigFile.Pattern, ConfigFile.Choose, ConfigFile.Create),
        };

        private readonly ILogger<SupportFileFinder> logger;
        private readonly IFileSystem fileSystem;

        public SupportFileFinder(ILogger<SupportFileFinder> logger, IFileSystem fileSystem)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Test which of multiple support files should be associated with a target.
        /// </summary>
        /// <param name="target">The target to link it.</param>
        /// <param name="files">The found support files that should possibly be associated with a target.</param>
        /// <returns>The chosen support file to associate.</returns>
        public delegate Option<SupportFile> AssociateFilter(TargetInformation target, IReadOnlyCollection<SupportFile> files);

        /// <summary>
        /// Read in a support file to extract information from it.
        /// Return null if the file is invalid or not supported.
        /// </summary>
        /// <param name="fileSystem">The file system to use.</param>
        /// <param name="path">The path to the support file.</param>
        /// <returns>A support file instance, or <c>null</c> for an invalid file.</returns>
        public delegate Fin<SupportFile> CreateSupportFile(IFileSystem fileSystem, string path);

        /// <summary>
        /// Gets a map of each identified support file and the path where to get it.
        /// Support files are log files, configuration files, etc.
        /// </summary>
        public static ConcurrentDictionary<string, Fin<SupportFile>> KnownSupportFiles { get; } = new();

        /// <summary>
        /// Given a set of targets in the same directory, find any support files that may be associated with them
        /// and then actually links them to the targets. Support files are only parsed once.
        /// Searches at maximum three parent directories above a target file.
        /// </summary>
        /// <param name="directory">the parent directory for the targets.</param>
        /// <param name="targets">the targets to find support files for.</param>
        public void FindSupportFiles(string directory, IReadOnlyList<TargetInformation> targets)
        {
            Debug.Assert(
                targets.All(t => this.fileSystem.Path.GetDirectoryName(t.Path) == directory),
                "All targets should be in the same directory");

            string searchDirectory = directory;
            int i = 0;
            const int maxHeight = 3;

            // search directories for files
            while (i++ < maxHeight)
            {
                // Find any potential support files
                bool foundAny = false;
                foreach (var type in SupportFileFinders)
                {
                    var anyLinked = this.ScanDirectory(targets, searchDirectory, type);

                    foundAny = foundAny || anyLinked;
                }

                // We assume that support files will only be found in one directory!
                if (foundAny)
                {
                    break;
                }

                searchDirectory = this.fileSystem.Directory.GetParent(searchDirectory)?.FullName;

                // return if root directory is reached before any support files are found
                if (searchDirectory == null)
                {
                    return;
                }
            }
        }

        private bool ScanDirectory(IReadOnlyList<TargetInformation> targets, string searchDirectory, SupportFileType type)
        {
            var (_, pattern, _, constructor) = type;

            var maker = par<CreateSupportFile, string, Fin<SupportFile>>(this.ConstructOrFetch, constructor);
            var (errors, supportFiles) = this
                .fileSystem
                .Directory
                .GetFiles(searchDirectory, pattern, SearchOption.TopDirectoryOnly)

                // multiple files can be returned, parse them or retrieve them from cache
                .Map(maker)
                .Partition();

            this.LogErrors(errors);

            // associate one of the support files with all targets in the target directory
            return this.AssociateSupportFiles(type, targets, supportFiles.ToArray());
        }

        private void LogErrors(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                this.logger.LogDebug("Failed to read support file: {reason}", error);
            }
        }

        private Fin<SupportFile> ConstructOrFetch(CreateSupportFile createSupportFile, string path)
        {
            return KnownSupportFiles.GetOrAdd(path, (path) =>
            {
                // realise files into  support files
                // add these results to the cache - even if null/invalid
                // we don't want to parse good or bad files twice
                return createSupportFile(this.fileSystem, path);
            });
        }

        private bool AssociateSupportFiles(SupportFileType type, IReadOnlyList<TargetInformation> targets, SupportFile[] supportFiles)
        {
            // if we didn't find anything no need to keep going
            if (supportFiles.Length == 0)
            {
                return false;
            }

            var (key, _, filter, _) = type;
            var anyLinked = false;

            // associate the files with all targets. Only one support file of each type should be
            // attached to a target.
            // This is specifically set up to deal with two log files in one directory - one log
            // file is for some files, the other, other files.
            foreach (TargetInformation target in targets)
            {
                // Correlate specific support files to each target.
                // It seems messy to do it like this but the decision on whether or not to
                // associate depends on which files were found so we need to send the whole list.
                var which = filter(target, supportFiles);

                if (which.IsSome)
                {
                    target.TargetSupportFiles.Add(key, which.ValueUnsafe());
                    anyLinked = true;
                }
            }

            return anyLinked;
        }

        private record SupportFileType(string Key, string Pattern, AssociateFilter Filter, CreateSupportFile Constructor);
    }
}
