// <copyright file="FrontierLabsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Audio.Vendors
{
    using System;
    using FluentAssertions;
    using MetadataUtility.Audio.Vendors;
    using MetadataUtility.Tests.TestHelpers;
    using Xunit;
    using Xunit.Abstractions;
    using static MetadataUtility.Audio.Vendors.FrontierLabs;

    public class FrontierLabsTests : TestBase
    {
        public FrontierLabsTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory]
        [InlineData("SensorFirmwareVersion=3.17", 3.17, null)]
        [InlineData("SensorFirmwareVersion=3.2", 3.2, null)]
        [InlineData("SensorFirmwareVersion= 3.20                                ", 3.2, null)]
        [InlineData("SensorFirmwareVersion= 3.20 atag emu wookies+rule          ", 3.2, new string[] { "atag", "emu", "wookies+rule" })]
        [InlineData("SensorFirmwareVersion=3.20", 3.2, null)]
        [InlineData("SensorFirmwareVersion= V3.12", 3.12, null)]
        [InlineData("SensorFirmwareVersion= V3.13", 3.13, null)]
        [InlineData("SensorFirmwareVersion= V3.14", 3.14, null)]
        [InlineData("SensorFirmwareVersion=Firmware: V3.08       ", 3.08, null)]
        [InlineData("SensorFirmwareVersion=Firmware: V3.08   tag    ", 3.08, new string[] { "tag" })]

        public void CanParseFirmware(string firmwareComment, decimal expected, string[] tags)
        {
            tags ??= Array.Empty<string>();

            var actual = ParseFirmwareComment(firmwareComment, 0..60);

            Assert.False(actual.IsFail);

            var firmware = (FirmwareRecord)actual;

            Assert.Equal(expected, firmware.Version);
            firmware.Tags.Should().BeEquivalentTo(tags);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void HasFrontierLabsVorbisComment(FixtureModel model)
        {
            bool hasComment = FrontierLabs.HasFrontierLabsVorbisComment(model.ToTargetInformation(this.RealFileSystem).FileStream).IfFail(false);

            hasComment.Should().Be(model.Process.ContainsKey(FixtureModel.FlacCommentExtractor));
        }
    }
}
