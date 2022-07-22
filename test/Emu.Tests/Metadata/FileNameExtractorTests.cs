// <copyright file="FileNameExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata
{
    using System.Linq;
    using System.Threading.Tasks;
    using Emu.Metadata;
    using Emu.Models;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using static Emu.Tests.TestHelpers.FixtureHelper;

    public class FileNameExtractorTests : TestBase
    {
        private readonly FilenameExtractor subject;

        public FileNameExtractorTests(ITestOutputHelper output)
            : base(output)
        {
            this.subject = new FilenameExtractor(
                this.BuildLogger<FilenameExtractor>(),
                this.TestFiles,
                this.FilenameParser);
        }

        public Recording Recording => new();

        [Theory]
        [ClassData(typeof(FixtureData))]
        public async Task CanProcessFilesWorks(FixtureModel model)
        {
            // we can process all files that have a filename
            var result = await this.subject.CanProcessAsync(model.ToTargetInformation(this.RealFileSystem));

            Assert.True(result);
        }

        [Theory]
        [ClassData(typeof(FixtureData))]
        public async Task ProcessFilesWorks(FixtureModel model)
        {
            if (model.Process.ContainsKey(FixtureModel.FilenameExtractor))
            {
                Recording expectedRecording = model.Record;

                if (model.Process[FixtureModel.FilenameExtractor] != null)
                {
                    expectedRecording = model.Process[FixtureModel.FilenameExtractor];
                }

                var recording = await this.subject.ProcessFileAsync(
                    model.ToTargetInformation(this.RealFileSystem),
                    this.Recording);

                recording.Extension.Should().Be(expectedRecording.Extension);
                recording.Stem.Should().Be(expectedRecording.Stem);
                recording.StartDate.Should().Be(expectedRecording.StartDate);
                recording.LocalStartDate.Should().Be(expectedRecording.LocalStartDate);
                recording.Location.Should().BeEquivalentTo(expectedRecording.Location);
                recording.FileSizeBytes.Should().Be(expectedRecording.FileSizeBytes);
            }
        }
    }
}
