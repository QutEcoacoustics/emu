// <copyright file="ParsedFilename.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Filenames
{
    using Emu.Models;
    using LanguageExt;
    using NodaTime;

    /// <summary>
    /// Represents the information extracted from a filename.
    /// </summary>
    public record ParsedFilename
    {
        /// <summary>
        /// Gets the unambiguous datetime parsed from the given filename.
        /// </summary>
        public OffsetDateTime? StartDate { get; init; }

        /// <summary>
        /// Gets a local date time (ambiguous because there is no information
        /// on how this date is related to UTC) parsed from the given filename.
        /// </summary>
        public LocalDateTime? LocalStartDate { get; init; }

        /// <summary>
        /// Gets the unambiguous *end* datetime parsed from the given filename.
        /// </summary>
        /// <remarks>
        /// Currently only one sensor (FL BAR-LT) encodes the end date in the filename.
        /// </remarks>
        public OffsetDateTime? EndDate { get; init; }

        /// <summary>
        /// Gets the location parsed from the given filename.
        /// </summary>
        public Location Location { get; init; }

        /// <summary>
        /// Gets the extension (including a leading period) found for the given filename.
        /// </summary>
        public string Extension { get; init; }

        /// <summary>
        /// Gets a tokenized representation of the name.
        /// </summary>
        public string TokenizedName => string.Join(string.Empty, this.NameTokens.Select(x => x.ToString()));

        /// <summary>
        /// Gets the list of tokens that represent the extracted information from the filename.
        /// </summary>
        /// <value></value>
        public Lst<FilenameToken> NameTokens { get; init; }

        /// <summary>
        /// Gets the directory the file was found in.
        /// </summary>
        public string Directory { get; internal set; }
    }
}
