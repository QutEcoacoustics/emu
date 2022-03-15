// <copyright file="WaveTests.cs" company="QutEcoacoustics">
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

    public class WaveTests : TestBase
    {
        public WaveTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadTotalSamplesTest(FixtureModel model)
        {
            if (model.Process.Contains("WaveHeaderExtractor"))
            {
                Fin<ulong> totalSamples = Wave.ReadTotalSamples(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(totalSamples.IsSucc);
                ((ulong)totalSamples).Should().Be(model.TotalSamples);
            }
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadSampleRateTest(FixtureModel model)
        {
            if (model.Process.Contains("WaveHeaderExtractor"))
            {
                Fin<uint> sampleRate = Wave.ReadWaveSampleRate(model.FmtBytes); //Changed from model.ToTargetInformation(this.RealFileSystem).FileStream
                Assert.True(sampleRate.IsSucc);
                ((uint)sampleRate).Should().Be(model.SampleRateHertz);
            }
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadNumChannelsTest(FixtureModel model)
        {
            if (model.Process.Contains("WaveHeaderExtractor"))
            {
                Fin<byte> channels = Wave.ReadWaveChannels(model.FmtBytes); //Changed from model.ToTargetInformation(this.RealFileSystem).FileStream
                Assert.True(channels.IsSucc);
                ((byte)channels).Should().Be(model.Channels);
            }
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void IsWaveFileTest(FixtureModel model)
        {
            Fin<bool> isWave = Wave.IsWaveFilePCM(model.ToTargetInformation(this.RealFileSystem).FileStream);
            Assert.True(isWave.IsSucc);

            ((bool)isWave).Should().Be(model.IsWave);
        }
    }
}
