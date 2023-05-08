// <copyright file="ConfigFileExtractorTests.cs" company="QutEcoacoustics">
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

    public class ConfigFileExtractorTests : TestBase
    {
        private readonly ConfigExtractor subject;

        public ConfigFileExtractorTests(ITestOutputHelper output)
            : base(output, realFileSystem: true)
        {
            this.subject = new ConfigExtractor();
        }

        [Theory]
        [ClassData(typeof(FixtureData))]
        public async Task CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(this.CreateTargetInformation(model));

            var expected = model.Process.ContainsKey(FixtureModel.AudioMothConfigFileExtractor);
            Assert.Equal(expected, result);
        }

        [SkippableTheory]
        [ClassData(typeof(FixtureData))]
        public async Task ProcessFilesWorks(FixtureModel model)
        {
            Skip.IfNot(model.ShouldProcess(FixtureModel.AudioMothConfigFileExtractor, out var expectedRecording));

            var recording = await this.subject.ProcessFileAsync(
                this.CreateTargetInformation(model),
                new());

            recording.Sensor.Should().BeEquivalentTo(expectedRecording.Sensor);
        }
    }
}
