// <copyright file="MetadataCommandTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Commands.Metadata
{
    using System.CommandLine.Parsing;
    using System.Linq;
    using MetadataUtility.Commands;
    using Xunit;

    public class MetadataCommandTests
    {
        [Fact]
        public void HasAMetadataCommandThatComplainsIfNoArgumentsAreGiven()
        {
            var command = "metadata";

            var parser = EmuEntry.BuildCommandLine();
            var result = parser.Parse(command);

            Assert.Equal(1, result.Errors.Count);
            Assert.Equal(
                "Required argument missing for command: metadata",
                result.Errors.First().Message);

            var errorLevel = result.Invoke();

            Assert.Equal(1, errorLevel);
        }
    }
}
