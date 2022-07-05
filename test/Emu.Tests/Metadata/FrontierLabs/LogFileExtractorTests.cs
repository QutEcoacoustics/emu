// <copyright file="LogFileExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata
{
    using Emu.Metadata.FrontierLabs;
    using Emu.Models;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class LogFileExtractorTests : TestBase
    {
        private readonly LogFileExtractor subject;

        public LogFileExtractorTests(ITestOutputHelper output)
            : base(output)
        {
            this.subject = new LogFileExtractor(
                this.BuildLogger<LogFileExtractor>());
        }

        public Recording Recording => new();

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(model.ToTargetInformation(this.RealFileSystem));

            // we can process any file that has Frontier Lab log files
            var expected = model.Process.ContainsKey(FixtureModel.FrontierLabsLogFileExtractor);
            Assert.Equal(expected, result);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void ProcessFilesWorks(FixtureModel model)
        {
            if (model.Process.ContainsKey(FixtureModel.FrontierLabsLogFileExtractor))
            {
                Recording expectedRecording = model.Record;

                if (model.Process[FixtureModel.FrontierLabsLogFileExtractor] != null)
                {
                    expectedRecording = model.Process[FixtureModel.FrontierLabsLogFileExtractor];
                }

                var recording = await this.subject.ProcessFileAsync(
                    model.ToTargetInformation(this.RealFileSystem),
                    this.Recording);

                recording.MemoryCard.Should().BeEquivalentTo(expectedRecording.MemoryCard);
                recording.Sensor.Firmware.Should().Be(expectedRecording.Sensor.Firmware);
                recording.Sensor.SerialNumber.Should().Be(expectedRecording.Sensor.SerialNumber);
                recording.Sensor.PowerSource.Should().Be(expectedRecording.Sensor.PowerSource);
                recording.Sensor.BatteryLevel.Should().Be(expectedRecording.Sensor.BatteryLevel);
                recording.Sensor.Voltage.Should().Be(expectedRecording.Sensor.Voltage);
                recording.Sensor.Microphones.Should().BeEquivalentTo(expectedRecording.Sensor.Microphones);
            }
        }
    }
}
