// <copyright file="SerializerTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Serialization
{
    using Emu.Models;
    using Emu.Serialization;
    using Emu.Tests.TestHelpers.Fakes;
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
            Recording recording = this.fakesFixture.Recording.Generate();

            string actual = new CsvSerializer().Serialize(new[] { recording });

            // actual property names should exist
            Assert.Contains($",{nameof(Recording.RecommendedName)},", actual);

            // sub-properties should be flattened and prefixed with parent
            Assert.Contains($"{nameof(Recording.CalculatedChecksum)}.{nameof(Checksum.Value)}", actual);

            // noda time type should be registered with csv helper
            Assert.DoesNotContain(nameof(OffsetDateTime.YearOfEra), actual);
            Assert.DoesNotContain(nameof(Duration.BclCompatibleTicks), actual);
        }

        [Fact]
        public void SerializerShouldWorkWithJson()
        {
            Recording recording = this.fakesFixture.Recording.Generate();

            string actual = new JsonSerializer().Serialize(new[] { recording });

            // actual property names should exist
            Assert.Contains($"\"{nameof(Recording.RecommendedName)}\":", actual);

            // sub-properties should not be flattened
            Assert.Contains($"\"{nameof(Recording.CalculatedChecksum)}\":", actual);
            Assert.Contains($"\"{nameof(Checksum.Value)}\":", actual);

            // noda time type should be registered with csv helper
            Assert.DoesNotContain(nameof(OffsetDateTime.YearOfEra), actual);
            Assert.DoesNotContain(nameof(Duration.BclCompatibleTicks), actual);
        }
    }
}
