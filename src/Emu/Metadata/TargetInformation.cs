// <copyright file="TargetInformation.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata
{
    using System.IO.Abstractions;
    using Emu.Metadata.SupportFiles;

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

        private FileStream stream = null;

        public TargetInformation(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Gets a store of checks (predicates) done on this target. A cache so the same
        /// check can be done many times by different metadata extractors without
        /// having to re-run the check.
        /// </summary>
        private Dictionary<object, bool> Predicates { get; } = new();

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
        public Stream FileStream
        {
            get
            {
                return this.stream ?? this.FileSystem.File.Open(this.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        }

        /// <summary>
        /// Gets all support files for this target.
        /// Support files are log files, configuration files, etc.
        /// </summary>
        public Dictionary<string, SupportFile> TargetSupportFiles { get; } = new Dictionary<string, SupportFile>();

        /// <summary>
        /// Gets list of each identified support file
        /// Support files are log files, configuration files, etc.
        /// </summary>
        public static List<SupportFile> KnownSupportFiles { get; } = new List<SupportFile>();

        public IFileSystem FileSystem => this.fileSystem;

        /// <summary>
        /// Checks is something about a target is true or not.
        /// The results are cached.
        /// </summary>
        public bool CheckPredicate(Func<TargetInformation, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            if (this.Predicates.ContainsKey(predicate))
            {
                return this.Predicates[predicate];
            }

            var result = predicate.Invoke(this);
            this.Predicates.Add(predicate, result);

            return result;
        }

        /// <summary>
        /// Checks is something about a target is true or not.
        /// The results are cached.
        /// </summary>
        public async ValueTask<bool> CheckPredicateAsync(Func<TargetInformation, ValueTask<bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            if (this.Predicates.ContainsKey(predicate))
            {
                return this.Predicates[predicate];
            }

            var result = await predicate.Invoke(this);
            this.Predicates.Add(predicate, result);

            return result;
        }

        public void Dispose()
        {
            this.stream?.Dispose();
        }
    }
}
