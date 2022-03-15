// <copyright file="LogFileExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Metadata
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using MetadataUtility.Metadata;
    using MetadataUtility.Metadata.FrontierLabs;
    using MetadataUtility.Models;
    using MetadataUtility.Tests.TestHelpers;
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
            var expected = model.Process.Contains("FrontierLabsLogFileExtractor");
            Assert.Equal(expected, result);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void ProcessFilesWorks(FixtureModel model)
        {
            if (model.Process.Contains("FrontierLabsLogFileExtractor"))
            {
                TargetInformation ti = model.ToTargetInformation(this.RealFileSystem);

                var recording = await this.subject.ProcessFileAsync(
                    ti,
                    this.Recording);

                recording.MemoryCard.Should().BeEquivalentTo(model.MemoryCard);
                recording.Sensor.Firmware.Should().Be(model.Sensor.Firmware);
                recording.Sensor.SerialNumber.Should().Be(model.Sensor.SerialNumber);
                recording.Sensor.PowerSource.Should().Be(model.Sensor.PowerSource);
            }
        }
    }
}
