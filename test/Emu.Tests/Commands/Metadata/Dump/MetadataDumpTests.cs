// <copyright file="MetadataDumpTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands.Metadata.Dump
{
    using System;
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Audio.Vendors.WildlifeAcoustics;
    using Emu.Cli.ObjectFormatters;
    using Emu.Commands.Metadata.Dump;
    using Emu.Metadata;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using LanguageExt;
    using static Emu.EmuCommand;

    public class MetadataDumpTests : TestBase, IClassFixture<FixtureData>
    {
        private readonly FixtureData data;

        public MetadataDumpTests(ITestOutputHelper output, FixtureData data)
            : base(output, realFileSystem: true)
        {
            this.data = data;
        }

        // CSV not supported - CSV exclusion test in MetadataDumpCommandTests
        [Theory]
        [InlineData(OutputFormat.Default, FixtureModel.NormalFile)]
        [InlineData(OutputFormat.Compact, FixtureModel.NormalFile)]
        [InlineData(OutputFormat.JSON, FixtureModel.NormalFile)]
        [InlineData(OutputFormat.JSONL, FixtureModel.NormalFile)]
        [InlineData(OutputFormat.Default, FixtureModel.NormalSm3)]
        [InlineData(OutputFormat.Compact, FixtureModel.NormalSm3)]
        [InlineData(OutputFormat.JSON, FixtureModel.NormalSm3)]
        [InlineData(OutputFormat.JSONL, FixtureModel.NormalSm3)]
        [InlineData(OutputFormat.Default, FixtureModel.Sm4HighPrecision)]
        [InlineData(OutputFormat.Compact, FixtureModel.Sm4HighPrecision)]
        [InlineData(OutputFormat.JSON, FixtureModel.Sm4HighPrecision)]
        [InlineData(OutputFormat.JSONL, FixtureModel.Sm4HighPrecision)]
        [InlineData(OutputFormat.Default, FixtureModel.Audiomoth180)]
        [InlineData(OutputFormat.Compact, FixtureModel.Audiomoth180)]
        [InlineData(OutputFormat.JSON, FixtureModel.Audiomoth180)]
        [InlineData(OutputFormat.JSONL, FixtureModel.Audiomoth180)]
        public async Task EachFormatterWorks(OutputFormat format, string fixtureName)
        {
            var command = new MetadataDump(
                this.BuildLogger<MetadataDump>(),
                this.CurrentFileSystem,
                new FileMatcher(this.BuildLogger<FileMatcher>(), this.CurrentFileSystem),
                new OutputRecordWriter(
                    this.Sink,
                    OutputRecordWriter.ChooseFormatter(this.ServiceProvider, format),
                    new Lazy<OutputFormat>(format)),
                new MetadataRegister(this.ServiceProvider),
                new PrettyFormatter(),
                new CompactFormatter())
            {
            };

            var fixture = this.data[fixtureName];
            command.Targets = fixture.AbsoluteFixturePath.AsArray();

            var result = await command.InvokeAsync(null);

            result.Should().Be(0);

            var output = this.AllOutput;

            var path = format is OutputFormat.JSON or OutputFormat.JSONL ? fixture.EscapedAbsoluteFixturePath : fixture.AbsoluteFixtureDirectory;
            output.Should().Contain(path);

            // pick a low-level key name that wouldn't be present in our higher-level
            // metadata show command
            Seq<string> expected = fixture switch
            {
                _ when fixture.IsMake(Vendor.FrontierLabs) =>
                    Seq.create("FL_FLAC_COMMENTS", "SensorFirmwareVersion"),
                _ when fixture.IsMake(Vendor.WildlifeAcoustics) && Models.IsSM3Variant(fixture.Record.Sensor.Model) =>
                    Seq.create("WAMD", "DevSerialNum", "ScenarioMemoryCardC"),
                _ when fixture.IsMake(Vendor.WildlifeAcoustics) && Models.IsSM4Variant(fixture.Record.Sensor.Model) =>
                    Seq.create("WAMD", "ScheduleMode", "LedSettings", "Bitmap2"),
                _ when fixture.IsMake(Vendor.OpenAcousticDevices) =>
                    Seq.create("AudioMothArtistAndComment"),
                _ => throw new NotImplementedException(),
            };

            foreach (var text in expected)
            {
                output.Should().Contain(text);
            }

            if (fixture.HasGuano)
            {
                output.Should().Contain("GUANO");
                output.Should().Contain("Version");
            }
        }

        [Fact]
        public async Task CanFilterToSingleBlockByName()
        {
            var fixture = this.data[FixtureModel.Sm4HighPrecision];

            var command = new MetadataDump(
                this.BuildLogger<MetadataDump>(),
                this.CurrentFileSystem,
                new FileMatcher(this.BuildLogger<FileMatcher>(), this.CurrentFileSystem),
                this.GetOutputRecordWriter(),
                new MetadataRegister(this.ServiceProvider),
                new PrettyFormatter(),
                new CompactFormatter())
            {
                Targets = fixture.AbsoluteFixturePath.AsArray(),
                Blocks = new[] { "GUANO" },
            };

            var result = await command.InvokeAsync(null);
            result.Should().Be(0);

            var output = this.AllOutput;
            output.Should().Contain("GUANO");
            output.Should().Contain("Version");
            output.Should().NotContain("Block WAMD");
        }

        [Fact]
        public async Task CanFilterToSingleBlockByAlias()
        {
            var fixture = this.data[FixtureModel.Sm4HighPrecision];

            var command = new MetadataDump(
                this.BuildLogger<MetadataDump>(),
                this.CurrentFileSystem,
                new FileMatcher(this.BuildLogger<FileMatcher>(), this.CurrentFileSystem),
                this.GetOutputRecordWriter(),
                new MetadataRegister(this.ServiceProvider),
                new PrettyFormatter(),
                new CompactFormatter())
            {
                Targets = fixture.AbsoluteFixturePath.AsArray(),
                Blocks = new[] { "guan" },
            };

            var result = await command.InvokeAsync(null);
            result.Should().Be(0);

            var output = this.AllOutput;
            output.Should().Contain("GUANO");
            output.Should().Contain("Version");
            output.Should().NotContain("Block WAMD");
        }

        [Fact]
        public async Task GuanoDumpHidesRawEntriesButKeepsVendorEntries()
        {
            var fixture = this.data[FixtureModel.Sm4HighPrecision];

            var command = new MetadataDump(
                this.BuildLogger<MetadataDump>(),
                this.CurrentFileSystem,
                new FileMatcher(this.BuildLogger<FileMatcher>(), this.CurrentFileSystem),
                new OutputRecordWriter(
                    this.Sink,
                    OutputRecordWriter.ChooseFormatter(this.ServiceProvider, OutputFormat.Default),
                    new Lazy<OutputFormat>(OutputFormat.Default)),
                new MetadataRegister(this.ServiceProvider),
                new PrettyFormatter(),
                new CompactFormatter())
            {
                Targets = fixture.AbsoluteFixturePath.AsArray(),
                Blocks = new[] { "GUANO" },
            };

            var result = await command.InvokeAsync(null);
            result.Should().Be(0);

            var output = this.AllOutput;
            output.Should().Contain("VendorEntries");
            output.Should().Contain("VendorEntries = Dictionary");
            output.Should().Contain("WA|");
            output.Should().NotContain("GuanoKey {");
            output.Should().NotContain("  Entries =");
        }
    }
}
