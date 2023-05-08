// <copyright file="FrontierLabsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Vendors.FrontierLabs
{
    using System;
    using Emu.Audio.Vendors;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using LanguageExt;
    using Xunit;
    using Xunit.Abstractions;
    using static Emu.Audio.Vendors.FrontierLabs;

    public class FrontierLabsTests : TestBase
    {
        public FrontierLabsTests(ITestOutputHelper output)
            : base(output, realFileSystem: true)
        {
        }

        [Theory]
        [InlineData("SensorFirmwareVersion=3.17", 3.17, null)]
        [InlineData("SensorFirmwareVersion=3.2", 3.2, null)]
        [InlineData("SensorFirmwareVersion= 3.20                                ", 3.2, null)]
        [InlineData("SensorFirmwareVersion= 3.20 atag emu wookies+rule          ", 3.2, "atag,emu,wookies+rule")]
        [InlineData("SensorFirmwareVersion=3.20", 3.2, null)]
        [InlineData("SensorFirmwareVersion= V3.12", 3.12, null)]
        [InlineData("SensorFirmwareVersion= V3.13", 3.13, null)]
        [InlineData("SensorFirmwareVersion= V3.14", 3.14, null)]
        [InlineData("SensorFirmwareVersion=Firmware: V3.08       ", 3.08, null)]
        [InlineData("SensorFirmwareVersion=Firmware: V3.08   tag    ", 3.08, "tag")]

        public void CanParseFirmware(string firmwareComment, decimal expected, string tagString)
        {
            var tags = tagString?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToSeq() ?? Seq<string>.Empty;

            var actual = ParseFirmwareComment(firmwareComment, 0..60);

            Assert.False(actual.IsFail);

            var firmware = (FirmwareRecord)actual;

            Assert.Equal(expected, firmware.Version);
            firmware.Tags.Should().BeEquivalentTo(tags);
        }

        [Theory]
        [ClassData(typeof(FixtureData))]
        public void HasFrontierLabsVorbisComment(FixtureModel model)
        {
            var hasComment = FrontierLabs.HasFrontierLabsVorbisComment(this.CreateTargetInformation(model).FileStream).IfFail(false);

            hasComment.Should().Be(model.Process.ContainsKey(FixtureModel.FlacCommentExtractor));
        }
    }
}
