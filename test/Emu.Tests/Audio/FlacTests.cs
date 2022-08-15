// <copyright file="FlacTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio
{
    using System;
    using Emu.Audio;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using LanguageExt;
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
                Fin<ulong> totalSamples = Flac.ReadTotalSamples(model.ToTargetInformation(this.RealFileSystem).FileStream);
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
                Fin<uint> sampleRate = Flac.ReadSampleRate(model.ToTargetInformation(this.RealFileSystem).FileStream);
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
                Fin<byte> channels = Flac.ReadNumChannels(model.ToTargetInformation(this.RealFileSystem).FileStream);
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
                Fin<byte> bitDepth = Flac.ReadBitDepth(model.ToTargetInformation(this.RealFileSystem).FileStream);
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
            Fin<byte[]> md5 = Flac.ReadMD5(stream);
            Assert.True(md5.IsSucc);

            var actualMd5 = ((byte[])md5).ToHexString();
            actualMd5.Should().Be(expectedMd5);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void IsFlacFileTest(FixtureModel model)
        {
            bool isFlac = Flac.IsFlacFile(model.ToTargetInformation(this.RealFileSystem).FileStream).IfFail(false);

            isFlac.Should().Be(model.IsFlac);
        }
    }
}
