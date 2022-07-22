// <copyright file="FilenameExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Emu.Filenames;
    using Emu.Models;
    using Microsoft.Extensions.Logging;

    public class FilenameExtractor : IMetadataOperation
    {
        private readonly IFileSystem fileSystem;
        private readonly FilenameParser parser;
        private readonly ILogger<FilenameExtractor> logger;

        public FilenameExtractor(ILogger<FilenameExtractor> logger, IFileSystem fileSystem, FilenameParser parser)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
            this.parser = parser;
        }

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var hasName = information.HasFileName();

            // true if the target has a filename
            return ValueTask.FromResult(hasName);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var result = this.parser.Parse(information.Path);

            var stem = this.fileSystem.Path.GetFileNameWithoutExtension(information.Path);

            recording = this.ApplyValues(recording, result, stem, (ulong)information.FileStream.Length);

            return ValueTask.FromResult(recording);
        }

        public Recording ApplyValues(Recording recording, ParsedFilename parsedFilename, string stem, ulong size)
        {
            // sometimes extensions aren't available
            // in that case other metadata extractors have the chance to update
            // the extension when they scan the media type in the files
            // Thus we ensure no extension (empty string) is forced to null
            // so other extractors can use the famililar ?? update style assignment.
            var extension = string.IsNullOrEmpty(parsedFilename.Extension) ? null : parsedFilename.Extension;

            return recording with
            {
                Extension = recording.Extension ?? extension,
                Stem = recording.Stem ?? stem,
                StartDate = recording.StartDate ?? parsedFilename.StartDate,
                LocalStartDate = recording.LocalStartDate ?? parsedFilename.LocalStartDate,
                Location = recording.Location ?? parsedFilename.Location,
                FileSizeBytes = size,
            };
        }
    }
}
