// <copyright file="Recording.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using NodaTime;
    using Rationals;

    /// <summary>
    /// A audio recording captured by a sensor or monitor.
    /// </summary>
    public record Recording
    {
        private string sourcePath;

        /// <summary>
        /// Gets the path to the filename as read by the program.
        /// </summary>
        public string SourcePath
        {
            get => this.sourcePath;
            init
            {
                this.sourcePath = value;
#pragma warning disable IO0006 // Replace Path class with IFileSystem.Path for improved testability
                this.Directory = Path.GetDirectoryName(this.sourcePath);
#pragma warning restore IO0006 // Replace Path class with IFileSystem.Path for improved testability
            }
        }

        /// <summary>
        /// Gets the directory of the recording. Used internally.
        /// </summary>
        [JsonIgnore]
        public string Directory { get; private init; }

        /// <summary>
        /// Gets the file extension as read by the program.
        /// It includes the period.
        /// </summary>
        public string Extension { get; init; }

        /// <summary>
        /// Gets the name of the file as read by the program
        /// without the extension.
        /// </summary>
        public string Stem { get; init; }

        /// <summary>
        /// Gets a recommended name.
        /// </summary>
        /// <remarks>
        /// This is a suggested name for the file that is better suited to archiving purposes.
        /// </remarks>
        public string RecommendedName { get; init; }

        /// <summary>
        /// Gets the original filename of the recording.
        /// </summary>
        public string Name => this.Stem + this.Extension;

        /// <summary>
        /// Gets the unambiguous start date of the recording.
        /// This is extracted either from the filename or from the metadata
        /// included in the recording.
        /// </summary>
        //public MetadataSource<OffsetDateTime>? StartDate { get; init; }
        public OffsetDateTime? StartDate { get; init; }

        /// <summary>
        /// Gets an ambiguous start date of the recording (no offset).
        /// This is extracted either from the filename or from the metadata
        /// included in the recording.
        /// </summary>
        public LocalDateTime? LocalStartDate { get; init; }

        /// <summary>
        /// Gets a Checksum calculated for the file.
        /// This checksum is calculated by EMU.
        /// </summary>
        public Checksum CalculatedChecksum { get; init; }

        /// <summary>
        /// Gets the duration of the recording.
        /// </summary>
        public Rational? DurationSeconds { get; init; }

        /// <summary>
        /// Gets the number of channels in the recording.
        /// </summary>
        public ushort? Channels { get; init; }

        /// <summary>
        /// Gets the sample rate of the recording.
        /// </summary>
        public uint? SampleRateHertz { get; init; }

        /// <summary>
        /// Gets the bit rate.
        /// </summary>
        public uint? BitsPerSecond { get; init; }

        /// <summary>
        /// Gets the numbers of bits used to quantize each sample.
        /// Value is per channel.
        /// </summary>
        public byte? BitDepth { get; init; }

        /// <summary>
        /// Gets an IANA Media Type.
        /// </summary>
        public string MediaType { get; init; }

        /// <summary>
        /// Gets the number of samples in this file.
        /// </summary>
        public ulong? TotalSamples { get; init; }

        /// <summary>
        /// Gets the number of bytes in this file.
        /// </summary>
        public ulong FileLengthBytes { get; init; }

        /// <summary>
        /// Gets a <see cref="MemoryCard"/> object
        /// that describes the memory card this recording was stored on.
        /// </summary>
        public MemoryCard MemoryCard { get; init; }

        /// <summary>
        /// Gets a <see cref="Sensor"/> object
        /// that describes the sensor that produced this recording.
        /// </summary>
        public Sensor Sensor { get; init; }

        /// <summary>
        /// Gets the location of the sensor
        /// when this recording was started.
        /// </summary>
        public Location Location { get; init; }

        /// <summary>
        /// Gets a list of locations captured while this
        /// recording was running.
        /// </summary>
        public IList<Location> AllLocations { get; init; }

        /// <summary>
        /// Gets a list of errors found in this audio file.
        /// </summary>
        public IList<Error> Errors { get; init; } = new List<Error>();

        /// <summary>
        /// Gets a list of errors found in this audio file.
        /// </summary>
        public IList<Warning> Warnings { get; init; } = new List<Warning>();

        /// <summary>
        /// Gets a Checksum calculated for the file.
        /// </summary>
        /// <remarks>
        /// This is a checksum produced by the sensor.
        /// </remarks>
        public Checksum EmbeddedChecksum { get; init; }

        /// <summary>
        /// Gets the date on the sensor for which this
        /// recording ended.
        /// </summary>
        /// <remarks>
        /// This field is useful for calculating drift in the sensor
        /// clock during recording.
        /// </remarks>
        public OffsetDateTime? EndDate { get; init; }

        /// <summary>
        /// Gets the Expected duration of the recording.
        /// </summary>
        public Duration? ExpectedDurationSeconds { get; init; }

        /// <summary>
        /// Gets a key-value store of other information not yet codified by the standard.
        /// </summary>
        public Dictionary<string, string> OtherFields { get; init; }
    }
}
