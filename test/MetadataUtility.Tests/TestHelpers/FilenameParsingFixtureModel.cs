// <copyright file="FilenameParsingFixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using NodaTime;

    public class FilenameParsingFixtureModel
    {
        public string Filename { get; set; }

        public LocalDateTime? ExpectedDateTime { get; set; }

        public Offset? ExpectedTzOffset { get; set; }

        public double? ExpectedLatitude { get; set; }

        public double? ExpectedLongitude { get; set; }

        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public string Extension { get; set; }

        //public string SuggestedFilename { get; set; }

        //public string SensorType { get; set; }

        //public double? SensorTypeEstimate { get; set; }

        public override string ToString()
        {
            return this.Filename;
        }
    }
}
