// <copyright file="WaveHeaderExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata
{
    using Emu.Metadata;
    using Emu.Models;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
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
        public async System.Threading.Tasks.Task CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(model.ToTargetInformation(this.RealFileSystem));

            // we can process any WAVE file
            var expected = model.IsWave && model.ValidMetadata == ValidMetadata.Yes;
            Assert.Equal(expected, result);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async System.Threading.Tasks.Task ProcessFilesWorks(FixtureModel model)
        {
            if (model.IsWave && model.ValidMetadata == ValidMetadata.Yes)
            {
                Recording expectedRecording = model.Record;

                var recording = await this.subject.ProcessFileAsync(
                    model.ToTargetInformation(this.RealFileSystem),
                    this.Recording);

                recording.DurationSeconds?.Should().Be(expectedRecording.DurationSeconds);
                recording.TotalSamples.Should().Be(expectedRecording.TotalSamples);
                recording.SampleRateHertz.Should().Be(expectedRecording.SampleRateHertz);
                recording.Channels.Should().Be(expectedRecording.Channels);
                recording.BitsPerSecond.Should().Be(expectedRecording.BitsPerSecond);
                recording.BitDepth.Should().Be(expectedRecording.BitDepth);
            }
        }
    }
}
