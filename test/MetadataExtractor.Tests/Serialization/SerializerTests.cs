// <copyright file="SerializerTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataExtractor.Tests.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using MetadataExtractor.Models;
    using MetadataExtractor.Serialization;
    using MetadataExtractor.Tests.TestHelpers.Fakes;
    using NodaTime;
    using Xunit;

    [Collection(nameof(Fakes))]
    public class SerializerTests
    {
        private readonly Fakes fakesFixture;

        public SerializerTests(Fakes fakesFixture)
        {
            this.fakesFixture = fakesFixture;
        }

        [Fact]
        public void SerializerShouldWorkWithCsv()
        {
            Recording recording = this.fakesFixture.GetRecording();

            var actual = new MetadataExtractor.Serialization.CsvSerializer().Serialize(new[] { recording });

            // actual property names should exist
            Assert.Contains($",{nameof(Recording.RecommendedName)},", actual);

            // sub-properties should be flattened and prefixed with parent
            Assert.Contains($"{nameof(Recording.CalculatedChecksum)}.{nameof(Checksum.Value)}", actual);

            // noda time type should be registered with csv helper
            Assert.DoesNotContain(nameof(OffsetDateTime.YearOfEra), actual);
            Assert.DoesNotContain(nameof(Duration.BclCompatibleTicks), actual);

            Debug.WriteLine(actual);
        }
    }
}
