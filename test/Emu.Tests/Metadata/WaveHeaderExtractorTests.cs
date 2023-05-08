// <copyright file="WaveHeaderExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata
{
    using System.Threading.Tasks;
    using Emu.Fixes.FrontierLabs;
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
            : base(output, realFileSystem: true)
        {
            this.subject = new WaveHeaderExtractor(
                this.BuildLogger<WaveHeaderExtractor>(),
                this.ServiceProvider.GetRequiredService<DataSizeOffBy44>());
        }

        [Theory]
        [ClassData(typeof(FixtureData))]
        public async Task CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(this.CreateTargetInformation(model));

            // we can process any WAVE file
            var expected = model.IsWave && model.ValidMetadata == ValidMetadata.Yes;
            Assert.Equal(expected, result);
        }

        [SkippableTheory]
        [ClassData(typeof(FixtureData))]
        public async Task ProcessFilesWorks(FixtureModel model)
        {
            Skip.IfNot(model.IsWave && model.ValidMetadata == ValidMetadata.Yes);

            Recording expectedRecording = model.Record;

            Recording recording = new();

            recording = await this.subject.ProcessFileAsync(
                this.CreateTargetInformation(model),
                recording);

            recording.DurationSeconds?.Should().Be(expectedRecording.DurationSeconds);
            recording.TotalSamples.Should().Be(expectedRecording.TotalSamples);
            recording.SampleRateHertz.Should().Be(expectedRecording.SampleRateHertz);
            recording.Channels.Should().Be(expectedRecording.Channels);
            recording.BitsPerSecond.Should().Be(expectedRecording.BitsPerSecond);
            recording.BitDepth.Should().Be(expectedRecording.BitDepth);
            recording.MediaType.Should().Be(expectedRecording.MediaType);
        }
    }
}
