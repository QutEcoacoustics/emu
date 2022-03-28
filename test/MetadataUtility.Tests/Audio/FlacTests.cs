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
            if (model.Process.Contains("FlacHeaderExtractor"))
            {
                Fin<ulong> totalSamples = Flac.ReadTotalSamples(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(totalSamples.IsSucc);
                ((ulong)totalSamples).Should().Be(model.TotalSamples);
            }
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadSampleRateTest(FixtureModel model)
        {
            if (model.Process.Contains("FlacHeaderExtractor"))
            {
                Fin<uint> sampleRate = Flac.ReadSampleRate(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(sampleRate.IsSucc);
                ((uint)sampleRate).Should().Be(model.SampleRateHertz);
            }
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadNumChannelsTest(FixtureModel model)
        {
            if (model.Process.Contains("FlacHeaderExtractor"))
            {
                Fin<byte> channels = Flac.ReadNumChannels(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(channels.IsSucc);
                ((byte)channels).Should().Be((byte)model.Channels);
            }
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadBitDepthTest(FixtureModel model)
        {
            if (model.Process.Contains("FlacHeaderExtractor"))
            {
                Fin<byte> bitDepth = Flac.ReadBitDepth(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(bitDepth.IsSucc);
                ((byte)bitDepth).Should().Be(model.BitDepth);
            }
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void IsFlacFileTest(FixtureModel model)
        {
            bool isFlac = Flac.IsFlacFile(model.ToTargetInformation(this.RealFileSystem).FileStream).IfFail(false);

            isFlac.Should().Be(model.IsFlac);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void HasMetadataBlockTest(FixtureModel model)
        {
            Fin<bool> hasMetadata = Flac.HasMetadataBlock(model.ToTargetInformation(this.RealFileSystem).FileStream);
            Assert.True(hasMetadata.IsSucc);

            ((bool)hasMetadata).Should().Be(model.IsFlac && model.ValidMetadata != ValidMetadata.No);
        }
    }
}
