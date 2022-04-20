// <copyright file="FlacCommentExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Metadata
{
    using System.Linq;
    using FluentAssertions;
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
            var expected = model.CanProcess.Contains("FlacCommentExtractor");
            Assert.Equal(expected, result);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void ProcessFilesWorks(FixtureModel model)
        {
            if (model.Process.Contains("FlacCommentExtractor"))
            {
                var recording = await this.subject.ProcessFileAsync(
                    model.ToTargetInformation(this.RealFileSystem),
                    this.Recording);

                recording.Sensor.Firmware.Should().Be(model.Sensor.Firmware);
                recording.Sensor.Microphones.Should().BeEquivalentTo(model.Sensor.Microphones);
                recording.Sensor.BatteryLevel.Should().Be(model.Sensor.BatteryLevel);
                recording.Sensor.LastTimeSync.Should().Be(model.Sensor.LastTimeSync);
                (recording.Location?.Longitude).Should().Be(model.Location.Longitude);
                (recording.Location?.Latitude).Should().Be(model.Location.Latitude);
                recording.StartDate.Should().Be(model.StartDate);
                recording.EndDate.Should().Be(model.EndDate);
                recording.MemoryCard.ManufacturerID.Should().Be(model.MemoryCard.ManufacturerID);
                recording.MemoryCard.OEMID.Should().Be(model.MemoryCard.OEMID);
                recording.MemoryCard.ProductName.Should().Be(model.MemoryCard.ProductName);
                recording.MemoryCard.ProductRevision.Should().Be(model.MemoryCard.ProductRevision);
                recording.MemoryCard.SerialNumber.Should().Be(model.MemoryCard.SerialNumber);
                recording.MemoryCard.ManufactureDate.Should().Be(model.MemoryCard.ManufactureDate);
            }
        }
    }
}
