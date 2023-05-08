// <copyright file="CommentExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata
{
    using System.Threading.Tasks;
    using Emu.Metadata.OpenAcousticDevices;
    using Emu.Models;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class CommentExtractorTests : TestBase
    {
        private readonly AudioMothCommentExtractor subject;

        public CommentExtractorTests(ITestOutputHelper output)
            : base(output, realFileSystem: true)
        {
            this.subject = new AudioMothCommentExtractor(
                this.BuildLogger<AudioMothCommentExtractor>(),
                this.FilenameParser);
        }

        [Theory]
        [ClassData(typeof(FixtureData))]
        public async Task CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(this.CreateTargetInformation(model));

            var expected = model.Process.ContainsKey(FixtureModel.AudioMothCommentExtractor);
            Assert.Equal(expected, result);
        }

        [SkippableTheory]
        [ClassData(typeof(FixtureData))]
        public async Task ProcessFilesWorks(FixtureModel model)
        {
            Skip.IfNot(model.ShouldProcess(FixtureModel.AudioMothCommentExtractor, out var expectedRecording));

            var recording = await this.subject.ProcessFileAsync(
                this.CreateTargetInformation(model),
                new Recording());

            recording.Sensor.Should().BeEquivalentTo(expectedRecording.Sensor);
            recording.MemoryCard.Should().BeEquivalentTo(expectedRecording.MemoryCard);
            recording.Location.Should().BeEquivalentTo(expectedRecording.Location);

            recording.StartDate.Should().Be(expectedRecording.StartDate);
            recording.TrueStartDate.Should().Be(expectedRecording.TrueStartDate);
            recording.TrueEndDate.Should().Be(expectedRecording.TrueEndDate);

            recording.RecordingStatus.Should().Be(expectedRecording.RecordingStatus);
            recording.Notices.Should().BeEquivalentTo(expectedRecording.Notices);
        }
    }
}
