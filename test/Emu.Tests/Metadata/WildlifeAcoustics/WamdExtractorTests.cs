// <copyright file="WamdExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata
{
    using System.Threading.Tasks;
    using Emu.Metadata.WildlifeAcoustics;
    using Emu.Models;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
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
        public async Task CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(model.ToTargetInformation(this.RealFileSystem));

            // we can process any file that is Wildlife Acoustics
            var expected = model.Process.ContainsKey(FixtureModel.WamdExtractor);
            Assert.Equal(expected, result);
        }

        [SkippableTheory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async Task ProcessFilesWorks(FixtureModel model)
        {
            Skip.IfNot(model.ShouldProcess(FixtureModel.WamdExtractor, out var expectedRecording));

            var recording = await this.subject.ProcessFileAsync(
                model.ToTargetInformation(this.RealFileSystem),
                this.Recording);

            recording.StartDate.Should().Be(expectedRecording.StartDate);
            recording.Sensor.Name.Should().Be(expectedRecording.Sensor.Name);
            recording.Sensor.SerialNumber.Should().Be(expectedRecording.Sensor.SerialNumber);
            recording.Sensor.Firmware.Should().Be(expectedRecording.Sensor.Firmware);
            recording.Sensor.Temperature.Should().Be(expectedRecording.Sensor.Temperature);
            recording.Sensor.Microphones.Should().BeEquivalentTo(expectedRecording.Sensor.Microphones);

            recording.Location.Should().BeEquivalentTo(expectedRecording.Location);

            recording.TrueStartDate.Should().Be(expectedRecording.TrueStartDate);
            recording.TrueEndDate.Should().Be(expectedRecording.TrueEndDate);
        }
    }
}
