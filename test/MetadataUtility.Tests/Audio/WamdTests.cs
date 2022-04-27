// <copyright file="WamdTests.cs" company="QutEcoacoustics">
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

    public class WamdTests : TestBase, IClassFixture<FixtureHelper.FixtureData>
    {
        private readonly FixtureHelper.FixtureData data;

        public WamdTests(ITestOutputHelper output, FixtureHelper.FixtureData data)
            : base(output)
        {
            this.data = data;
        }

        [Fact]

        public void GetVersionTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            var (format, dataRange) = this.ReadChunkRanges(model);

            var channels = Wave.GetChannels(format);
            channels.Should().Be(model.Channels);
        }

        [Fact]

        public void IsWildlifeAcousticsWaveFileTest()
        {
            var model = this.data[FixtureModel.SM4BatNormal1];
            using var stream = model.ToTargetInformation(this.RealFileSystem).FileStream;

            Fin<bool> isWamd = Wamd.HasVersion1WamdChunk(stream);

            Assert.True(isWamd.IsSucc);

            bool wildlifeAcoustics;

            if (model.Vendor == "Wildlife Acoustics")
            {
                wildlifeAcoustics = true;
            }
            else
            {
                wildlifeAcoustics = false;
            }

            ((bool)isWamd).Should().Be(wildlifeAcoustics);
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
