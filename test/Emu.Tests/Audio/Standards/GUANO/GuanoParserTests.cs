// <copyright file="GuanoParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Standards.GUANO
{
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

            guano.Version.Should().Be("1.0");
            guano.Entries.Should().Contain(x => x.Namespaces.SequenceEqual(new[] { "GUANO" }) && x.Field == "Version");
            guano.Entries.Should().Contain(x => x.Namespaces.Count == 0 && x.Field == "Timestamp");
            guano.Entries.Should().Contain(x => x.Namespaces.Count == 0 && x.Field == "Make");
            guano.Entries.Should().Contain(x => x.Namespaces.Count == 0 && x.Field == "Model");
            guano.Entries.Should().Contain(x => x.Namespaces.Count == 0 && x.Field == "Serial");
            guano.Entries.Should().Contain(x => x.Namespaces.SequenceEqual(new[] { "WA", "Kaleidoscope" }) && x.Field == "Version");
            guano.Entries.Should().Contain(x => x.Namespaces.SequenceEqual(new[] { "WA", "Song Meter" }) && x.Field == "Audio settings");
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

            var entry = guano.GetValue(["WA", "Song Meter"], "Audio settings");

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
                Entries =
                [
                    new GuanoEntry { Namespaces = ["GUANO"], Field = "Version", Value = "1.0" },
                    new GuanoEntry { Field = "Make", Value = "Unknown" },
                ],
            };

            guano.PrimaryVendorNamespace.Should().BeNull();
        }
    }
}
