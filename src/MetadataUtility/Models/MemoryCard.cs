// <copyright file="MemoryCard.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
    /// <summary>
    /// Holds information regarding a microphone
    /// that is attached to the device.
    /// </summary>
    public record MemoryCard
    {
        public string SDFormatType { get; init; }

        public uint SDManufacturerID { get; init; }

        public string SDOEMID { get; init; }

        public string SDProductName { get; init; }

        public float SDProductRevision { get; init; }

        public uint SDSerialNumber { get; init; }

        public string SDManufactureDate { get; init; }

        public uint SDSpeed { get; init; }

        public uint SDCapacity { get; init; }

        public uint SDWrCurrentVmin { get; init; }

        public uint SDWrCurrentVmax { get; init; }

        public uint SDWriteB1Size { get; init; }

        public uint SDEraseB1Size { get; init; }
    }
}
