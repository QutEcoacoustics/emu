// <copyright file="Sensor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
    /// <summary>
    /// Describes a passive acoustic monitor/sensor that
    /// was used to generate a recording.
    /// </summary>
    public record Sensor
    {
        /// <summary>
        /// Gets the common name used to refer to the sensor.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the firmware version of this sensor.
        /// </summary>
        public float Firmware { get; init; }

        /// <summary>
        /// Gets the serial number of the sensor.
        /// </summary>
        public string SerialNumber { get; init; }

        /// <summary>
        /// Gets a description of the powersource used by this sensor.
        /// </summary>
        public string PowerSource { get; init; }

        /// <summary>
        /// Gets the voltage of the <see cref="PowerSource"/>
        /// used by this sensor.
        /// </summary>
        public double Voltage { get; init; }

        /// <summary>
        /// Gets the gain setting used by the sensor.
        /// </summary>
        public string Gain { get; init; }

        /// <summary>
        /// Gets a list of microphones attached to this sensor.
        /// </summary>
        public Microphone[] Microphones { get; init; }

        /// <summary>
        /// Gets a base64 encoded representation of the sensor's
        /// configuration file.
        /// </summary>
        public string[] Configuration { get; init; }
    }
}
