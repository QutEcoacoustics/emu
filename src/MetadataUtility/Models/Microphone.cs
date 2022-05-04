// <copyright file="Microphone.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
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
        public string BuildDate { get; init; }

        /// <summary>
        /// Gets the gain of this microphone.
        /// </summary>
        public string Gain { get; init; }

        /// <summary>
        /// Gets microphone number according to the sensor (1, 2, etc.).
        /// </summary>
        public int Channel { get; init; }

        /// <summary>
        /// Gets the name of the microphone (style depends on vendor).
        /// </summary>
        public string ChannelName { get; init; }

        /// <summary>
        /// Gets the sensitivity of the microphone.
        /// Units: Decibels relative to full scale (dBFS).
        /// </summary>
        public string Sensitivity { get; init; }
    }
}
