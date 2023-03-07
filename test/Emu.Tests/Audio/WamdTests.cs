// <copyright file="WamdTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio
{
    using System.IO;
    using Emu.Audio;
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
            : base(output)
        {
            this.data = data;
        }

        [Fact]
        public void HasVersion1WamdChunkTest()
        {
            var fixture = this.data[FixtureModel.Sm4BatNormal1];
            var wamdFile = this.RealFileSystem.File.OpenRead(fixture.AbsoluteFixturePath);
            bool hasWamd = Wamd.HasVersion1WamdChunk(wamdFile).IfFail(false);
            Assert.True(hasWamd);
        }

        [Fact]
        public void DoesNotHaveVersion1WamdChunkTest()
        {
            var fixture = this.data[FixtureModel.NormalFile];
            var noWamdFile = this.RealFileSystem.File.OpenRead(fixture.AbsoluteFixturePath);
            var hasWamd = Wamd.HasVersion1WamdChunk(noWamdFile).IfFail(false);
            Assert.False(hasWamd);
        }

        [Fact]
        public void ExtractMetadataTest()
        {
            var fixture = this.data[FixtureModel.Sm4BatNormal1];
            var stream = this.RealFileSystem.File.OpenRead(fixture.AbsoluteFixturePath);

            var tryWamdData = Wamd.ExtractMetadata(stream);

            Assert.True(tryWamdData.IsSucc);

            Wamd wamdData = (Wamd)tryWamdData;

            ((OffsetDateTime)wamdData.StartDate).Should().Be(OffsetDateTimePattern.CreateWithInvariantCulture("G").Parse("2021-06-21T20:57:06-03:00").Value);
            wamdData.Name.Should().Be("SM4BAT-FS");
            wamdData.SerialNumber.Should().Be("S4U09523");
            wamdData.Firmware.Should().Be("2.2.1");
            wamdData.Temperature.Should().Be(24.25);
            wamdData.MicrophoneType.Should().AllBe("U2");
            wamdData.MicrophoneSensitivity.Should().AllBeEquivalentTo(13.0);
            wamdData.Location.Latitude.Should().Be(45.7835);
            wamdData.Location.Longitude.Should().Be(64.23352);
        }
    }
}
