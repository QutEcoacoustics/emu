// <copyright file="WamdTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Audio
{
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using MetadataUtility.Audio;
    using MetadataUtility.Tests.TestHelpers;
    using NodaTime;
    using NodaTime.Text;
    using Xunit;
    using Xunit.Abstractions;

    public class WamdTests : TestBase, IClassFixture<FixtureHelper.FixtureData>
    {
        public WamdTests(ITestOutputHelper output, FixtureHelper.FixtureData data)
            : base(output)
        {
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void HasVersion1WamdChunkTest(FixtureModel model)
        {
            bool hasWamd = Wamd.HasVersion1WamdChunk(model.ToTargetInformation(this.RealFileSystem).FileStream).IfFail(false);

            hasWamd.Should().Be(model.CanProcess.Contains("WamdExtractor"));
        }

        [Fact]
        public void ExtractMetadataTest()
        {
            Stream stream = this.RealFileSystem.File.Open(Helpers.FixturesRoot + "/WA_SM4BAT/2.2.1_Normal/S4U09523_20210621_205706.wav", FileMode.Open, FileAccess.Read, FileShare.Read);

            var tryWamdData = Wamd.ExtractMetadata(stream);

            Assert.True(tryWamdData.IsSucc);

            Wamd wamdData = (Wamd)tryWamdData;

            ((OffsetDateTime)wamdData.StartDate).Should().Be(OffsetDateTimePattern.CreateWithInvariantCulture("G").Parse("2021-06-21T20:57:06-03:00").Value);
            wamdData.Name.Should().Be("SM4BAT-FS");
            wamdData.SerialNumber.Should().Be("S4U09523");
            wamdData.Firmware.Should().Be("2.2.1");
            wamdData.Temperature.Should().Be("24.25C");
            wamdData.MicrophoneType.Should().AllBe("U2");
            wamdData.MicrophoneSensitivity.Should().AllBe("13.0dBFS");
            wamdData.Latitude.Should().Be(45.7835);
            wamdData.Longitude.Should().Be(-64.23352);
        }
    }
}
