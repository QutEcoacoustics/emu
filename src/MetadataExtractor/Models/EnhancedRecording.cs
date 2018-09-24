

namespace MetadataExtractor.Models
{
    using System.Collections.Generic;
    using NodaTime;

    public class EnhancedRecording : Recording
    {

        public double? Gain { get; set; }

        public Checksum EmbeddedChecksum { get; set; }

        public OffsetDateTime EndDate { get; set; }

        public string StorageCardIdentifier { get; set; }

        public Duration ExpectedDurationSeconds { get; set; }

        public Dictionary<string, string> OtherFields { get; set; }

    }
}