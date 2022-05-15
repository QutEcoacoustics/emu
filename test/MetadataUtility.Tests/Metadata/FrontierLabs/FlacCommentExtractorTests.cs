// <copyright file="FlacCommentExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Metadata
{
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
            var expected = model.Process.ContainsKey(FixtureModel.FlacCommentExtractor);
            Assert.Equal(expected, result);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void ProcessFilesWorks(FixtureModel model)
        {
            if (model.Process.ContainsKey(FixtureModel.FlacCommentExtractor))
            {
                Recording expectedRecording = model.Record;

                var recording = await this.subject.ProcessFileAsync(
                    model.ToTargetInformation(this.RealFileSystem),
                    this.Recording);

                recording.Sensor.Firmware.Should().Be(expectedRecording.Sensor.Firmware);
                recording.Sensor.Microphones.Should().BeEquivalentTo(expectedRecording.Sensor.Microphones);
                recording.Sensor.BatteryLevel.Should().Be(expectedRecording.Sensor.BatteryLevel);
                recording.Sensor.LastTimeSync.Should().Be(expectedRecording.Sensor.LastTimeSync);
                (recording.Location?.Longitude).Should().Be(expectedRecording.Location.Longitude);
                (recording.Location?.Latitude).Should().Be(expectedRecording.Location.Latitude);
                recording.StartDate.Should().Be(expectedRecording.StartDate);
                recording.EndDate.Should().Be(expectedRecording.EndDate);
                recording.MemoryCard.ManufacturerID.Should().Be(expectedRecording.MemoryCard.ManufacturerID);
                recording.MemoryCard.OEMID.Should().Be(expectedRecording.MemoryCard.OEMID);
                recording.MemoryCard.ProductName.Should().Be(expectedRecording.MemoryCard.ProductName);
                recording.MemoryCard.ProductRevision.Should().Be(expectedRecording.MemoryCard.ProductRevision);
                recording.MemoryCard.SerialNumber.Should().Be(expectedRecording.MemoryCard.SerialNumber);
                recording.MemoryCard.ManufactureDate.Should().Be(expectedRecording.MemoryCard.ManufactureDate);
            }
        }
    }
}
