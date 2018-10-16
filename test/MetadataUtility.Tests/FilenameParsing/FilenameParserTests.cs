// <copyright file="FilenameParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.FilenameParsing
{
    using MetadataUtility.FilenameParsing;
    using MetadataUtility.Filenames;
    using MetadataUtility.Models;
    using MetadataUtility.Tests.TestHelpers;
    using NodaTime;
    using Xunit;

    public class FilenameParserTests
    {
        private readonly FilenameParser parser;

        public FilenameParserTests()
        {
            this.parser = new FilenameParser();
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FilenameParsingFixtureData))]
        public void CanExtractDatesOutOfFileNames(FilenameParsingFixtureModel test)
        {
            var testFilename = test.Filename;

            var actual = this.parser.Parse(testFilename);

            Assert.Equal(test.DateParseable, actual.LocalDateTime.HasValue);

            if (test.DateParseable)
            {
                Assert.Equal(test.ExpectedDateTime.Value, actual.LocalDateTime);

                Assert.Equal(test.ExpectedTzOffset.HasValue, actual.OffsetDateTime.HasValue);

                if (test.ExpectedTzOffset.HasValue)
                {
                    var date = test.ExpectedDateTime.Value.WithOffset(test.ExpectedTzOffset.Value);
                    Assert.Equal(date, actual.OffsetDateTime);
                }
            }

            // construct a "location" from our test data
            if (test.ExpectedLongitude.HasValue)
            {
                var location = new Location()
                {
                    Latitude = test.ExpectedLatitude.Value,
                    Longitude = test.ExpectedLongitude.Value,
                };
                Assert.Equal(location, actual.Location);
            }
            else
            {
                Assert.Null(actual.Location);
            }

            Assert.Equal(test.Prefix, actual.Prefix);
            Assert.Equal(test.Suffix, actual.Suffix);
            Assert.Equal(test.Extension, actual.Extension);

            var actualSuggestedFilename = FilenameSuggester.SuggestName(actual);
            Assert.Equal(test.SuggestedFilename, actualSuggestedFilename);

            Assert.Equal(test.SensorType, actual.SensorType);
            Assert.Equal(test.SensorTypeEstimate, actual.SensorTypeEstimate);

        }
    }
}
