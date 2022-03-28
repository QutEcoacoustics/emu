// <copyright file="Microphone.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Holds information regarding a microphone
    /// that is attached to the device.
    /// </summary>
    public record Microphone
    {
        /// <summary>
        /// Gets microphone number according to the sensor (1, 2, etc.).
        /// Used internally to correlate microphone metadata correctly.
        /// </summary>
        [JsonIgnore]
        public int Number { get; init; }

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
    }
}
