// <copyright file="SerializerTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataExtractor.Tests.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MetadataExtractor.Models;
    using MetadataExtractor.Serialization;
    using Xunit;

    public class SerializerTests
    {
        [Fact]
        public void SerializerShouldWorkWithCsv()
        {
            var recording = new Recording();
            recording.CalculatedChecksum = new Checksum()
            {
                Type = "SHA-2-256",
                Value = "abc",
            };

            var actual = Serializer.Serialize(new[] { recording });

            Assert.Contains(nameof(Recording.CalculatedChecksum) + "." + nameof(Checksum.Value), actual);
        }
    }
}
