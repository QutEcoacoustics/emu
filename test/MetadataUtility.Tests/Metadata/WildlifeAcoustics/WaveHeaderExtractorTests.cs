// <copyright file="WaveHeaderExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using MetadataUtility.Audio;
    using MetadataUtility.Metadata;
    using MetadataUtility.Metadata.WildlifeAcoustics.SM4BAT;
    using MetadataUtility.Models;
    using MetadataUtility.Tests.TestHelpers;
    using Xunit;
    using Xunit.Abstractions;

    public class WaveHeaderExtractorTests : TestBase
    {
        private readonly WaveHeaderExtractor subject;

        public WaveHeaderExtractorTests(ITestOutputHelper output)
            : base(output)
        {
            this.subject = new WaveHeaderExtractor(
                this.BuildLogger<WaveHeaderExtractor>());
        }

        public Recording Recording => new();

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(model.ToTargetInformation(this.RealFileSystem));

            // we can process any WAVE file
            var expected = model.IsWave && model.ValidMetadata == ValidMetadata.Yes;
            Assert.Equal(expected, result);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void ProcessFilesWorks(FixtureModel model)
        {
            if (model.IsWave && model.ValidMetadata == ValidMetadata.Yes)
            {
                var recording = await this.subject.ProcessFileAsync(
                    model.ToTargetInformation(this.RealFileSystem),
                    this.Recording);

                recording.DurationSeconds?.Should().Be(model.DurationSeconds);

                recording.SampleRateHertz.Should().Be(model.SampleRateHertz);

                recording.Channels.Should().Be(model.Channels);

                recording.BitsPerSecond.Should().Be(model.BitsPerSecond);

                recording.BitDepth.Should().Be((byte)model.BitDepth);

                recording.FileLengthBytes.Should().Be(model.FileLengthBytes);

                // TODO: Add other assertions here!
            }
        }
    }
}
