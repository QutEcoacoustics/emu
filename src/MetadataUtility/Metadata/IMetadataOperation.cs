// <copyright file="IMetadataOperation.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata
{
    using System.IO.Abstractions;
    using MetadataUtility.Models;

    public class MetadataRegister
    {
        private readonly IServiceProvider provider;

        public static readonly Type[] KnownOperations = new[]
        {
            // each time we make a new extractor we'll add it here
            typeof(FilenameExtractor),
        };

        private IEnumerable<IMetadataOperation> resolved;

        public MetadataRegister(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public IEnumerable<IMetadataOperation> All
        {
            get
            {
                this.resolved ??= KnownOperations.Select(x => (IMetadataOperation)this.provider.GetService(x));
                return this.resolved;
            }
        }
    }

    public interface IMetadataOperation
    {
        /// <summary>
        /// Checks if this metadata extractor is applicable to the given target.
        /// </summary>
        /// <param name="information">Information about the current target.</param>
        /// <returns>True if this metadata extractor can operate on the current target.</returns>
        ValueTask<bool> CanProcessAsync(TargetInformation information);

        /// <summary>
        /// Extracts the metadata from the given target.
        /// </summary>
        /// <param name="information">Information about the current target.</param>
        /// <param name="recording">The current information extracted from the target.</param>
        /// <returns>An updated recording with the newly extracted metadata.</returns>
        ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording);
        // todo: encode success/failure of the operation?
    }

    // a.wav
    // b.flac
    // extractor for FL FLAC files
    //   is this a flac file?
    //   FL file?
    //   more than 0 bytes?
    //   does it have a header?
    //   does it have a flac header?
    //     if yes return true
    // extractor for FL WAVE files
    //   is this a WAVE file?
    //   FL file?
    //   more than 0 bytes?
    //   does it have a header?
    //     if yes return true?
    //   does it have a flac header?
    //     if yes return false

    public record TargetInformation : IDisposable
    {
        private readonly IFileSystem fileSystem;

        public TargetInformation(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Gets a store of checks (predicates) done on this target. A cache so the same
        /// check can be done many times by different metadata extractors without
        /// having to re-run the check.
        /// </summary>
        public Dictionary<string, bool> Predicates { get; } = new Dictionary<string, bool>();

        /// <summary>
        /// Gets the path to the current target.
        /// </summary>
        public string Path { get; init; }

        /// <summary>
        /// Gets the path to the directory the target was found in.
        /// </summary>
        public string Base { get; init; }

        /// <summary>
        /// Gets the file stream for the current target.
        /// </summary>
        public FileStream FileStream => (FileStream)this.fileSystem.File.Open(this.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

        /// <summary>
        /// Gets all known support files.
        /// Support files are log files, configuration files, etc.
        /// </summary>
        public Dictionary<string, string> KnownSupportFiles { get; } = new Dictionary<string, string>();

        public void Dispose()
        {
            this.FileStream?.Dispose();
        }
    }
}
