// <copyright file="FlacTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Audio
{
    using System.Linq;
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
            if (model.Process.Contains("FlacHeaderExtractor"))
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
            if (model.Process.Contains("FlacHeaderExtractor"))
            {
                Fin<uint> sampleRateResult = Flac.ReadSampleRate(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(sampleRateResult.IsSucc);
                sampleRate = (uint)sampleRateResult;
            }

            sampleRate?.Should().Be(model.SampleRateHertz);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadNumChannelsTest(FixtureModel model)
        {
            byte? channels = null;
            if (model.Process.Contains("FlacHeaderExtractor"))
            {
                Fin<byte> channelsResult = Flac.ReadNumChannels(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(channelsResult.IsSucc);
                channels = (byte)channelsResult;
            }

            channels?.Should().Be(model.Channels);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadBitDepthTest(FixtureModel model)
        {
            byte? bitDepth = null;
            if (model.Process.Contains("FlacHeaderExtractor"))
            {
                Fin<byte> bitDepthResult = Flac.ReadBitDepth(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(bitDepthResult.IsSucc);
                bitDepth = (byte)bitDepthResult;
            }

            bitDepth?.Should().Be(model.BitDepth);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void IsFlacFileTest(FixtureModel model)
        {
            Fin<bool> isFlac = Flac.IsFlacFile(model.ToTargetInformation(this.RealFileSystem).FileStream);
            Assert.True(isFlac.IsSucc);

            ((bool)isFlac).Should().Be(model.IsFlac);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void HasMetadataBlockTest(FixtureModel model)
        {
            Fin<bool> hasMetadata = Flac.HasMetadataBlock(model.ToTargetInformation(this.RealFileSystem).FileStream);
            Assert.True(hasMetadata.IsSucc);

            ((bool)hasMetadata).Should().Be(model.ValidMetadata != ValidMetadata.No);
        }
    }
}
