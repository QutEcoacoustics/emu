// <copyright file="FilenameParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.FilenameParsing
{
    using Shouldly;
    using MetadataUtility.Filenames;
    using MetadataUtility.Models;
    using MetadataUtility.Tests.TestHelpers;
    using NodaTime;
    using Xbehave;
    using Xunit;
    using static MetadataUtility.Tests.TestHelpers.TestHelpers;

    public class FilenameParserTests
    {
        private FilenameParser parser;

        public FilenameParserTests()
        {
        }

        [Background]
        public void Background()
        {
            $"Given a {nameof(FilenameParser)} using the default formats"
                .x(() => this.parser = FilenameParser.Default);
        }

        [Scenario]
        [ClassData(typeof(FixtureHelper.FilenameParsingFixtureData))]
        public void CanParse(FilenameParsingFixtureModel test)
        {
            $"Given {test}".x(Nop);

            ParsedFilename actual = null;
            "When I parse the filename".x(() => actual = this.parser.Parse(test.Filename));

            if (test.ExpectedDateTime.HasValue)
            {
                $"Then local date should have the value {test.ExpectedDateTime}"
                    .x(() => actual.LocalDateTime.ShouldBe(test.ExpectedDateTime, () => $"{test.ExpectedDateTime:o} ≠ {actual.LocalDateTime:o}"));

                if (test.ExpectedTzOffset.HasValue)
                {
                    OffsetDateTime? date = null;
                    "Given the constructed date offset"
                        .x(() => date = test.ExpectedDateTime?.WithOffset(test.ExpectedTzOffset.Value));

                    $"Then the offset date should have the expected value {test.ExpectedDateTime}{test.ExpectedTzOffset}"
                        .x(() => actual.OffsetDateTime.ShouldBe(date, () => $"{date:o} ≠ {actual.OffsetDateTime:o}"));
                }
                else
                {
                    "But the offset date should NOT have a value"
                        .x(() => Assert.Null(actual.OffsetDateTime));
                }
            }
            else
            {
                "Then local date should NOT have a value"
                    .x(() => Assert.Null(actual.LocalDateTime));
            }

            if (test.ExpectedLongitude.HasValue)
            {
                $"And then the latitude parsed in this filename should be {test.ExpectedLatitude}"
                    .x(() => Assert.Equal(test.ExpectedLatitude, actual.Location.Latitude));

                $"and the longitude should be {test.ExpectedLongitude}"
                    .x(() => Assert.Equal(test.ExpectedLongitude, actual.Location.Longitude));
            }
            else
            {
                "And in this case we do not find a location"
                    .x(() => Assert.Null(actual.Location));
            }

//
//            Assert.Equal(test.Prefix, actual.Prefix);
//            Assert.Equal(test.Suffix, actual.Suffix);
//            Assert.Equal(test.Extension, actual.Extension);
//
//            var actualSuggestedFilename = FilenameSuggester.SuggestName(actual);
//            Assert.Equal(test.SuggestedFilename, actualSuggestedFilename);
//
//            Assert.Equal(test.SensorType, actual.SensorType);
//            Assert.Equal(test.SensorTypeEstimate, actual.SensorTypeEstimate);

        }
    }
}
