// <copyright file="Recording.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataExtractor.Models
{
    using System.Collections.Generic;
    using NodaTime;

    /// <summary>
    /// A audio recording captured by a sensor or monitor.
    /// </summary>
    public class Recording
    {
        /// <summary>
        /// Gets or sets the path to the filename as read by the program.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the file extension as read by the program.
        /// It includes the period.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets the name of the file as read by the program
        /// without the extension.
        /// </summary>
        public string Stem { get; set; }

        /// <summary>
        /// Gets or sets a recommended name.
        /// </summary>
        /// <remarks>
        /// This is a suggested name for the file that is better suited to archiving purposes.
        /// </remarks>
        public string RecommendedName { get; set; }

        /// <summary>
        /// Gets or sets the starte date of the recording.
        /// This is extracted either from the filename or from the metadata
        /// included in the recording.
        /// </summary>
        public OffsetDateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets a Checksum calculated for the file.
        /// This checksum is calculated by EMU.
        /// </summary>
        public Checksum CalculatedChecksum { get; set; }

        /// <summary>
        /// Gets or sets the duration of the recording.
        /// </summary>
        public Duration DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the number of channels in the recording.
        /// </summary>
        public int Channels { get; set; }

        /// <summary>
        /// Gets or sets the sample rate of the recording.
        /// </summary>
        public int SampleRateHertz { get; set; }

        /// <summary>
        /// Gets or sets the bit rate.
        /// </summary>
        public int BitsPerSecond { get; set; }

        /// <summary>
        /// Gets or sets an IANA Media Type.
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes in this file.
        /// </summary>
        public string FileLengthBytes { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Sensor"/> object
        /// that describes the sensor that produced this recording.
        /// </summary>
        public Sensor Sensor { get; set; }

        /// <summary>
        /// Gets or sets a list of errors found in this audio file.
        /// </summary>
        public Error[] Errors { get; set; }

        /// <summary>
        /// Gets or sets a list of errors found in this audio file.
        /// </summary>
        public Warning[] Warnings { get; set; }

        /// <summary>
        /// Gets or sets a Checksum calculated for the file.
        /// </summary>
        /// <remarks>
        /// This is a checksum produced by the sensor.
        /// </remarks>
        public Checksum EmbeddedChecksum { get; set; }

        /// <summary>
        /// Gets or sets the date on the sensor for which this
        /// recording ended.
        /// </summary>
        /// <remarks>
        /// This field is useful for calculating drift in the sensor
        /// clock during recording.
        /// </remarks>
        public OffsetDateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier for the memory card
        /// that this recording was stored on.
        /// </summary>
        /// <remarks>
        /// Such as https://www.cameramemoryspeed.com/sd-memory-card-faq/reading-sd-card-cid-serial-psn-internal-numbers/.
        /// </remarks>
        public string StorageCardIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the Expected duration of the recording.
        /// </summary>
        public Duration? ExpectedDurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets a key-value store of other information not yet codified by the standard.
        /// </summary>
        public Dictionary<string, string> OtherFields { get; set; }
    }
}
