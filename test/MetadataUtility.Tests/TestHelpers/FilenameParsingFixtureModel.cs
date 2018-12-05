// <copyright file="FilenameParsingFixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using NodaTime;

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "These fields are set by reflection when deserialized")]
    public class FilenameParsingFixtureModel
    {
        public string Filename { get; private set; }

        public LocalDateTime? ExpectedDateTime { get; private set; }

        public Offset? ExpectedTzOffset { get; private set; }

        public double? ExpectedLatitude { get; private set; }

        public double? ExpectedLongitude { get; private set; }

        public string Prefix { get; private set; }

        public string Suffix { get; private set; }

        public string Extension { get; private set; }

        //public string SuggestedFilename { get; private set; }

        //public string SensorType { get; private set; }

        //public double? SensorTypeEstimate { get; private set; }

        public override string ToString()
        {
            return this.Filename;
        }
    }
}
