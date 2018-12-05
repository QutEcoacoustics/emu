// <copyright file="ParsedFilename.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Filenames
{
    using MetadataUtility.Models;
    using NodaTime;

    /// <summary>
    /// Represents the information extracted from a filename.
    /// </summary>
    public class ParsedFilename
    {
        /// <summary>
        /// Gets or sets the unambiguous datetime parsed from the given filename.
        /// </summary>
        public OffsetDateTime? OffsetDateTime { get; set; }

        /// <summary>
        /// Gets or sets a local date time (ambiguous because there is no information
        /// on how this date is related to UTC) parsed from the given filename.
        /// </summary>
        public LocalDateTime? LocalDateTime { get; set; }

        /// <summary>
        /// Gets or sets the location parsed from the given filename.
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Gets or sets any prefix found before the date stamp from the given filename.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the portion of the name that was parsed as a date into
        /// <see cref="OffsetDateTime"/> or <see cref="LocalDateTime"/>.
        /// </summary>
        public string DatePart { get; set; }

        /// <summary>
        /// Gets or sets any suffix found after the date stamp from the given filename.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the extension (including a leading period) found for the given filename.
        /// </summary>
        public string Extension { get; set; }
    }
}
