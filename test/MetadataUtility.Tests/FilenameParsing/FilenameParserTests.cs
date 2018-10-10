// <copyright file="FilenameParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.FilenameParsing
{
    using MetadataUtility.FilenameParsing;
    using MetadataUtility.Tests.TestHelpers;
    using Xunit;

    public class FilenameParserTests
    {
        private FilenameParser parser;

        public FilenameParserTests()
        {
            parser = new FilenameParser();
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FilenameParsingFixtureData))]
        public void CanExtractDatesOutOfFileNames(FilenameParsingFixtureModel test)
        {
            var testFilename = test.Filename;

            var actual = this.parser.Parse(testFilename);

            Assert.Equal(test.);
            if (!test.DateParseable)
            {
                Assert.False(actual.HasDate);
            }


        }
    }
}
