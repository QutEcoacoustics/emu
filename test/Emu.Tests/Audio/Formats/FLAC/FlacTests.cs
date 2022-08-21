// <copyright file="FlacTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Formats.FLAC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using LanguageExt;
    using MoreLinq;
    using Xunit;
    using Xunit.Abstractions;

    public class FlacTests : TestBase, IClassFixture<FixtureHelper.FixtureData>
    {
        private readonly FixtureHelper.FixtureData data;

        public FlacTests(FixtureHelper.FixtureData data, ITestOutputHelper output)
            : base(output)
        {
            this.data = data;
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadTotalSamplesTest(FixtureModel model)
        {
            if (model.Process.ContainsKey(FixtureModel.FlacHeaderExtractor))
            {
                var totalSamples = Flac.ReadTotalSamples(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(totalSamples.IsSucc);
                ((ulong)totalSamples).Should().Be(model.Record.TotalSamples);
            }
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadSampleRateTest(FixtureModel model)
        {
            if (model.Process.ContainsKey(FixtureModel.FlacHeaderExtractor))
            {
                var sampleRate = Flac.ReadSampleRate(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(sampleRate.IsSucc);
                ((uint)sampleRate).Should().Be(model.Record.SampleRateHertz);
            }
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadNumChannelsTest(FixtureModel model)
        {
            if (model.Process.ContainsKey(FixtureModel.FlacHeaderExtractor))
            {
                var channels = Flac.ReadNumberChannels(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(channels.IsSucc);
                ((byte)channels).Should().Be((byte)model.Record.Channels);
            }
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadBitDepthTest(FixtureModel model)
        {
            if (model.Process.ContainsKey(FixtureModel.FlacHeaderExtractor))
            {
                var bitDepth = Flac.ReadBitDepth(model.ToTargetInformation(this.RealFileSystem).FileStream);
                Assert.True(bitDepth.IsSucc);
                ((byte)bitDepth).Should().Be(model.Record.BitDepth);
            }
        }

        [Theory]
        [InlineData(FixtureModel.NormalFile, "00000000000000000000000000000000")]
        [InlineData(FixtureModel.ArtificialZeroes, "abeb2a164488788cca9455a132909f02")]

        public void ReadMD5Test(string name, string expectedMd5)
        {
            var model = this.data[name];
            using var stream = this.RealFileSystem.File.OpenRead(model.AbsoluteFixturePath);
            var md5 = Flac.ReadMD5(stream);
            Assert.True(md5.IsSucc);

            var actualMd5 = ((byte[])md5).ToHexString();
            actualMd5.Should().Be(expectedMd5);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void IsFlacFileTest(FixtureModel model)
        {
            var isFlac = Flac.IsFlacFile(model.ToTargetInformation(this.RealFileSystem).FileStream).IfFail(false);

            isFlac.Should().Be(model.IsFlac);
        }

        [Fact]
        public void CanGetFrameOffset()
        {
            var fixture = this.data[FixtureModel.NormalFile];
            using var stream = this.RealFileSystem.File.OpenRead(fixture.AbsoluteFixturePath);

            var result = Flac.FindFrameStart(stream);

            result.IsSucc.Should().BeTrue();
            result.ThrowIfFail().Should().Be(679);
        }

        [Fact]
        public async Task EnumerateFramesWorks()
        {
            // 52 frames
            var path = FixtureHelper.ResolvePath("Generic\\Audacity\\hello.flac");
            using var stream = this.RealFileSystem.File.OpenRead(path);

            Flac.IsFlacFile(stream).ThrowIfFail().Should().BeTrue();

            var sampleRate = Flac.ReadSampleRate(stream).ThrowIfFail();
            var sampleSize = Flac.ReadBitDepth(stream).ThrowIfFail();

            Flac.FindFrameStart(stream).ThrowIfFail().Should().Be(0x56);

            List<Frame> frames = new(53);
            await foreach (var frame in Flac.EnumerateFrames(stream, sampleRate, sampleSize))
            {
                frames.Add(frame.ThrowIfFail());
            }

            // extracted from source with flac -a "test\Fixtures\Generic\Audacity\hello.flac" --stdout
            var expectedFrames = new[]
                {
                    (0, 86),
                    (1, 4073),
                    (2, 7989),
                    (3, 11899),
                    (4, 15835),
                    (5, 20036),
                    (6, 24361),
                    (7, 29306),
                    (8, 35178),
                    (9, 41031),
                    (10, 47129),
                    (11, 53118),
                    (12, 58577),
                    (13, 63500),
                    (14, 68336),
                    (15, 73028),
                    (16, 77673),
                    (17, 82163),
                    (18, 86462),
                    (19, 90742),
                    (20, 95450),
                    (21, 100234),
                    (22, 104670),
                    (23, 108956),
                    (24, 113218),
                    (25, 117565),
                    (26, 121809),
                    (27, 126371),
                    (28, 130989),
                    (29, 135622),
                    (30, 140091),
                    (31, 144338),
                    (32, 148550),
                    (33, 152783),
                    (34, 157005),
                    (35, 161206),
                    (36, 165317),
                    (37, 169468),
                    (38, 173624),
                    (39, 177929),
                    (40, 182200),
                    (41, 186376),
                    (42, 190519),
                    (43, 194706),
                    (44, 198912),
                    (45, 203104),
                    (46, 207277),
                    (47, 211419),
                    (48, 215701),
                    (49, 220021),
                    (50, 224272),
                    (51, 228501),
                    (52, 232720),
                };
            foreach (var pair in frames.Select(x => (x.Index, x.Offset)).Zip(expectedFrames))
            {
                var actual = pair.First;
                var expected = pair.Second;
                actual.Should().BeEquivalentTo(expected);
            }

            frames.Count.Should().Be(53);

            var expected1 = new Frame(
                0,
                86,
                new FrameHeader(FrameBlockingStrategy.Fixed, 4096, 44100, FrameChannelAssignment.MidPlusSideStereo, 16, 0, null, 0x8d));
            frames[0].Should().BeEquivalentTo(expected1);

            var expectedSecondLast = new Frame(
                51,
                228501,
                new FrameHeader(FrameBlockingStrategy.Fixed, 4096, 44100, FrameChannelAssignment.MidPlusSideStereo, 16, 51, null, 0x14));
            frames[^2].Should().BeEquivalentTo(expectedSecondLast);

            var expectedLast = new Frame(
                52,
                232720,
                new FrameHeader(FrameBlockingStrategy.Fixed, 795, 44100, FrameChannelAssignment.MidPlusSideStereo, 16, 52, null, 0xad));
            frames[^1].Should().BeEquivalentTo(expectedLast, options => options.ComparingRecordsByMembers().WithTracing());
        }

        [Fact]
        public async Task CanCountSamples()
        {
            var fixture = this.data[FixtureModel.NormalFile];
            using var stream = this.RealFileSystem.File.OpenRead(fixture.AbsoluteFixturePath);

            var expected = Flac.ReadTotalSamples(stream);
            var actual = await Flac.CountSamplesAsync(stream);

            expected.ThrowIfFail().Should().Be(158644224UL);
            actual.ThrowIfFail().Should().Be(158644224UL);
        }

        [Fact]
        public async Task CanCountSamples2()
        {
            var path = FixtureHelper.ResolvePath("Generic\\Audacity\\hello.flac");
            using var stream = this.RealFileSystem.File.OpenRead(path);

            var expected = Flac.ReadTotalSamples(stream);
            var actual = await Flac.CountSamplesAsync(stream);

            expected.ThrowIfFail().Should().Be(213787UL);
            actual.ThrowIfFail().Should().Be(213787UL);
        }

        [Fact]
        public async Task CanCountSamples3()
        {
            var fixture = this.data[FixtureModel.SpaceInDateStamp];
            using var stream = this.RealFileSystem.File.OpenRead(fixture.AbsoluteFixturePath);


            var expected = Flac.ReadTotalSamples(stream);
            var actual = await Flac.CountSamplesAsync(stream);

            expected.ThrowIfFail().Should().Be(396288UL);
            actual.ThrowIfFail().Should().Be(404352UL);
        }

        [Fact]
        public void CanReadBlockSizes()
        {
            var path = FixtureHelper.ResolvePath("Generic\\Audacity\\hello.flac");
            using var stream = this.RealFileSystem.File.OpenRead(path);

            var actual = Flac.ReadBlockSizes(stream);
            actual.ThrowIfFail().Should().BeEquivalentTo((4096, 4096));
        }

        [Fact]
        public void CanReadFrameSizes()
        {
            var path = FixtureHelper.ResolvePath("Generic\\Audacity\\hello.flac");
            using var stream = this.RealFileSystem.File.OpenRead(path);

            var actual = Flac.ReadFrameSizes(stream);
            actual.ThrowIfFail().Should().BeEquivalentTo((829, 6098));
        }
    }
}