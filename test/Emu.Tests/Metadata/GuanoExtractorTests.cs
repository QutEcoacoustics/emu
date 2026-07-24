// <copyright file="GuanoExtractorTests.cs" company="QutEcoacoustics">
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

    public class GuanoExtractorTests : TestBase, IClassFixture<FixtureData>
    {
        private readonly FixtureData data;
        private readonly GuanoExtractor subject;

        public GuanoExtractorTests(ITestOutputHelper output, FixtureData data)
            : base(output, realFileSystem: true)
        {
            this.data = data;
            this.subject = new GuanoExtractor(this.BuildLogger<GuanoExtractor>());
        }

        [Theory]
        [ClassData(typeof(FixtureData))]
        public async Task CanProcessFilesWorks(FixtureModel model)
        {
            var result = await this.subject.CanProcessAsync(this.CreateTargetInformation(model));
            result.Should().Be(model.HasGuano);
        }

        [Theory]
        [ClassData(typeof(FixtureData))]
        public async Task ProcessFilesWorks(FixtureModel model)
        {
            Assert.SkipUnless(model.ShouldProcess(FixtureModel.GuanoExtractor, out var expectedRecording), "Not applicable to this fixture");

            var recording = await this.subject.ProcessFileAsync(
                this.CreateTargetInformation(model),
                new Recording());

            recording.StartDate.Should().Be(expectedRecording.StartDate);
            recording.LocalStartDate.Should().Be(expectedRecording.LocalStartDate);
            recording.TrueStartDate.Should().Be(expectedRecording.TrueStartDate);
            recording.Sensor.Should().BeEquivalentTo(expectedRecording.Sensor);
            recording.Location.Should().BeEquivalentTo(expectedRecording.Location);
            recording.OtherFields.Should().BeEquivalentTo(expectedRecording.OtherFields);
        }

        [Fact]
        public async Task ProcessFilesWorksForKnownGuanoFixture()
        {
            var model = this.data[FixtureModel.Sm4HighPrecision];

            var recording = await this.subject.ProcessFileAsync(this.CreateTargetInformation(model), new Recording());

            recording.Sensor.Should().NotBeNull();
            recording.Sensor.Make.Should().Be("Wildlife Acoustics");
            recording.Sensor.Model.Should().Be("SM4");
            recording.Sensor.SerialNumber.Should().Be("S4A04894");
            recording.Sensor.Microphones.Should().HaveCount(2);
            recording.Sensor.Microphones.Should().AllSatisfy(microphone =>
            {
                microphone.Type.Should().Be("Internal");
                microphone.Gain.Should().Be(42.2);
            });
            recording.StartDate.Should().NotBeNull();
            recording.LocalStartDate.Should().NotBeNull();
            recording.OtherFields.Should().NotBeNull();
            recording.OtherFields.Keys.Should().Contain(k => k.StartsWith("(GUANO) WA|", System.StringComparison.Ordinal));
        }

        [Fact]
        public async Task ProcessFilesMapsTitleyVendorFields()
        {
            var model = this.data[FixtureModel.TsChorusGuanoFile1];

            var recording = await this.subject.ProcessFileAsync(this.CreateTargetInformation(model), new Recording());

            recording.Sensor.Should().NotBeNull();
            recording.Sensor.Make.Should().Be("Titley Scientific");
            recording.Sensor.Voltage.Should().Be(4.99);
            recording.Sensor.Microphones.Should().ContainSingle();
            recording.Sensor.Microphones[0].Type.Should().Be("Acoustic");
            recording.Sensor.Microphones[0].Gain.Should().Be(3);
            recording.OtherFields.Should().ContainKey("(GUANO) Anabat|Battery voltage");
        }

        [Fact]
        public async Task ProcessFilesMapsFrontierLabsVendorFields()
        {
            var model = this.data.All.Single(fixture => fixture.Name == "Frontier Labs GUANO File 1");

            var recording = await this.subject.ProcessFileAsync(this.CreateTargetInformation(model), new Recording());

            recording.TrueStartDate.Should().Be(model.Record.TrueStartDate);
            recording.TrueEndDate.Should().Be(model.Record.TrueEndDate);
            recording.MemoryCard.Should().BeEquivalentTo(model.Record.MemoryCard);
            recording.Sensor.Should().BeEquivalentTo(model.Record.Sensor);
            recording.Location.Should().BeEquivalentTo(model.Record.Location);
            recording.OtherFields.Should().ContainKey("(GUANO) FLABS|ScheduleEntry");
        }
    }
}
