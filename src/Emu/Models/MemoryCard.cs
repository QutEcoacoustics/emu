// <copyright file="MemoryCard.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models
{
    /// <summary>
    /// Holds information regarding a memory card
    /// that is installed in to the device.
    /// </summary>
    public record MemoryCard
    {
        public string FormatType { get; init; }

        public byte? ManufacturerID { get; init; }

        public string OEMID { get; init; }

        public string ProductName { get; init; }

        public float? ProductRevision { get; init; }

        public uint? SerialNumber { get; init; }

        public string ManufactureDate { get; init; }

        public uint? Speed { get; init; }

        public ulong? Capacity { get; init; }

        public uint? WrCurrentVmin { get; init; }

        public uint? WrCurrentVmax { get; init; }

        public uint? WriteBlSize { get; init; }

        public uint? EraseBlSize { get; init; }
    }
}
