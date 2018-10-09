// <copyright file="Sensor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Describes a passive acoustic monitor/sensor that
    /// was used to generate a recording.
    /// </summary>
    public class Sensor
    {
        /// <summary>
        /// Gets or sets the common name used to refer to the sensor.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the firmware version of this sensor.
        /// </summary>
        public string Firmware { get; set; }

        /// <summary>
        /// Gets or sets the serial number of the sensor.
        /// </summary>
        public string SensorSerialNumber { get; set; }

        /// <summary>
        /// Gets or sets a description of the powersource used by this sensor.
        /// </summary>
        public string PowerSource { get; set; }

        /// <summary>
        /// Gets or sets the voltage of the <see cref="PowerSource"/>
        /// used by this sensor.
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// Gets or sets the gain setting used by the sensor.
        /// </summary>
        public string Gain { get; set; }

        /// <summary>
        /// Gets or sets a list of microphones attached to this sensor.
        /// </summary>
        public Microphone[] Microphones { get; set; }

        /// <summary>
        /// Gets or sets a base64 encoded representation of the sensor's
        /// configuration file.
        /// </summary>
        public string[] Configuration { get; set; }
    }
}
