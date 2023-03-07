// <copyright file="FilenameParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.FilenameParsing
{
    using System;
    using Emu.Dates;
    using Emu.Filenames;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using NodaTime;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;
    using static Emu.Filenames.FilenameParser;
    using static Emu.Tests.TestHelpers.Helpers;

    public class FilenameParserTests : TestBase
    {
        private readonly FilenameGenerator generator;

        public FilenameParserTests(ITestOutputHelper output)
            : base(output)
        {
            this.generator = this.ServiceProvider.GetRequiredService<FilenameGenerator>();
        }

        [Theory]
        [ClassData(typeof(FilenameParsingFixtureData))]
        public void CanParse(FilenameParsingFixtureModel test)
        {
            var actual = this.FilenameParser.Parse(test.Filename);

            if (test.ExpectedDateTime.HasValue)
            {
                // Then local date should have the value {test.ExpectedDateTime}
                actual.LocalStartDate.ShouldBe(test.ExpectedDateTime, $"{test.ExpectedDateTime:o} ≠ {actual.LocalStartDate:o}");

                if (test.ExpectedTzOffset.HasValue)
                {
                    // Given the constructed date offset
                    OffsetDateTime? date = test.ExpectedDateTime?.WithOffset(test.ExpectedTzOffset.Value);

                    // Then the offset date should have the expected value {test.ExpectedDateTime}{test.ExpectedTzOffset}
                    actual.StartDate.ShouldBe(date, $"{date:o} ≠ {actual.StartDate:o}");
                }
                else
                {
                    // But the offset date should NOT have a value
                    Assert.Null(actual.StartDate);
                }
            }
            else
            {
                // Then local date should NOT have a value
                Assert.Null(actual.LocalStartDate);
            }

            if (test.ExpectedLongitude.HasValue)
            {
                // And then the latitude parsed in this filename should be {test.ExpectedLatitude}
                Assert.Equal(test.ExpectedLatitude.Value, (double)actual.Location.Latitude, Wgs84Epsilon);

                // and the longitude should be {test.ExpectedLongitude}
                Assert.Equal(test.ExpectedLongitude.Value, (double)actual.Location.Longitude, Wgs84Epsilon);

                // and the location sample date should be set
                Assert.Equal(actual.StartDate?.ToInstant(), actual.Location.SampleDateTime);
            }
            else
            {
                // And in this case we do not find a location
                    Assert.Null(actual.Location);
            }

            // And the extension field should be set
            Assert.Equal(test.Extension, actual.Extension);
        }

        [Theory]
        [ClassData(typeof(FilenameParsingFixtureData))]
        public void ParsesTokenizedNameCorrectly(FilenameParsingFixtureModel test)
        {
            var actual = this.FilenameParser.Parse(test.Filename);

            actual.TokenizedName.Should().Be(test.TokenizedName);
        }

        [Theory]
        [ClassData(typeof(FilenameParsingFixtureData))]
        public void CanReconstructTheFileName(FilenameParsingFixtureModel test)
        {
            var parsed = this.FilenameParser.Parse(test.Filename);

            var actual = this.generator.Reconstruct(parsed);

            actual.Should().Be(test.NormalizedName);
        }

        [Fact]
        public void WillThrowIfConstructedWithNulls()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => new FilenameParser(this.TestFiles, this.generator, null, null));

            Assert.Equal("No date variants were given to filename parser", exception.Message);
        }

        [Fact]
        public void WillThrowIfConstructedWithEmpty()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => new FilenameParser(
                    this.TestFiles,
                    this.generator,
                    Array.Empty<DateVariant<LocalDateTime>>(),
                    Array.Empty<DateVariant<OffsetDateTime>>()));

            Assert.Equal("No date variants were given to filename parser", exception.Message);
        }
    }
}
