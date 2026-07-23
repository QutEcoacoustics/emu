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
    using LanguageExt;
    using Newtonsoft.Json.Linq;
    using NodaTime;
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
            var bytes = Encoding.UTF8.GetBytes(@"GUANO|Version:1.0
Note:line one\nline two
");

            var result = GuanoParser.ParseGuano(bytes);

            result.IsSucc.Should().BeTrue();
            (var guano, var notices) = result.ThrowIfFail();

            guano.GetValue("Note").Should().Be("line one\nline two");
            notices.Should().BeEmpty();
        }

        [Fact]
        public void CanParseSpecExampleGuanoBlock()
        {
            // This is the example from the GUANO specification:
            // https://github.com/riggsd/guano-spec/blob/master/guano_specification.md#example
            var bytes = Encoding.UTF8.GetBytes(@"GUANO|Version:  1.0

Timestamp:  2012-03-29T03:58:01+04:00
Species Auto ID:  MYLU
Species Manual ID:  Myosod
Tags:  hand-release, voucher, workshop
Note:  Hand release of male Indiana Bat caught in triple-high net at Mammoth Cave Historic Ent.\nReleased in low-clutter 100m diameter clearing, bat flew directly overhead, circled once, then darted off into cluttered forest.\n\nRecorded by David Riggs with Pettersson D1000X at 2014 BCM acoustic workshop.
TE:  1
Samplerate:  500000
Length:  6.5
Filter HP:  20.0
Make:  Pettersson
Model:  D1000X
Loc Position:  37.1878016 -86.1057312
Loc Accuracy:  20
Loc Elevation:  228.6

SB|Version:  3.4
SB|Classifier:  US Northeast
SB|DiscrProb:  0.913
SB|Filter:  20kHz Anti-Katydid

PET|Gain:  80
PET|Firmware:  1.0.4 (2009-11-25)
");

            var result = GuanoParser.ParseGuano(bytes);

            result.IsSucc.Should().BeTrue();
            (var guano, var notices) = result.ThrowIfFail();

            notices.Should().BeEmpty();

            guano.GuanoVersion.Should().Be("1.0");
            var expectedOffset = (Either<LocalDateTime, OffsetDateTime>)new OffsetDateTime(new LocalDateTime(2012, 3, 29, 3, 58, 1), Offset.FromHours(4));
            guano.Timestamp.ThrowIfFail().ShouldBe(expectedOffset);

            guano.SpeciesAutoID.Should().BeEquivalentTo(["MYLU"]);
            guano.SpeciesManualID.Should().BeEquivalentTo(["Myosod"]);
            guano.Tags.Should().BeEquivalentTo(["hand-release", "voucher", "workshop"]);

            guano.Note.Should().StartWith("Hand release of male Indiana Bat");
            guano.Note.Should().Contain("\n");

            guano.TE.Should().Be(1u);
            guano.Samplerate.Should().Be(500000u);
            guano.Length.Should().Be(6.5);
            guano.FilterHP.Should().Be(20.0);
            guano.FilterLP.Should().BeNull();

            guano.Make.Should().Be("Pettersson");
            guano.Model.Should().Be("D1000X");

            guano.LocAccuracy.Should().Be(20.0);
            guano.Location.Should().NotBeNull();
            guano.Location.Latitude.Should().BeApproximately(37.1878016, 0.0000001);
            guano.Location.Longitude.Should().BeApproximately(-86.1057312, 0.0000001);
            guano.Location.HorizontalAccuracy.Should().Be(20.0);
            guano.Location.Altitude.Should().Be(228.6);

            guano.PrimaryVendorNamespace.Should().Be("SB");
            guano.Entries.Should().ContainKey(new GuanoKey("SB", "Version"));
            guano.Entries.Should().ContainKey(new GuanoKey("PET", "Gain"));
        }

        [Fact]
        public void GuanoFieldsWithMultipleSpeciesAndTagsAreParsedAsList()
        {
            var bytes = Encoding.UTF8.GetBytes(@"GUANO|Version:1.0
Species Auto ID:  MYLU, EPFU, LANO
Species Manual ID:  Myotis lucifugus, Eptesicus fuscus
Tags:  alpha, beta, gamma
");

            var result = GuanoParser.ParseGuano(bytes);

            result.IsSucc.Should().BeTrue();
            (var guano, var notices) = result.ThrowIfFail();

            guano.SpeciesAutoID.Should().BeEquivalentTo(["MYLU", "EPFU", "LANO"]);
            guano.SpeciesManualID.Should().BeEquivalentTo(["Myotis lucifugus", "Eptesicus fuscus"]);
            guano.Tags.Should().BeEquivalentTo(["alpha", "beta", "gamma"]);
        }

        [Fact]
        public void GuanoTimestampParsesLocalDateTime()
        {
            var bytes = Encoding.UTF8.GetBytes(@"GUANO|Version:1.0
Timestamp: 2015-12-31T23:59:59.123
");

            var result = GuanoParser.ParseGuano(bytes);

            result.IsSucc.Should().BeTrue();
            (var guano, _) = result.ThrowIfFail();

            var expectedLocal = (Either<LocalDateTime, OffsetDateTime>)new LocalDateTime(2015, 12, 31, 23, 59, 59, 123);
            guano.Timestamp.ThrowIfFail().ShouldBe(expectedLocal);
        }

        [Fact]
        public void GuanoTimestampReturnsErrorWhenInvalid()
        {
            var bytes = Encoding.UTF8.GetBytes(@"GUANO|Version:1.0
Timestamp: definitely-not-a-timestamp
");

            var result = GuanoParser.ParseGuano(bytes);

            result.IsSucc.Should().BeTrue();
            (var guano, _) = result.ThrowIfFail();

            guano.Timestamp.IsFail.Should().BeTrue();
        }

        [Fact]
        public void GuanoFieldsReturnNullWhenAbsent()
        {
            var bytes = Encoding.UTF8.GetBytes(@"GUANO|Version:1.0
");

            var result = GuanoParser.ParseGuano(bytes);

            result.IsSucc.Should().BeTrue();
            (var guano, _) = result.ThrowIfFail();

            guano.FilterHP.Should().BeNull();
            guano.FilterLP.Should().BeNull();
            guano.HardwareVersion.Should().BeNull();
            guano.Humidity.Should().BeNull();
            guano.LocAccuracy.Should().BeNull();
            guano.Note.Should().BeNull();
            guano.OriginalFilename.Should().BeNull();
            guano.SpeciesAutoID.Should().BeNull();
            guano.SpeciesManualID.Should().BeNull();
            guano.Tags.Should().BeNull();
            guano.TE.Should().BeNull();
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
