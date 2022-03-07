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
            var expected = model.Process.Contains("LogFileExtractor");
            Assert.Equal(expected, result);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void ProcessFilesWorks(FixtureModel model)
        {
            if (model.Process.Contains("LogFileExtractor"))
            {
                TargetInformation ti = model.ToTargetInformation(this.RealFileSystem);
                ti.KnownSupportFiles.Add("Log file", FixtureHelper.ResolvePath(model.FLLogFile));

                var recording = await this.subject.ProcessFileAsync(
                    ti,
                    this.Recording);

                recording.MemoryCard.SDFormatType.Should().Be(model.SDFormatType);
                recording.MemoryCard.SDManufacturerID.Should().Be(model.SDManufacturerID);
                recording.MemoryCard.SDOEMID.Should().Be(model.SDOEMID);
                recording.MemoryCard.SDProductName.Should().Be(model.SDProductName);
                recording.MemoryCard.SDProductRevision.Should().Be(model.SDProductRevision);
                recording.MemoryCard.SDSerialNumber.Should().Be(model.SDSerialNumber);
                recording.MemoryCard.SDManufactureDate.Should().Be(model.SDManufactureDate);
                recording.MemoryCard.SDSpeed.Should().Be(model.SDSpeed);
                recording.MemoryCard.SDCapacity.Should().Be(model.SDCapacity);
                recording.MemoryCard.SDWrCurrentVmin.Should().Be(model.SDWrCurrentVmin);
                recording.MemoryCard.SDWrCurrentVmax.Should().Be(model.SDWrCurrentVmax);
                recording.MemoryCard.SDWriteB1Size.Should().Be(model.SDWriteB1Size);
                recording.MemoryCard.SDEraseB1Size.Should().Be(model.SDEraseB1Size);
            }
        }
    }
}
