// <copyright file="Checksum.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
    /// <summary>
    /// Represents a checksum, a unique signature for some data.
    /// </summary>
    public class Checksum
    {
        /// <summary>
        /// Gets or sets the algorithm name used to calculate this checksum.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value of checksum.
        /// </summary>
        public string Value { get; set; }
    }
}
