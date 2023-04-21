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
    using Xunit.Abstractions;
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
        public async Task Works(OutputFormat format, string fixtureName)
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
                _ => throw new NotImplementedException(),
            };

            foreach (var text in expected)
            {
                output.Should().Contain(text);
            }
        }
    }
}
