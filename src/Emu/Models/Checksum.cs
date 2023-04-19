// <copyright file="Checksum.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models
{
    using System;

    /// <summary>
    /// Represents a checksum, a unique signature for some data.
    /// </summary>
    public record Checksum : IFormattable
    {
        /// <summary>
        /// Gets the algorithm name used to calculate this checksum.
        /// </summary>
        public string Type { get; init; }

        /// <summary>
        /// Gets the value of checksum.
        /// </summary>
        public string Value { get; init; }

        public override string ToString()
        {
            return $"{this.Type}::{this.Value}";
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return format switch
            {
                "" or null => this.ToString(),
                "x" => this.ToString(),
                "X" => $"{this.Type.ToUpperInvariant()}::{this.Value.ToUpperInvariant()}",
                _ => throw new FormatException($"Unknown format {format}"),
            };
        }
    }
}
