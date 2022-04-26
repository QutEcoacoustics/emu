// <copyright file="WaveTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Audio
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using LanguageExt;
    using MetadataUtility.Audio;
    using MetadataUtility.Tests.TestHelpers;
    using Xunit;
    using Xunit.Abstractions;

    public class WaveTests : TestBase, IClassFixture<FixtureHelper.FixtureData>
    {
        private readonly FixtureHelper.FixtureData data;

        public WaveTests(ITestOutputHelper output, FixtureHelper.FixtureData data)
            : base(output)
        {
            this.data = data;
        }

        [Fact]

        public void ReadTotalSamplesTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            var (format, dataRange) = this.ReadChunkRanges(model);
            var bitsPerSample = Wave.GetBitsPerSample(format);
            var channels = Wave.GetChannels(format);

            var totalSamples = (ulong)Wave.GetTotalSamples(dataRange, channels, bitsPerSample);
            totalSamples.Should().Be(model.TotalSamples);
        }

        [Fact]

        public void ReadSampleRateTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            var (format, dataRange) = this.ReadChunkRanges(model);

            var sampleRate = Wave.GetSampleRate(format);
            sampleRate.Should().Be(model.SampleRateHertz);
        }

        [Fact]

        public void GetAudioFormatTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            var (format, dataRange) = this.ReadChunkRanges(model);

            var audioFormat = Wave.GetAudioFormat(format);
            audioFormat.Should().Be(Wave.Format.Pcm);
        }

        [Fact]

        public void BitsPerSampleTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            var (format, dataRange) = this.ReadChunkRanges(model);

            var bitsPerSample = Wave.GetBitsPerSample(format);
            bitsPerSample.Should().Be(model.BitDepth);
        }

        [Fact]

        public void ReadNumChannelsTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            var (format, dataRange) = this.ReadChunkRanges(model);

            var channels = Wave.GetChannels(format);
            channels.Should().Be(model.Channels);
        }

        [Fact]

        public void GetByteRateTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            var (format, dataRange) = this.ReadChunkRanges(model);

            var byteRate = Wave.GetByteRate(format) * 8;
            byteRate.Should().Be(model.BitsPerSecond);
        }

        [Fact]

        public void GetBlockAlignTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            var (format, dataRange) = this.ReadChunkRanges(model);

            var blockAlign = Wave.GetBlockAlign(format);
            blockAlign.Should().Be(model.BlockAlign);
        }

        [Fact]

        public void IsWaveFileTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            using var stream = model.ToTargetInformation(this.RealFileSystem).FileStream;

            Fin<bool> isWave = Wave.IsWaveFile(stream);

            Assert.True(isWave.IsSucc);

            ((bool)isWave).Should().Be(model.IsWave);
        }

        [Fact]

        public void IsPcmWaveFileTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            using var stream = model.ToTargetInformation(this.RealFileSystem).FileStream;

            Fin<bool> isWave = Wave.IsPcmWaveFile(stream);

            Assert.True(isWave.IsSucc);

            ((bool)isWave).Should().Be(model.IsWave);
        }

        private (byte[] FormatChunk, RangeHelper.Range DataChunk) ReadChunkRanges(FixtureModel model)
        {
            using var stream = model.ToTargetInformation(this.RealFileSystem).FileStream;

            var riffChunk = Wave.FindRiffChunk(stream);
            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var formatChunk = waveChunk.Bind(w => Wave.FindFormatChunk(stream, w));
            var dataChunk = waveChunk.Bind(w => Wave.FindDataChunk(stream, w));

            var formatSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)formatChunk);

            return (formatSpan.ToArray(), (RangeHelper.Range)dataChunk);
        }
    }
}
