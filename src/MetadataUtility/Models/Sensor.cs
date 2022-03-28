// <copyright file="Sensor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
    using Newtonsoft.Json;
    using NodaTime;

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
        public string Firmware { get; init; }

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
        /// Gets the battery level of the sensor.
        /// </summary>
        public string BatteryLevel { get; init; }

        /// <summary>
        /// Gets a list of microphones attached to this sensor.
        /// </summary>
        public IList<Microphone> Microphones { get; init; }

        /// <summary>
        /// Gets the last time the sensor was synchronized.
        /// </summary>
        public OffsetDateTime? LastTimeSync { get; init; }

        /// <summary>
        /// Gets a base64 encoded representation of the sensor's
        /// configuration file.
        /// </summary>
        public string[] Configuration { get; init; }
    }
}
