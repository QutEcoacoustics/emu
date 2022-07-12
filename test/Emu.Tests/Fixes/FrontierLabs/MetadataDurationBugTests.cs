// <copyright file="MetadataDurationBugTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Fixes.FrontierLabs
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Fixes;
    using Emu.Fixes.FrontierLabs;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Xunit;
    using static Emu.Audio.Vendors.FrontierLabs;

    public class MetadataDurationBugTests : IClassFixture<FixtureHelper.FixtureData>, IDisposable
    {
        private const ulong BeforeFixSamples = 317292544ul;
        private const decimal FirmwareVersion = 3.2m;
        private const ulong AfterFixSamples = 158646272ul;
        private const string PatchedTag = "EMU+FL010";
        private readonly FixtureModel fixture;
        private readonly TempFile target;
        private readonly FileUtilities fileUtilities;
        private readonly MetadataDurationBug fixer;
        private readonly ILogger<DryRun> dryRunLogger;
        private readonly FixtureHelper.FixtureData data;
        private readonly FileSystem fileSystem;

        public MetadataDurationBugTests(FixtureHelper.FixtureData data)
        {
            this.fixture = data[FixtureModel.MetadataDurationBug];
            this.target = TempFile.DuplicateExisting(this.fixture.AbsoluteFixturePath);

            this.fileUtilities = new FileUtilities(Helpers.NullLogger<FileUtilities>(), new FileSystem());
            this.fixer = new MetadataDurationBug(Helpers.NullLogger<MetadataDurationBug>(), this.fileUtilities);

            this.dryRunLogger = Helpers.NullLogger<DryRun>();
            this.data = data;
            this.fileSystem = new FileSystem();
        }

        void IDisposable.Dispose()
        {
            this.target.Dispose();
        }

        [Fact]
        public async Task CanDetectFaultyDurations()
        {
            var actual = await this.fixer.CheckAffectedAsync(this.target.Path);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("File's duration is wrong", actual.Message);

            var record = Assert.IsType<FirmwareRecord>(actual.Data);

            Assert.Equal(new Range(207, 267), record.FoundAt);

            await this.AssertMetadata(BeforeFixSamples, FirmwareVersion);
        }

        [Fact]
        public async Task WillNotTriggerForFirmwaresNotAffected()
        {
            var actual = await this.fixer.CheckAffectedAsync(this.data[FixtureModel.NormalFile].AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Unaffected, actual.Status);
            Assert.Contains("File not affected", actual.Message);

            var record = Assert.IsType<FirmwareRecord>(actual.Data);

            Assert.Equal(new Range(207, 267), record.FoundAt);

            await this.AssertMetadata(BeforeFixSamples, FirmwareVersion);
        }

        [Fact]
        public async Task CanRepairFaultyDurations()
        {
            var dryRun = new DryRun(false, this.dryRunLogger);

            await this.AssertMetadata(BeforeFixSamples, FirmwareVersion);

            var actual = await this.fixer.ProcessFileAsync(this.target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Contains($"Old total samples was", actual.Message);

            await this.AssertMetadata(AfterFixSamples, FirmwareVersion, PatchedTag);
        }

        [Fact]
        public async Task WillDoNothingInADryRun()
        {
            var dryRun = new DryRun(true, this.dryRunLogger);

            var before = await this.fileUtilities.CalculateChecksumSha256(this.target.Path);
            await this.AssertMetadata(BeforeFixSamples, FirmwareVersion);

            var actual = await this.fixer.ProcessFileAsync(this.target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Contains($"Old total samples was", actual.Message);

            await this.AssertMetadata(BeforeFixSamples, FirmwareVersion);
            var after = await this.fileUtilities.CalculateChecksumSha256(this.target.Path);

            Assert.Equal(before, after);
        }

        [Fact]
        public async Task IsIdempotant()
        {
            var dryRun = new DryRun(false, this.dryRunLogger);

            await this.AssertMetadata(BeforeFixSamples, FirmwareVersion);

            var actual = await this.fixer.ProcessFileAsync(this.target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Contains($"Old total samples was", actual.Message);

            await this.AssertMetadata(AfterFixSamples, FirmwareVersion, PatchedTag);
            var first = await this.fileUtilities.CalculateChecksumSha256(this.target.Path);

            // now again!
            var secondActual = await this.fixer.ProcessFileAsync(this.target.Path, dryRun);

            Assert.Equal(FixStatus.NoOperation, secondActual.Status);
            Assert.Contains($"File has already had it's duration repaired", secondActual.Message);
            Assert.Equal(CheckStatus.Repaired, secondActual.CheckResult.Status);
            var second = await this.fileUtilities.CalculateChecksumSha256(this.target.Path);

            Assert.Equal(first, second);

            await this.AssertMetadata(AfterFixSamples, FirmwareVersion, PatchedTag);
        }

        private async Task AssertMetadata(ulong samples, decimal firmwareVersion, params string[] tags)
        {
            using var stream = (FileStream)this.fileSystem.File.OpenRead(this.target.Path);
            var actualSamples = (ulong)Flac.ReadTotalSamples(stream);
            var actualFirmware = (FirmwareRecord)await ReadFirmwareAsync(stream);
            Assert.Equal(samples, actualSamples);
            Assert.Equal(firmwareVersion, actualFirmware.Version);

            actualFirmware.Tags.Should().BeEquivalentTo(tags);
        }
    }
}
