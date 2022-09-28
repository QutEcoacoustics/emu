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
    using Xunit.Abstractions;
    using static Emu.Audio.Vendors.FrontierLabs;
    using static Emu.Fixes.FrontierLabs.MetadataDurationBug;
    using static Emu.Utilities.DryRun;

    public class MetadataDurationBugTests : TestBase, IClassFixture<FixtureHelper.FixtureData>
    {
        private const decimal FirmwareVersion = 3.2m;
        private const ulong BeforeFixSamples = 317292544ul;
        private const ulong AfterFixSamples = 158646272ul;

        private const decimal FirmwareVersion2 = 3.3m;
        private const ulong AfterFixSamples2 = 158535680ul;
        private const ulong BeforeFixSamples2 = 317071360ul;

        private const decimal FirmwareVersion3 = 3.2m;
        private const ulong BeforeFixSamples3 = 317292544ul;
        private const ulong AfterFixSamples3 = 158646272ul;

        private const decimal FirmwareVersion4 = 3.2m;
        private const ulong BeforeFixSamples4 = 317292544ul;
        private const ulong AfterFixSamples4 = 158646272ul;

        private const string PatchedTag = "EMU+FL010";

        private readonly FileUtilities fileUtilities;
        private readonly MetadataDurationBug fixer;
        private readonly FixtureHelper.FixtureData data;
        private readonly FileSystem fileSystem;

        public MetadataDurationBugTests(ITestOutputHelper output, FixtureHelper.FixtureData data)
            : base(output, true)
        {
            this.fileUtilities = this.ServiceProvider.GetRequiredService<FileUtilities>();
            this.fixer = new MetadataDurationBug(Helpers.NullLogger<MetadataDurationBug>(), this.CurrentFileSystem);

            this.data = data;
            this.fileSystem = new FileSystem();
        }

        [Fact]
        public async Task CanDetectFaultyDurations()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug];
            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var actual = await this.fixer.CheckAffectedAsync(target.Path);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("File's duration is wrong", actual.Message);

            var record = Assert.IsType<MetadaDurationBugData>(actual.Data);

            Assert.Equal(new Range(207, 267), record.Firmware.FoundAt);

            await this.AssertMetadata(BeforeFixSamples, FirmwareVersion, target.Path);
        }

        [Fact]
        public async Task CanDetectFaultyDurations2()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug2];
            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var actual = await this.fixer.CheckAffectedAsync(target.Path);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("File's duration is wrong", actual.Message);

            var record = Assert.IsType<MetadaDurationBugData>(actual.Data);

            Assert.Equal(new Range(207, 267), record.Firmware.FoundAt);

            await this.AssertMetadata(BeforeFixSamples2, FirmwareVersion2, target.Path);
        }

        [Fact]
        public async Task CanDetectFaultyDurations3()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug3];
            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var actual = await this.fixer.CheckAffectedAsync(target.Path);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("File's duration is wrong", actual.Message);

            var record = Assert.IsType<MetadaDurationBugData>(actual.Data);

            Assert.Equal(new Range(207, 267), record.Firmware.FoundAt);

            await this.AssertMetadata(BeforeFixSamples3, FirmwareVersion3, target.Path);
        }

        [Fact]
        public async Task CanDetectFaultyDurations4()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug4];
            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var actual = await this.fixer.CheckAffectedAsync(target.Path);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("File's duration is wrong", actual.Message);

            var record = Assert.IsType<MetadaDurationBugData>(actual.Data);

            Assert.Equal(new Range(207, 267), record.Firmware.FoundAt);

            await this.AssertMetadata(BeforeFixSamples4, FirmwareVersion3, target.Path);
        }

        [Fact]
        public async Task WillNotTriggerForFirmwaresNotAffected()
        {
            var target = this.data[FixtureModel.NormalFile];
            var actual = await this.fixer.CheckAffectedAsync(target.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Unaffected, actual.Status);
            Assert.Contains("File not affected", actual.Message);

            var record = Assert.IsType<MetadaDurationBugData>(actual.Data);

            Assert.Equal(new Range(207, 267), record.Firmware.FoundAt);

            await this.AssertMetadata(158644224, 3.14m, target.AbsoluteFixturePath);
        }

        [Fact]
        public async Task WillNotTriggerForRatiosNotEqualTo2()
        {
            // there are other bugs where the sample count is recorded incorrectly...
            // we are not fixing those here

            var target = this.data[FixtureModel.SpaceInDateStamp];
            var actual = await this.fixer.CheckAffectedAsync(target.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Unaffected, actual.Status);
            Assert.Contains("File not affected", actual.Message);

            var record = Assert.IsType<MetadaDurationBugData>(actual.Data);

            Assert.Equal(new Range(207, 267), record.Firmware.FoundAt);

            await this.AssertMetadata(396288, 3.08m, target.AbsoluteFixturePath);
        }

        [Fact]
        public async Task WillNotTriggerForNonFLFiles()
        {
            var path = FixtureHelper.ResolvePath("Generic/Audacity/hello.flac");
            var actual = await this.fixer.CheckAffectedAsync(path);

            Assert.Equal(CheckStatus.Unaffected, actual.Status);
            Assert.Contains("Frontier Labs firmware comment string not found", actual.Message);

            Assert.Null(actual.Data);
        }

        [Fact]
        public async Task CanRepairFaultyDurations()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug];
            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var dryRun = this.DryRunFactory(false);

            await this.AssertMetadata(BeforeFixSamples, FirmwareVersion, target.Path);

            var actual = await this.fixer.ProcessFileAsync(target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Contains($"Old total samples was", actual.Message);

            await this.AssertMetadata(AfterFixSamples, FirmwareVersion, target.Path, PatchedTag);
        }

        [Fact]
        public async Task CanRepairFaultyDurations2()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug2];
            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var dryRun = this.DryRunFactory(false);

            await this.AssertMetadata(BeforeFixSamples2, FirmwareVersion2, target.Path);

            var actual = await this.fixer.ProcessFileAsync(target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Contains($"Old total samples was", actual.Message);

            await this.AssertMetadata(AfterFixSamples2, FirmwareVersion2, target.Path, PatchedTag);
        }

        [Fact]
        public async Task CanRepairFaultyDuration3()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug3];
            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var dryRun = this.DryRunFactory(false);

            await this.AssertMetadata(BeforeFixSamples3, FirmwareVersion3, target.Path);

            var actual = await this.fixer.ProcessFileAsync(target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Contains($"Old total samples was", actual.Message);

            await this.AssertMetadata(AfterFixSamples3, FirmwareVersion3, target.Path, PatchedTag);
        }

        [Fact]
        public async Task CanRepairFaultyDuration4()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug4];
            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var dryRun = this.DryRunFactory(false);

            await this.AssertMetadata(BeforeFixSamples4, FirmwareVersion4, target.Path);

            var actual = await this.fixer.ProcessFileAsync(target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Contains($"Old total samples was", actual.Message);

            await this.AssertMetadata(AfterFixSamples4, FirmwareVersion4, target.Path, PatchedTag);
        }

        [Fact]
        public async Task WillDoNothingInADryRun()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug];
            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var dryRun = this.DryRunFactory(true);

            var before = await this.fileUtilities.CalculateChecksumSha256(target.Path);
            await this.AssertMetadata(BeforeFixSamples, FirmwareVersion, target.Path);

            var actual = await this.fixer.ProcessFileAsync(target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Contains($"Old total samples was", actual.Message);

            await this.AssertMetadata(BeforeFixSamples, FirmwareVersion, target.Path);
            var after = await this.fileUtilities.CalculateChecksumSha256(target.Path);

            Assert.Equal(before, after);
        }

        [Fact]
        public async Task IsIdempotant()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug];
            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);
            var dryRun = this.DryRunFactory(false);

            await this.AssertMetadata(BeforeFixSamples, FirmwareVersion, target.Path);

            var actual = await this.fixer.ProcessFileAsync(target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Contains($"Old total samples was", actual.Message);

            await this.AssertMetadata(AfterFixSamples, FirmwareVersion, target.Path, PatchedTag);
            var first = await this.fileUtilities.CalculateChecksumSha256(target.Path);

            // now again!
            var secondActual = await this.fixer.ProcessFileAsync(target.Path, dryRun);

            Assert.Equal(FixStatus.NoOperation, secondActual.Status);
            Assert.Contains($"File has already had it's duration repaired", secondActual.Message);
            Assert.Equal(CheckStatus.Repaired, secondActual.CheckResult.Status);
            var second = await this.fileUtilities.CalculateChecksumSha256(target.Path);

            Assert.Equal(first, second);

            await this.AssertMetadata(AfterFixSamples, FirmwareVersion, target.Path, PatchedTag);
        }

        private async Task AssertMetadata(ulong samples, decimal firmwareVersion, string path, params string[] tags)
        {
            using var stream = (FileStream)this.fileSystem.File.OpenRead(path);
            var actualSamples = (ulong)Flac.ReadTotalSamples(stream);
            var actualFirmware = (FirmwareRecord)await ReadFirmwareAsync(stream);
            Assert.Equal(samples, actualSamples);
            Assert.Equal(firmwareVersion, actualFirmware.Version);

            actualFirmware.Tags.Should().BeEquivalentTo(tags);
        }
    }
}
