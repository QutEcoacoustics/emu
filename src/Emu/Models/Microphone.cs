// <copyright file="Microphone.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models
{
    using NodaTime;

    /// <summary>
    /// Holds information regarding a microphone
    /// that is attached to the device.
    /// </summary>
    public record Microphone
    {
        /// <summary>
        /// Gets the type of this microphone.
        /// </summary>
        public string Type { get; init; }

        /// <summary>
        /// Gets the unique identifier of this microphone.
        /// </summary>
        public string UID { get; init; }

        /// <summary>
        /// Gets the build date of this microphone.
        /// </summary>
        public LocalDate? BuildDate { get; init; }

        /// <summary>
        /// Gets the gain of this microphone.
        /// Units: Decibels.
        /// </summary>
        public double? Gain { get; init; }

        /// <summary>
        /// Gets channel index assigned to the microphone according to the sensor (0, 1, 2, etc.).
        /// As per: https://en.wikipedia.org/wiki/Surround_sound#Channel_identification.
        /// </summary>
        public int? Channel { get; init; }

        /// <summary>
        /// Gets the name of the channel assigned to this microphone (style depends on vendor).
        /// </summary>
        public string ChannelName { get; init; }

        /// <summary>
        /// Gets the sensitivity of the microphone.
        /// Units: Decibels relative to full scale (dBFS).
        /// </summary>
        public double? Sensitivity { get; init; }
    }
}
