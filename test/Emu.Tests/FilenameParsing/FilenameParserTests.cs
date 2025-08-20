// <copyright file="FilenameParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.FilenameParsing
{
    using System;
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

        [Fact]
        public void SpecialCaseForStartAndEndFormat()
        {
            // Given a filename with start and end date
            var filename = "S20240815T091156.982648+1000_E20240815T091251.967555+1000_-12.34567+78.98102.wav";

            // When we parse it
            var actual = this.FilenameParser.Parse(filename);

            // Then the start date should be parsed correctly
            actual.LocalStartDate.ShouldBe(new LocalDateTime(2024, 8, 15, 9, 11, 56).PlusNanoseconds(982648000));
            actual.StartDate.ShouldBe(new OffsetDateTime(new LocalDateTime(2024, 8, 15, 9, 11, 56).PlusNanoseconds(982648000), Offset.FromHours(10)));

            // And the end date should be parsed correctly
            actual.EndDate.ShouldBe(new OffsetDateTime(new LocalDateTime(2024, 8, 15, 9, 12, 51).PlusNanoseconds(967555000), Offset.FromHours(10)));

            // all other fields tested in the normal tests
        }

        [Fact]
        public void CanParseFilenameWithSquareBrackets()
        {
            var filename = "/20221010T010000+0000_REC [-38.36231+145.31787].wav";

            var actual = this.FilenameParser.Parse(filename);

            actual.Location.Latitude.Should().BeApproximately(-38.36231, Wgs84Epsilon);
            actual.Location.Longitude.Should().BeApproximately(145.31787, Wgs84Epsilon);

            // we should have matched the square brackets as part of the location expression.
            actual.TokenizedName.Should().NotContain("[");
        }
    }
}
