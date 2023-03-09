// <copyright file="WamdTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Vendors.WildlifeAcoustics.WAMD
{
    using System.IO;
    using Emu.Audio;
    using Emu.Audio.Vendors.WildlifeAcoustics.WAMD;
    using Emu.Models;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using NodaTime;
    using NodaTime.Text;
    using Xunit;
    using Xunit.Abstractions;

    public class WamdTests : TestBase, IClassFixture<FixtureData>
    {
        private readonly FixtureData data;

        public WamdTests(ITestOutputHelper output, FixtureData data)
            : base(output, realFileSystem: true)
        {
            this.data = data;
        }

        [Fact]
        public void HasVersion1WamdChunkTest()
        {
            var fixture = this.data[FixtureModel.Sm4BatNormal1];
            var wamdFile = this.RealFileSystem.File.OpenRead(fixture.AbsoluteFixturePath);
            var hasWamd = WamdParser.HasVersion1WamdChunk(wamdFile).IfFail(false);
            Assert.True(hasWamd);
        }

        [Fact]
        public void DoesNotHaveVersion1WamdChunkTest()
        {
            var fixture = this.data[FixtureModel.NormalFile];
            var noWamdFile = this.RealFileSystem.File.OpenRead(fixture.AbsoluteFixturePath);
            var hasWamd = WamdParser.HasVersion1WamdChunk(noWamdFile).IfFail(false);
            Assert.False(hasWamd);
        }

        [Fact]
        public void ExtractMetadataTest()
        {
            var fixture = this.data[FixtureModel.Sm4BatNormal1];
            using var stream = fixture.ToFileInfo(this.CurrentFileSystem).OpenRead();

            var tryWamdData = WamdParser.ExtractMetadata(stream);

            Assert.True(tryWamdData.IsSucc);

            var wamdData = (Wamd)tryWamdData;

            ((OffsetDateTime)wamdData.FileStartTime).Should().Be(OffsetDateTimePattern.CreateWithInvariantCulture("G").Parse("2021-06-21T20:57:06-03:00").Value);
            wamdData.DevModel.Should().Be("SM4BAT-FS");
            wamdData.DevSerialNum.Should().Be("S4U09523");
            wamdData.SwVersion.Should().Be("2.2.1");
            wamdData.DevName.Should().Be("S4U09523");
            wamdData.TempInt.Should().Be(24.25);
            wamdData.MicType.Should().AllBe("U2");
            wamdData.MicSensitivity.Should().AllBeEquivalentTo(13.0);
            wamdData.PosLast.Latitude.Should().Be(45.7835);
            wamdData.PosLast.Longitude.Should().Be(-64.23352);
        }

        [Fact]
        public void ExtractMetadataTestSm4Gps()
        {
            var fixture = this.data[FixtureModel.Sm4HighPrecision];
            using var stream = fixture.ToFileInfo(this.CurrentFileSystem).OpenRead();

            var tryWamdData = WamdParser.ExtractMetadata(stream);

            Assert.True(tryWamdData.IsSucc);

            var wamdData = (Wamd)tryWamdData;

            // in order by which the values were inspected in the file (via hex editor)
            wamdData.Version.Should().Be(1);
            wamdData.TimeExpansion.Should().Be(1);
            wamdData.DevModel.Should().Be("SM4");
            wamdData.DevSerialNum.Should().Be("S4A04894");
            wamdData.SwVersion.Should().Be("2.1.1");
            wamdData.DevName.Should().Be("FNQ-RBS");

            wamdData.FileStartTime.Should().NotBeNull();
            var start = wamdData.FileStartTime.Value;
            ((OffsetDateTime)start).Should().Be(
                new OffsetDateTime(
                    new LocalDateTime(2019, 01, 02, 04, 48, 02, 010),
                    Offset.FromHours(10)));

            wamdData.Software.Should().Be("Kaleidoscope 4.5.5");
            wamdData.LicenseId.Should().Be("KP-MFU8XF2A");
            wamdData.MicType.Should().BeEquivalentTo("IN", "IN");
            wamdData.MicSensitivity.Should().BeEquivalentTo(new[] { 45.2, 45.2 });
            wamdData.TempInt.Should().Be(21.75);
            wamdData.PosLast.Should().BeEquivalentTo(
                new Location("-17.07007", "145.37851", null, string.Empty));

            wamdData.DevParams.Should().NotBeNull("<<Dev Params detected but EMU does not support parsing it>>");
            wamdData.DevRunstate.Should().NotBeNull("<<Dev Runstate detected but EMU does not support parsing it>>");
        }
    }
}
