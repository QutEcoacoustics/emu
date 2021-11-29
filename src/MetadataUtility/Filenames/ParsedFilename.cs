// <copyright file="ParsedFilename.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Filenames
{
    using System.IO;
    using MetadataUtility.Dates;
    using MetadataUtility.Models;
    using NodaTime;

    /// <summary>
    /// Represents the information extracted from a filename.
    /// </summary>
    public record ParsedFilename
    {
        /// <summary>
        /// Gets the unambiguous datetime parsed from the given filename.
        /// </summary>
        public OffsetDateTime? OffsetDateTime { get; init; }

        /// <summary>
        /// Gets a local date time (ambiguous because there is no information
        /// on how this date is related to UTC) parsed from the given filename.
        /// </summary>
        public LocalDateTime? LocalDateTime { get; init; }

        /// <summary>
        /// Gets the location parsed from the given filename.
        /// </summary>
        public Location Location { get; init; }

        /// <summary>
        /// Gets any prefix found before the date stamp from the given filename.
        /// </summary>
        public string Prefix { get; init; }

        /// <summary>
        /// Gets the portion of the name that was parsed as a date into
        /// <see cref="OffsetDateTime"/> or <see cref="LocalDateTime"/>.
        /// </summary>
        public string DatePart { get; init; }

        /// <summary>
        /// Gets any suffix found after the date stamp from the given filename.
        /// </summary>
        public string Suffix { get; init; }

        /// <summary>
        /// Gets the extension (including a leading period) found for the given filename.
        /// </summary>
        public string Extension { get; init; }

        /// <summary>
        /// Gets the directory the file was found in.
        /// </summary>
        public string Directory { get; internal set; }

        public string Reconstruct()
        {
            var datePart = this switch
            {
                { OffsetDateTime: not null } => DateFormatting.FormatFileName(this.OffsetDateTime.Value),
                { LocalDateTime: not null } => DateFormatting.FormatFileName(this.LocalDateTime.Value),
                _ => this.DatePart,
            };

            return Path.Combine(this.Directory, $"{this.Prefix}{datePart}{this.Suffix}{this.Extension}");
        }
    }
}
