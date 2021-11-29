// <copyright file="Provenance.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
    using System;

    /// <summary>
    /// The enum representing the provenance of a metadata value.
    /// </summary>
    [Flags]
    public enum Provenance : byte
    {
        /// <summary>
        /// This value has no provenance information.
        /// </summary>
        None = 0b00000000,

        /// <summary>
        /// We don't know where this metadata came from.
        /// </summary>
        Unknown = 0b00000001,

        /// <summary>
        /// This metadata was extracted from the filename.
        /// </summary>
        Filename = 0b00000010,

        /// <summary>
        /// This metadata was extracted from the log file found near the file.
        /// </summary>
        LogFile = 0b00000100,

        /// <summary>
        /// This metadata was extracted from some file found near the file.
        /// </summary>
        OtherFile = 0b00001000,

        /// <summary>
        /// This metadata was extracted from the header of the file.
        /// </summary>
        FileHeader = 0b00010000,

        /// <summary>
        /// This value was calculated from other metadata values.
        /// </summary>
        Calculated = 0b00100000,

        /// <summary>
        /// This value was supplied by the user when running Emu.
        /// </summary>
        UserSupplied = 0b01000000,

        /// <summary>
        /// This the default or fallback value used when a value is missing
        /// or cannot be derived.
        /// </summary>
        Default = 0b10000000,
    }
}
