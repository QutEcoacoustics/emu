// <copyright file="FlacCommentExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Metadata
{
    using System.Linq;
    using MetadataUtility.Metadata.FrontierLabs;
    using MetadataUtility.Models;
    using MetadataUtility.Tests.TestHelpers;
    using Xunit;
    using Xunit.Abstractions;

    public class FlacCommentExtractorTests : TestBase
    {
        private readonly FlacCommentExtractor subject;

        public FlacCommentExtractorTests(ITestOutputHelper output)
            : base(output)
        {
            this.subject = new FlacCommentExtractor(
                this.BuildLogger<FlacCommentExtractor>());
        }

        public Recording Recording => new();

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(model.ToTargetInformation(this.RealFileSystem));

            // we can process any file that is Frontier Labs and FLAC
            var expected = model.Process.Contains("FlacCommentExtractor");
            Assert.Equal(expected, result);
        }
    }
}
