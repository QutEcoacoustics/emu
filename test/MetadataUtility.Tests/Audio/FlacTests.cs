// <copyright file="FlacTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Audio
{
    using FluentAssertions;
    using LanguageExt;
    using MetadataUtility.Audio;
    using MetadataUtility.Tests.TestHelpers;
    using Xunit;
    using Xunit.Abstractions;

    public class FlacTests : TestBase
    {
        public FlacTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadTotalSamplesTest(FixtureModel model)
        {
            ulong? totalSamples = null;
            if (model.IsFlac)
            {
                Fin<ulong> totalSamplesResult = Flac.ReadTotalSamples(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(totalSamplesResult.IsSucc);
                totalSamples = (ulong)totalSamplesResult;
            }

            totalSamples?.Should().Be(model.TotalSamples);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadSampleRateTest(FixtureModel model)
        {
            uint? sampleRate = null;
            if (model.IsFlac)
            {
                Fin<uint> sampleRateResult = Flac.ReadSampleRate(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(sampleRateResult.IsSucc);
                sampleRate = (uint)sampleRateResult;
            }

            sampleRate?.Should().Be(model.SampleRateHertz);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadNumChannels(FixtureModel model)
        {
            byte? channels = null;
            if (model.IsFlac)
            {
                Fin<byte> channelsResult = Flac.ReadNumChannels(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(channelsResult.IsSucc);
                channels = (byte)channelsResult;
            }

            channels?.Should().Be(model.Channels);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadBitDepth(FixtureModel model)
        {
            byte? bitDepth = null;
            if (model.IsFlac)
            {
                Fin<byte> bitDepthResult = Flac.ReadBitDepth(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(bitDepthResult.IsSucc);
                bitDepth = (byte)bitDepthResult;
            }

            bitDepth?.Should().Be(model.BitDepth);
        }
    }
}
