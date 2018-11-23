// <copyright file="FilenameParsingFixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using NodaTime;

    public class FilenameParsingFixtureModel
    {
        public string Filename { get; }

        public LocalDateTime? ExpectedDateTime { get; }

        public Offset? ExpectedTzOffset { get; }

        public double? ExpectedLatitude { get; }

        public double? ExpectedLongitude { get; }

        public string Prefix { get; }

        public string Suffix { get; }

        public string Extension { get; }

        public string SuggestedFilename { get; }

        public string SensorType { get; }

        public double? SensorTypeEstimate { get; }

        public override string ToString()
        {
            return this.Filename;
        }
    }
}
