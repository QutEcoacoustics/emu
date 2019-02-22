// <copyright file="Renamer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using MetadataUtility.Cli;
    using MetadataUtility.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Renames files according to the recommended name.
    /// </summary>
    public class Renamer
    {
        private readonly ILogger<Renamer> logger;
        private readonly MainArgs arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="Renamer"/> class.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <param name="arguments">The arguments supplied to Emu.</param>
        public Renamer(ILogger<Renamer> logger,  MainArgs arguments)
        {
            this.logger = logger;
            this.arguments = arguments;
        }

        /// <summary>
        /// Generates renamed paths for recordings.
        /// </summary>
        /// <param name="recordings">The recordings to operate on. Modifies the <see cref="Recording.RenamedPath"/> value for each recording.</param>
        /// <returns>The number of conflicts that would occur if the recordings were renamed.</returns>
        public int CreateAndCheckNewPaths(IReadOnlyCollection<Recording> recordings)
        {
            var hash = new HashSet<string>(recordings.Count * 2);
            var conflicts = 0;
            foreach (var recording in recordings)
            {
                if (!hash.Add(recording.SourcePath))
                {
                    this.logger.LogError($"Cannot rename {recording.SourcePath} it conflicts with a path of the same name");
                    conflicts++;
                }

                recording.RenamedPath = Path.GetFullPath(Path.Combine(recording.Directory, recording.RecommendedName));

                if (!hash.Add(recording.RenamedPath))
                {
                    this.logger.LogError($"Cannot rename {recording.SourcePath} to {recording.RenamedPath} because it conflicts with another renamed path");
                    conflicts++;
                }

                if (File.Exists(recording.RenamedPath))
                {
                    conflicts++;
                    this.logger.LogError($"Cannot rename {recording.SourcePath} to {recording.RenamedPath} because it conflicts with a pre-existing file");
                }
            }

            return conflicts;
        }

        /// <summary>
        /// Renames a file according to an archival standard.
        /// </summary>
        /// <param name="recording">The metadata required to rename the file.</param>
        public void RenameFile(Recording recording)
        {
            if (this.arguments.DryRun)
            {
                this.logger.LogWarning($"{recording.SourcePath} would be renamed to {recording.RenamedPath}");
            }
            else
            {
                this.logger.LogDebug($"{recording.SourcePath} renamed to {recording.RenamedPath}");
                File.Move(recording.SourcePath, recording.RenamedPath);
            }
        }

        /// <summary>
        /// Renames all recordings supplied after running validation checks.
        /// </summary>
        /// <param name="recordings">The recordings to operate on.</param>
        /// <returns>The same recordings, with updated <see cref="Recording.RenamedPath"/> values.</returns>
        public async Task<Recording[]> RenameAll(Recording[] recordings)
        {
            return await Task.Run(Process);

            Recording[] Process()
            {
                this.logger.LogDebug("Processing renames");
                var conflicts = this.CreateAndCheckNewPaths(recordings);

                if (conflicts == 0)
                {
                    foreach (var recording in recordings)
                    {
                        this.RenameFile(recording);
                    }
                }
                else
                {
                    this.logger.LogError($"Renaming cannot continue because there are {conflicts.ToString()} conflicts");
                }

                return recordings;
            }
        }
    }
}
