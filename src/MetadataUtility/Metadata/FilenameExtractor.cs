// <copyright file="FilenameExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using MetadataUtility.Filenames;
    using MetadataUtility.Models;

    public class FilenameExtractor : IMetadataOperation
    {
        private readonly IFileSystem fileSystem;
        private readonly FilenameParser parser;

        public FilenameExtractor(IFileSystem fileSystem, FilenameParser parser)
        {
            this.fileSystem = fileSystem;
            this.parser = parser;
        }

        public ValueTask<bool> CanProcessAsync(ITargetInformation information)
        {
            var hasName = !string.IsNullOrWhiteSpace(this.fileSystem.Path.GetFileName(information.Path));

            // true if the target has a filename
            return ValueTask.FromResult(hasName);
        }

        public ValueTask<Recording> ProcessFileAsync(ITargetInformation information, Recording recording)
        {
            var result = this.parser.Parse(information.Path);

            var updated = recording with
            {
                Extension = result.Extension,
                // etc...
            };

            return ValueTask.FromResult(updated);
        }
    }
}
