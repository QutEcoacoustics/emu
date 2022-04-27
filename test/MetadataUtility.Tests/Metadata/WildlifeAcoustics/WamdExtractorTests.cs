// <copyright file="WamdETests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Metadata
{
    using System.Linq;
    using MetadataUtility.Metadata.WildlifeAcoustics;
    using MetadataUtility.Models;
    using MetadataUtility.Tests.TestHelpers;
    using Xunit;
    using Xunit.Abstractions;

    public class WamdExtractorTests : TestBase
    {
        private readonly WamdExtractor subject;

        public WamdExtractorTests(ITestOutputHelper output)
            : base(output)
        {
            this.subject = new WamdExtractor(
                this.BuildLogger<WamdExtractor>());
        }

        public Recording Recording => new();

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(model.ToTargetInformation(this.RealFileSystem));

            // we can process any file that is Frontier Labs and FLAC
            var expected = model.CanProcess.Contains("WamdExtractor");
            Assert.Equal(expected, result);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void ProcessFilesWorks(FixtureModel model)
        {
            if (model.Process.Contains("WamdExtractor"))
            {
                var recording = await this.subject.ProcessFileAsync(
                    model.ToTargetInformation(this.RealFileSystem),
                    this.Recording);

                // Test model vs recording
            }
        }
    }
}
