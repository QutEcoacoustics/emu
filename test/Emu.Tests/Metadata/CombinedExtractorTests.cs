// <copyright file="CombinedExtractorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata
{
    using System.Linq;
    using Emu.Metadata.FrontierLabs;
    using Emu.Models;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class CombinedExtractorTests : TestBase
    {
        public CombinedExtractorTests(ITestOutputHelper output)
            : base(output)
        {
        }

        public Recording Recording => new();

        /// <summary>
        /// Testing files affected by FL007 (Incorrect log SD card) are processed properly.
        /// The FLAC comment extractor is run first, followed by the log file extractor.
        /// Only the true serial number should remain.
        /// </summary>
        /// <param name="model">The model test file.</param>
        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async System.Threading.Tasks.Task FLCommentAndLogExtractor(FixtureModel model)
        {
            if (model.Process.ContainsKey(FixtureModel.FLCommentAndLogExtractor))
            {
                FlacCommentExtractor commentExtractor = new FlacCommentExtractor(
                    this.BuildLogger<FlacCommentExtractor>());

                LogFileExtractor logExtractor = new LogFileExtractor(
                    this.BuildLogger<LogFileExtractor>());

                var recording = await commentExtractor.ProcessFileAsync(
                    model.ToTargetInformation(this.RealFileSystem),
                    this.Recording);

                recording = await logExtractor.ProcessFileAsync(
                    model.ToTargetInformation(this.RealFileSystem),
                    recording);

                recording.MemoryCard.SerialNumber.Should().Be(model.Record.MemoryCard.SerialNumber);
            }
        }
    }
}
