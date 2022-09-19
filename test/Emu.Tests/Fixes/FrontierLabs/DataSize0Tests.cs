// <copyright file="DataSize0Tests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Fixes.FrontierLabs
{
    using System;
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Audio.WAVE;
    using Emu.Fixes;
    using Emu.Fixes.FrontierLabs;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class DataSize0Tests : TestBase, IClassFixture<FixtureHelper.FixtureData>, IDisposable
    {
        private const uint BeforeDataSize = 0u;
        private const uint AfterDataSize = 158_535_636u;

        private const uint BeforeRiffSize = 36u;
        private const uint AfterRiffSize = 158_535_672u;

        // bit depth is 16, two bytes. Only one channel, therefore number of samples is length divided by bytes per sample: 2
        private const uint BeforeSamples = BeforeDataSize / 2;
        private const uint AfterSamples = AfterDataSize / 2;

        private readonly TempFile target;
        private readonly DataSize0 fixer;

        private readonly long beforeSize;

        public DataSize0Tests(ITestOutputHelper output, FixtureHelper.FixtureData data)
            : base(output, true)
        {
            var fixture = data[FixtureModel.DataSize0];
            this.target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);
            this.beforeSize = this.target.File.Length;

            this.fixer = new DataSize0(this.CurrentFileSystem);
        }

        void IDisposable.Dispose()
        {
            this.target.Dispose();
        }

        [Fact]
        public void ItsMetadataIsCorrect()
        {
            var info = this.fixer.GetOperationInfo();

            info.Fixable.Should().BeTrue();
            info.Automatic.Should().BeTrue();
            info.Safe.Should().BeFalse();

            Assert.True(this.fixer is IFixOperation);
        }

        [Fact]
        public async Task CanDetectProblem()
        {
            var actual = await this.fixer.CheckAffectedAsync(this.target.Path);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("RIFF length and data length are incorrect", actual.Message);
            Assert.Equal(Severity.Mild, actual.Severity);
            Assert.NotNull(actual.Data);
        }

        [SkippableTheory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async Task NoOtherFixtureIsDetectedAsAPositive(FixtureModel fixture)
        {
            Skip.If(fixture.Name is FixtureModel.DataSize0);

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            if (fixture.IsWave)
            {
                Assert.Equal(CheckStatus.Unaffected, actual.Status);
            }
            else
            {
                Assert.Equal(CheckStatus.NotApplicable, actual.Status);
            }

            Assert.Null(actual.Message);
            Assert.Null(actual.Data);
            Assert.Equal(Severity.None, actual.Severity);
        }

        [Fact]
        public async Task CanFixProblem()
        {
            var dryRun = this.DryRunFactory(false);

            await this.AssertMetadata(BeforeRiffSize, BeforeDataSize, BeforeSamples, false);

            var actual = await this.fixer.ProcessFileAsync(this.target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Equal(
                $"RIFF length set to {AfterRiffSize} (was {BeforeRiffSize}). data length set to {AfterDataSize} (was {BeforeDataSize})",
                actual.Message);

            await this.AssertMetadata(AfterRiffSize, AfterDataSize, AfterSamples, false);
        }

        [Fact]
        public async Task WillDoNothingInADryRun()
        {
            var dryRun = this.DryRunFactory(true);

            await this.AssertMetadata(BeforeRiffSize, BeforeDataSize, BeforeSamples, false);

            var actual = await this.fixer.ProcessFileAsync(this.target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Equal(
                $"RIFF length set to {AfterRiffSize} (was {BeforeRiffSize}). data length set to {AfterDataSize} (was {BeforeDataSize})",
                actual.Message);

            // expect as before, expect no change
            await this.AssertMetadata(BeforeRiffSize, BeforeDataSize, BeforeSamples, false);
        }

        [Fact]
        public async Task IsIdempotant()
        {
            var dryRun = this.DryRunFactory(false);

            await this.AssertMetadata(BeforeRiffSize, BeforeDataSize, BeforeSamples, false);

            var actual = await this.fixer.ProcessFileAsync(this.target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Equal(
                $"RIFF length set to {AfterRiffSize} (was {BeforeRiffSize}). data length set to {AfterDataSize} (was {BeforeDataSize})",
                actual.Message);

            await this.AssertMetadata(AfterRiffSize, AfterDataSize, AfterSamples, false);

            // now again!
            var secondActual = await this.fixer.ProcessFileAsync(this.target.Path, dryRun);

            Assert.Equal(FixStatus.NoOperation, secondActual.Status);
            Assert.Null(secondActual.Message);
            Assert.Equal(CheckStatus.Unaffected, secondActual.CheckResult.Status);

            await this.AssertMetadata(AfterRiffSize, AfterDataSize, AfterSamples, false);
        }

        private async Task AssertMetadata(uint expectedRiffSize, uint expectedDataSize, uint expectedSamples, bool expectOutOfBounds)
        {
            using var stream = this.CurrentFileSystem.File.OpenRead(this.target.Path);

            // file total size should never change
            stream.Length.Should().Be(this.beforeSize);

            var riffChunk = Wave.FindRiffChunk(stream);
            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var formatChunk = waveChunk.Bind(w => Wave.FindFormatChunk(stream, w, true));
            var dataChunk = waveChunk.Bind(w => Wave.FindDataChunk(stream, w, true));

            riffChunk.ThrowIfFail().Length.Should().Be(expectedRiffSize);
            riffChunk.ThrowIfFail().OutOfBounds.Should().Be(expectOutOfBounds);

            dataChunk.ThrowIfFail().Length.Should().Be(expectedDataSize);
            dataChunk.ThrowIfFail().OutOfBounds.Should().Be(expectOutOfBounds);

            var formatSpan = await RangeHelper.ReadRangeAsync(stream, (RangeHelper.Range)formatChunk);

            var bitsPerSample = Wave.GetBitsPerSample(formatSpan);
            var channels = Wave.GetChannels(formatSpan);

            var samples = dataChunk.Map(d => (ulong?)Wave.GetTotalSamples(d, channels, bitsPerSample));

            samples.ThrowIfFail().Should().Be(expectedSamples);
        }
    }
}
