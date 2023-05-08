// <copyright file="WamdExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata
{
    using System.Threading.Tasks;
    using Emu.Metadata.WildlifeAcoustics;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class WamdExtractorTests : TestBase
    {
        private readonly WamdExtractor subject;

        public WamdExtractorTests(ITestOutputHelper output)
            : base(output, realFileSystem: true)
        {
            this.subject = new WamdExtractor(
                this.BuildLogger<WamdExtractor>());
        }

        [Theory]
        [ClassData(typeof(FixtureData))]
        public async Task CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(this.CreateTargetInformation(model));

            // we can process any file that is Wildlife Acoustics
            var expected = model.Process.ContainsKey(FixtureModel.WamdExtractor);
            Assert.Equal(expected, result);
        }

        [SkippableTheory]
        [ClassData(typeof(FixtureData))]
        public async Task ProcessFilesWorks(FixtureModel model)
        {
            Skip.IfNot(model.ShouldProcess(FixtureModel.WamdExtractor, out var expectedRecording));

            var recording = await this.subject.ProcessFileAsync(
                this.CreateTargetInformation(model),
                new());

            recording.StartDate.Should().Be(expectedRecording.StartDate);
            recording.Sensor.Make.Should().Be(expectedRecording.Sensor.Make);
            recording.Sensor.Model.Should().Be(expectedRecording.Sensor.Model);
            recording.Sensor.Name.Should().Be(expectedRecording.Sensor.Name);
            recording.Sensor.SerialNumber.Should().Be(expectedRecording.Sensor.SerialNumber);
            recording.Sensor.Firmware.Should().Be(expectedRecording.Sensor.Firmware);
            recording.Sensor.Temperature.Should().Be(expectedRecording.Sensor.Temperature);
            recording.Sensor.TemperatureExternal.Should().Be(expectedRecording.Sensor.TemperatureExternal);
            recording.Sensor.Microphones.Should().BeEquivalentTo(expectedRecording.Sensor.Microphones);

            recording.Location.Should().BeEquivalentTo(expectedRecording.Location);

            recording.Sensor.Microphones.Should().BeEquivalentTo(expectedRecording.Sensor.Microphones);

            recording.TrueStartDate.Should().Be(expectedRecording.TrueStartDate);
            recording.TrueEndDate.Should().Be(expectedRecording.TrueEndDate);
        }
    }
}
