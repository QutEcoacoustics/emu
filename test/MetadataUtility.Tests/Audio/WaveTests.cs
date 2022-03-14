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

        public void ReadNumChannelsTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            var (format, dataRange) = this.ReadChunkRanges(model);

            var channels = Wave.GetChannels(format);
            channels.Should().Be(model.Channels);
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

        // TODO: broken code, out of sync with master branch
        // [Theory]
        // [ClassData(typeof(FixtureHelper.FixtureData))]
        // public void ReadTotalSamplesTest(FixtureModel model)
        // {
        //     if (model.Process.Contains("WaveHeaderExtractor"))
        //     {
        //         var (format, dataRange) = this.ReadChunkRanges(model);
        //         var bitsPerSample = Wave.GetBitsPerSample(format);
        //         var channels = Wave.GetChannels(format);

        //         var totalSamples = (ulong)Wave.GetTotalSamples(dataRange, channels, bitsPerSample);
        //         totalSamples.Should().Be(model.TotalSamples);
        //     }
        // }

        // [Theory]
        // [ClassData(typeof(FixtureHelper.FixtureData))]
        // public void ReadSampleRateTest(FixtureModel model)
        // {
        //     if (model.Process.Contains("WaveHeaderExtractor"))
        //     {
        //         var (format, dataRange) = this.ReadChunkRanges(model);

        //         var sampleRate = Wave.GetSampleRate(format);
        //         sampleRate.Should().Be(model.SampleRateHertz);
        //     }
        // }

        // [Theory]
        // [ClassData(typeof(FixtureHelper.FixtureData))]
        // public void ReadNumChannelsTest(FixtureModel model)
        // {
        //     if (model.Process.Contains("WaveHeaderExtractor"))
        //     {
        //         var (format, dataRange) = this.ReadChunkRanges(model);

        //         var channels = Wave.GetChannels(format);
        //         channels.Should().Be(model.Channels);
        //     }
        // }

        // [Theory]
        // [ClassData(typeof(FixtureHelper.FixtureData))]
        // public void IsWaveFileTest(FixtureModel model)
        // {
        //     using var stream = model.ToTargetInformation(this.RealFileSystem).FileStream;

        //     Fin<bool> isWave = Wave.IsWaveFile(stream);
        //     Assert.True(isWave.IsSucc);

        //     ((bool)isWave).Should().Be(model.IsWave);
        // }

        private (byte[] FormatChunk, Wave.Range DataChunk) ReadChunkRanges(FixtureModel model)
        {
            using var stream = model.ToTargetInformation(this.RealFileSystem).FileStream;

            var riffChunk = Wave.FindRiffChunk(stream);
            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var formatChunk = waveChunk.Bind(w => Wave.FindFormatChunk(stream, w));
            var dataChunk = waveChunk.Bind(w => Wave.FindDataChunk(stream, w));

            var formatSpan = Wave.ReadRange(stream, (Wave.Range)formatChunk);

            return (formatSpan.ToArray(), (Wave.Range)dataChunk);
        }
    }
}
