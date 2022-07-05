// <copyright file="Checksum.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models
{
    /// <summary>
    /// Represents a checksum, a unique signature for some data.
    /// </summary>
    public record Checksum
    {
        /// <summary>
        /// Gets the algorithm name used to calculate this checksum.
        /// </summary>
        public string Type { get; init; }

        /// <summary>
        /// Gets the value of checksum.
        /// </summary>
        public string Value { get; init; }
    }
}
