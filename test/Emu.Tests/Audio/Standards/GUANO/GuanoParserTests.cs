// <copyright file="GuanoParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Standards.GUANO
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Emu.Audio.Standards.GUANO;
    using Emu.Models.Notices;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using Shouldly;
    using Xunit;

    public class GuanoParserTests : TestBase, IClassFixture<FixtureData>
    {
        private readonly FixtureData data;

        public GuanoParserTests(ITestOutputHelper output, FixtureData data)
            : base(output, realFileSystem: true)
        {
            this.data = data;
        }

        [Fact]
        public void CanDetectGuanoChunkInKnownFixture()
        {
            var fixture = this.data[FixtureModel.Sm4HighPrecision];
            using var stream = fixture.ToFileInfo(this.CurrentFileSystem).OpenRead();

            var hasGuano = GuanoParser.HasGuanoChunk(stream).IfFail(false);

            hasGuano.Should().BeTrue();
        }

        [Fact]
        public void CanParseGuanoFieldsInKnownFixture()
        {
            var fixture = this.data[FixtureModel.Sm4HighPrecision];
            using var stream = fixture.ToFileInfo(this.CurrentFileSystem).OpenRead();

            var result = GuanoParser.ReadGuanoBlock(stream);

            result.IsSucc.Should().BeTrue();
            (var guano, var notices) = result.ThrowIfFail();

            guano.GuanoVersion.Should().Be("1.0");
            guano.Entries.Should().ContainKey(new GuanoKey("GUANO", "Version"));
            guano.Entries.Should().ContainKey(new GuanoKey("Timestamp"));
            guano.Entries.Should().ContainKey(new GuanoKey("Make"));
            guano.Entries.Should().ContainKey(new GuanoKey("Model"));
            guano.Entries.Should().ContainKey(new GuanoKey("Serial"));
            guano.Entries.Should().ContainKey(new GuanoKey("WA", "Kaleidoscope", "Version"));
            guano.Entries.Should().ContainKey(new GuanoKey("WA", "Song Meter", "Audio settings"));
            guano.PrimaryVendorNamespace.Should().Be("WA");
            notices.Count(n => n is Warning).Should().Be(0);
        }

        [Fact]
        public void CanParseSmartQuoteAudioSettingsAndReportNotice()
        {
            var fixture = this.data[FixtureModel.SongMeterMiniNormalFile1];
            using var stream = fixture.ToFileInfo(this.CurrentFileSystem).OpenRead();

            var result = GuanoParser.ReadGuanoBlock(stream);

            result.IsSucc.Should().BeTrue();
            (var guano, var notices) = result.ThrowIfFail();

            var entry = guano.GetValue(Emu.Metadata.WildlifeAcoustics.Guano.AudioSettingsKey);

            // should not contain smart quotes, and should be parseable json
            entry.Should().NotContain("\u201C");
            entry.Should().NotContain("\u201D");

            Should.NotThrow(() => JArray.Parse(entry));

            notices.Should().ContainSingle();
            notices.Single().Message.Should().Be("Normalized smart quotes in `WA|Song Meter|Audio settings` JSON.");
        }

        [Fact]
        public void ParseGuanoUnescapesLiteralNewlinesInValues()
        {
            var bytes = Encoding.UTF8.GetBytes(
                "GUANO|Version:1.0\n" +
                "Note:line one\\nline two\n");

            var result = GuanoParser.ParseGuano(bytes);

            result.IsSucc.Should().BeTrue();
            (var guano, var notices) = result.ThrowIfFail();

            guano.GetValue("Note").Should().Be("line one\nline two");
            notices.Should().BeEmpty();
        }

        [Fact]
        public void GuanoWithoutVendorEntriesHasNoPrimaryVendorNamespace()
        {
            var guano = new GuanoBlock
            {
                Entries = new Dictionary<GuanoKey, string>
                {
                    [new GuanoKey("GUANO", "Version")] = "1.0",
                    [new GuanoKey("Make")] = "Unknown",
                },
            };

            guano.PrimaryVendorNamespace.Should().BeNull();
        }
    }
}
