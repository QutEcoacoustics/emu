// <copyright file="FlacHeaderExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata
{
    using Emu.Audio;
    using Emu.Metadata.FrontierLabs;
    using Emu.Models;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
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
        public async System.Threading.Tasks.Task CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(model.ToTargetInformation(this.RealFileSystem));

            // we can process any file that is Frontier Labs and FLAC
            var expected = model.IsVendor(Vendor.FrontierLabs) && model.IsFlac;
            Assert.Equal(expected, result);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async System.Threading.Tasks.Task ProcessFilesWorks(FixtureModel model)
        {
            if (model.Process.ContainsKey(FixtureModel.FlacHeaderExtractor))
            {
                Recording expectedRecording = model.Record;

                var recording = await this.subject.ProcessFileAsync(
                    model.ToTargetInformation(this.RealFileSystem),
                    this.Recording);

                recording.DurationSeconds.Should().Be(expectedRecording.DurationSeconds);
                recording.SampleRateHertz.Should().Be(expectedRecording.SampleRateHertz);
                recording.Channels.Should().Be(expectedRecording.Channels);
                recording.BitDepth.Should().Be(expectedRecording.BitDepth);
                recording.BitsPerSecond.Should().Be(expectedRecording.BitsPerSecond);
                recording.EmbeddedChecksum.Should().Be(expectedRecording.EmbeddedChecksum);
            }
        }
    }
}
