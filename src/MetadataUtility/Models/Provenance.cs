// <copyright file="Provenance.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
    /// <summary>
    /// The enum representing the provenance of a metadata value.
    /// </summary>
    public enum Provenance
    {
        /// <summary>
        /// We don't know where this metadata came from.
        /// </summary>
        Unknown,

        /// <summary>
        /// This metadata was extracted from the filename.
        /// </summary>
        Filename,

        /// <summary>
        /// This metadata was extracted from the log file found near the file.
        /// </summary>
        LogFile,

        /// <summary>
        /// This metadata was extracted from some file found near the file.
        /// </summary>
        OtherFile,

        /// <summary>
        /// This metadata was extracted from the header of the file.
        /// </summary>
        FileHeader,

        /// <summary>
        /// This value was calculated from other metadata values.
        /// </summary>
        Calculated,
    }
}
