// <copyright file="FlacHeaderExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Metadata
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using MetadataUtility.Audio;
    using MetadataUtility.Metadata.FrontierLabs;
    using MetadataUtility.Models;
    using MetadataUtility.Tests.TestHelpers;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class FlacHeaderExtractorTests : TestBase
    {
        private readonly FlacHeaderExtractor subject;

        public FlacHeaderExtractorTests(ITestOutputHelper output)
            : base(output)
        {
            this.subject = new FlacHeaderExtractor(
                this.BuildLogger<FlacHeaderExtractor>());
        }

        public Recording Recording => new();

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(model.ToTargetInformation(this.RealFileSystem));

            // we can process any file that is Frontier Labs and FLAC
            var expected = model.IsVendor(Vendor.FrontierLabs) && model.IsFlac;
            Assert.Equal(expected, result);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void ProcessFilesWorks(FixtureModel model)
        {
            if (model.Process.Contains("FlacHeaderExtractor"))
            {
                var recording = await this.subject.ProcessFileAsync(
                    model.ToTargetInformation(this.RealFileSystem),
                    this.Recording);

                recording.DurationSeconds.Should().Be(Duration.FromSeconds((double)model.DurationSeconds));
                recording.SampleRateHertz.Should().Be(model.SampleRateHertz);
                recording.Channels.Should().Be(model.Channels);
                recording.BitDepth.Should().Be(model.BitDepth);
                recording.BitsPerSecond.Should().Be(model.BitsPerSecond);
            }
        }
    }
}
