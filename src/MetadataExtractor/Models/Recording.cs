

using NodaTime;

namespace MetadataExtractor.Models
{

    public class Recording
    {

        public string Name { get; set; }

        public string Extension { get; set; }

        public string Stem { get; set; }

        public OffsetDateTime StartDate { get; set; }

        public Checksum CalculatedChecksum { get; set; }

        public Duration DurationSeconds { get; set; }

        public int Channels { get; set; }

        public int SampleRateHertz { get; set; }

        public int BitsPerSecond { get; set; }

        public string MediaType { get; set; }

        public string FileLengthBytes { get; set; }

        public Sensor Sensor { get; set; }

        public Error[] Errors { get; set; }

        public Warning[] Warnings { get; set; }

    }
}