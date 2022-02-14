// <copyright file="FilenameExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using MetadataUtility.Filenames;
    using MetadataUtility.Models;
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

            var updated = recording with
            {
                Extension = result.Extension,

                //StartDate = result.OffsetDateTime ?? result.LocalDateTime,
                // etc...
            };

            return ValueTask.FromResult(updated);
        }
    }
}
