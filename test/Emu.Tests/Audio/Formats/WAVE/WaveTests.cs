// <copyright file="WaveTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Formats.WAVE
{
    using System.Collections.Generic;
    using Emu.Audio;
    using Emu.Audio.WAVE;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using LanguageExt;
    using Xunit;
    using Xunit.Abstractions;

    public class WaveTests : TestBase, IClassFixture<FixtureData>
    {
        private readonly FixtureData data;

        public WaveTests(ITestOutputHelper output, FixtureData data)
            : base(output)
        {
            this.data = data;
        }

        [Fact]

        public void ReadTotalSamplesTest()
        {
            var model = this.data[FixtureModel.Sm4BatNormal1];
            var (format, dataRange) = this.ReadChunkRanges(model);
            var bitsPerSample = Wave.GetBitsPerSample(format);
            var channels = Wave.GetChannels(format);

            var totalSamples = Wave.GetTotalSamples(dataRange, channels, bitsPerSample);
            totalSamples.Should().Be(model.Record.TotalSamples);
        }

        [Fact]

        public void ReadSampleRateTest()
        {
            var model = this.data[FixtureModel.Sm4BatNormal1];
            var (format, _) = this.ReadChunkRanges(model);

            var sampleRate = Wave.GetSampleRate(format);
            sampleRate.Should().Be(model.Record.SampleRateHertz);
        }

        [Fact]

        public void GetAudioFormatTest()
        {
            var model = this.data[FixtureModel.Sm4BatNormal1];
            var (format, _) = this.ReadChunkRanges(model);

            var audioFormat = Wave.GetAudioFormat(format);
            audioFormat.Should().Be(Wave.Format.Pcm);
        }

        [Fact]

        public void BitsPerSampleTest()
        {
            var model = this.data[FixtureModel.Sm4BatNormal1];
            var (format, _) = this.ReadChunkRanges(model);

            var bitsPerSample = Wave.GetBitsPerSample(format);
            bitsPerSample.Should().Be(model.Record.BitDepth);
        }

        [Fact]

        public void ReadNumChannelsTest()
        {
            var model = this.data[FixtureModel.Sm4BatNormal1];
            var (format, _) = this.ReadChunkRanges(model);

            var channels = Wave.GetChannels(format);
            channels.Should().Be(model.Record.Channels);
        }

        [Fact]

        public void GetByteRateTest()
        {
            var model = this.data[FixtureModel.Sm4BatNormal1];
            var (format, _) = this.ReadChunkRanges(model);

            var byteRate = Wave.GetByteRate(format) * 8;
            byteRate.Should().Be(model.Record.BitsPerSecond);
        }

        [Fact]

        public void GetBlockAlignTest()
        {
            var model = this.data[FixtureModel.Sm4BatNormal1];
            var (format, _) = this.ReadChunkRanges(model);

            var blockAlign = Wave.GetBlockAlign(format);
            blockAlign.Should().Be(2);
        }

        [Fact]

        public void IsWaveFileTest()
        {
            var model = this.data[FixtureModel.Sm4BatNormal1];
            using var stream = model.ToTargetInformation(this.RealFileSystem).FileStream;

            var isWave = Wave.IsWaveFile(stream);

            Assert.True(isWave.IsSucc);

            ((bool)isWave).Should().Be(model.IsWave);
        }

        [Fact]

        public void IsPcmWaveFileTest()
        {
            var model = this.data[FixtureModel.Sm4BatNormal1];
            using var stream = model.ToTargetInformation(this.RealFileSystem).FileStream;

            var isWave = Wave.IsPcmWaveFile(stream);

            Assert.True(isWave.IsSucc);

            ((bool)isWave).Should().Be(model.IsWave);
        }

        [Fact]
        public void CanReadCuesTest()
        {
            var model = this.data[FixtureModel.GenericWaveWithCueChunk];
            using var stream = model.ToTargetInformation(this.RealFileSystem).FileStream;

            var waveChunk = Wave.FindRiffChunk(stream).Bind(r => Wave.FindWaveChunk(stream, r));
            var result = Wave.FindAndParseCuePoints(stream, (RangeHelper.Range)waveChunk);

            Assert.True(result.IsSucc);
            result.IfFail(null).Should().BeEquivalentTo(new Cue[]
            {
                new Cue(925632, null, null, null),
                new Cue(1241088, null, null, null),
                new Cue(1609728, null, null, null),
                new Cue(1941504, null, null, null),
            });
        }

        [Fact]
        public void CanReadCuesWithLabelsTest()
        {
            var model = this.data[FixtureModel.GenericWaveWithCueAndLabelChunks];
            using var stream = model.ToTargetInformation(this.RealFileSystem).FileStream;

            var waveChunk = Wave.FindRiffChunk(stream).Bind(r => Wave.FindWaveChunk(stream, r));
            var result = Wave.FindAndParseCuePoints(stream, (RangeHelper.Range)waveChunk);

            Assert.True(result.IsSucc);
            result.IfFail(null).Should().BeEquivalentTo(new Cue[]
            {
                new Cue(2178432, "MARK_01", null, null),
                new Cue(3095424, "MARK_02", null, null),
                new Cue(3146112, "MARK_03", null, null),
                new Cue(3795840, "MARK_04", null, null),
                new Cue(4569984, "MARK_05", null, null),
            });
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
