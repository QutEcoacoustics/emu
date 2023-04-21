// <copyright file="PartialFileRepairTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Fixes.FrontierLabs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Filenames;
    using Emu.Fixes;
    using Emu.Fixes.FrontierLabs;
    using Emu.Metadata;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using LanguageExt;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Xunit;
    using Xunit.Abstractions;
    using static Emu.Audio.Vendors.FrontierLabs;
    using static Emu.Tests.TestHelpers.FixtureModel;

    public class PartialFileRepairTests : TestBase, IClassFixture<FixtureData>
    {
        private readonly FileUtilities fileUtilities;
        private readonly PartialFileRepair fixer;
        private readonly FixtureData data;

        public PartialFileRepairTests(ITestOutputHelper output, FixtureData data)
            : base(output, true)
        {
            this.fileUtilities = this.ServiceProvider.GetRequiredService<FileUtilities>();
            this.fixer = new PartialFileRepair(
                this.BuildLogger<PartialFileRepair>(),
                this.CurrentFileSystem,
                this.fileUtilities,
                this.ServiceProvider.GetRequiredService<MetadataRegister>(),
                this.ServiceProvider.GetRequiredService<FilenameGenerator>(),
                this.ServiceProvider.GetRequiredService<MetadataDurationBug>());

            this.data = data;
        }

        public static IEnumerable<object[]> TestCaseData => new object[][]
        {
            // caused due to a duplicate file name after sensor sync
            //   This file has some kind of conflict after frame 35661 (3,312.187210884354 seconds).
            //   The FLAC decoder, audacity, and ffmpeg all stop decoding in the same spot, so we do as well.
            //   35662 frames * 2048 blocksize = 73,035,776
            new TestCase(RobsonDryAPartialWithConflict, FixStatus.Fixed, "20200426T020000Z_recovered.flac", 317_292_544, 73_035_776, 217129512, 99824893, "EMU+FL010", "EMU+FL011").AsArray(),

            new TestCase(RobsonDryAEmptyPartial, FixStatus.Renamed, "data.error_empty", Flac.FileTooShort.Message, Flac.FileTooShort.Message, 0, 0).AsArray(),

            // caused due to a duplicate file name after sensor sync
            //   This file has some kind of conflict after frame 10980 (1,019.820408163265 seconds).
            //   The FLAC decoder, audacity, and ffmpeg all stop decoding in the same spot, so we do as well.
            //   10980 frames * 2048 blocksize = 22,487,040
            new TestCase(RobsonDryBPartialWithConflict, FixStatus.Fixed, "20200227T020000Z_recovered.flac", 317292544, 22_489_088, 230471988, 34044352, "EMU+FL010", "EMU+FL011").AsArray(),

            // caused due to a crash on the device that failed to cleanup the end of the file
            // part of the file is encoded as flac - the rest is (?) WAVE data that is to be discarded
            new TestCase(Test330Partial0400, FixStatus.Fixed, "20220426T040000+1000_recovered.flac", 0, 151_957_504, 317292588, 174538110, "EMU+FL011").AsArray(),

            // caused due to a crash on the device that failed to cleanup the end of the file
            // part of the file is encoded as flac - the rest is (?) WAVE data that is to be discarded
            new TestCase(Test330Partial0600, FixStatus.Fixed, "20220426T060000+1000_recovered.flac", 0, 2_281_472, 317292588, 2833099, "EMU+FL011").AsArray(),

            // caused by a full-size preallocated file which has a WAVE header
            // sensor crashed some unknown error, an excerpt from the log file:
            //  Watchdog recovered from CPU lockup!! Please report this error to Frontier Labs.
            new TestCase(PartialFile314FullPreallocated, FixStatus.Renamed, "data.error_stub", 16786006, 16786006, 317292588, 317292588, expectNoFirmware: true).AsArray(),

            // caused by a full-size preallocated file which has a WAVE header
            // sensor crashed some unknown error, an excerpt from the log file:
            //  Watchdog recovered from CPU lockup!! Please report this error to Frontier Labs.
            new TestCase(PartialFile312FullPreallocated, FixStatus.Renamed, "data.error_stub", 16786006, 16786006, 316858412, 316858412, expectNoFirmware: true).AsArray(),

            // a very short file created when the battery ran out
            // An excerpt from the log file:
            //   28/03/2021 00:00:11 Battery empty! 0% ( 5.57 V )
            new TestCase(PartialFile320EmptyBattery, FixStatus.Renamed, "data.error_partial", 8192, 8192, 3776, 3776).AsArray(),

            // This particular bug was caused because the full file scan method was used to verify FL011,
            // but the Fl010 detection started with the end-only method, and threw when it found invalid frames (WAVE data).
            // The fix here was not to throw when variable frames are found, and fallback to full file scan.
            new TestCase(Fl011Bug346, FixStatus.Fixed, "20220616T110000-0300_recovered.flac", 0, 60203008, 634_142_764, 115_080_383, "EMU+FL011").AsArray(),
        };

        [Theory]
        [MemberData(nameof(TestCaseData))]
        public async Task CanDetectPartialFiles(TestCase test)
        {
            var fixture = this.data[test.FixtureName];

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("Partial file detected", actual.Message);

            Assert.NotNull(actual.Data);

            await this.AssertMetadataBefore(test, fixture.AbsoluteFixturePath);
        }

        [SkippableTheory]
        [ClassData(typeof(FixtureData))]
        public async Task NoOtherFixtureIsDetectedAsAPositive(FixtureModel fixture)
        {
            Skip.If(fixture.IsAffectedByProblem(WellKnownProblems.FrontierLabsProblems.PartialDataFiles));

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.NotApplicable, actual.Status);

            Assert.Equal("File is not named `data`", actual.Message);
            Assert.Null(actual.Data);
            Assert.Equal(Severity.None, actual.Severity);
        }

        [Theory]
        [MemberData(nameof(TestCaseData))]
        public async Task CanRepairPartialFiles(TestCase test)
        {
            var fixture = this.data[test.FixtureName];

            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var dryRun = this.DryRunFactory(false);

            await this.AssertMetadataBefore(test, target.Path);

            var actual = await this.fixer.ProcessFileAsync(target.Path, dryRun);

            Assert.Equal(test.ExpectedStatus, actual.Status);
            if (test.ExpectedStatus is FixStatus.Fixed)
            {
                Assert.Contains(
                    $"Partial file repaired. New name is {test.NewName}. Samples count was {(ulong)test.OldSamples}, new samples count is: {(ulong)test.NewSamples}. File truncated at {test.NewSize}.",
                    actual.Message);
            }
            else
            {
                // one of the following two cases
                if (actual.Message.Contains("empty"))
                {
                    Assert.Contains("Partial file was empty", actual.Message);
                }
                else if (actual.Message.Contains("short"))
                {
                    Assert.Contains("The file is to short to repair. At least three frames are needed.", actual.Message);
                }
                else
                {
                    Assert.Contains("Partial file was a stub and has no useable data", actual.Message);
                }
            }

            this.CurrentFileSystem.File.Exists(target.Path).Should().BeFalse();
            await this.AssertMetadataAfter(test, actual.NewPath);
        }

        [Theory]
        [MemberData(nameof(TestCaseData))]
        public async Task WillDoNothingInADryRun(TestCase test)
        {
            var fixture = this.data[test.FixtureName];
            using var target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var dryRun = this.DryRunFactory(true);

            var before = await this.fileUtilities.CalculateChecksumSha256(target.Path);
            await this.AssertMetadataBefore(test, target.Path);

            var actual = await this.fixer.ProcessFileAsync(target.Path, dryRun);

            Assert.Equal(test.ExpectedStatus, actual.Status);
            if (test.ExpectedStatus is FixStatus.Fixed)
            {
                Assert.Contains(
                $"Partial file repaired. New name is {test.NewName}. Samples count was {(ulong)test.OldSamples}, new samples count is: {(ulong)test.NewSamples}. File truncated at {test.NewSize}.",
                actual.Message);
            }
            else
            {
                // one of the following two cases
                if (actual.Message.Contains("empty"))
                {
                    Assert.Contains("Partial file was empty", actual.Message);
                }
                else if (actual.Message.Contains("short"))
                {
                    Assert.Contains("The file is to short to repair. At least three frames are needed.", actual.Message);
                }
                else
                {
                    Assert.Contains("Partial file was a stub and has no useable data", actual.Message);
                }
            }

            await this.AssertMetadataBefore(test, target.Path);
            var after = await this.fileUtilities.CalculateChecksumSha256(target.Path);

            Assert.Equal(before, after);

            // check no new files are made
            target.Directory.GetFiles().Select(x => x.FullName)
                .Should().BeEquivalentTo(target.File.FullName.AsEnumerable());
        }

        [Theory]
        [MemberData(nameof(TestCaseData))]
        public async Task IsIdempotant(TestCase test)
        {
            var fixture = this.data[test.FixtureName];
            using var target = TempFile.DuplicateExistingDirectory(fixture.AbsoluteFixturePath);
            var path = target.Path;
            var dryRun = this.DryRunFactory(false);

            await this.AssertMetadataBefore(test, path);

            var actual = await this.fixer.ProcessFileAsync(path, dryRun);

            Assert.Equal(test.ExpectedStatus, actual.Status);
            if (test.ExpectedStatus is FixStatus.Fixed)
            {
                Assert.Contains("Partial file repaired", actual.Message);
            }
            else
            {
                // one of the following two cases
                if (actual.Message.Contains("empty"))
                {
                    Assert.Contains("Partial file was empty", actual.Message);
                }
                else if (actual.Message.Contains("short"))
                {
                    Assert.Contains("The file is to short to repair. At least three frames are needed.", actual.Message);
                }
                else
                {
                    Assert.Contains("Partial file was a stub and has no useable data", actual.Message);
                }
            }

            path = actual.NewPath;

            await this.AssertMetadataAfter(test, path);
            var first = await this.fileUtilities.CalculateChecksumSha256(path);

            // now again!
            var secondActual = await this.fixer.ProcessFileAsync(path, dryRun);

            Assert.Equal(FixStatus.NoOperation, secondActual.Status);
            if (test.NewName.Contains("error"))
            {
                Assert.Equal(CheckStatus.NotApplicable, secondActual.CheckResult.Status);
                Assert.Contains($"File is not named `data`", secondActual.Message);
            }
            else
            {
                Assert.Equal(CheckStatus.Repaired, secondActual.CheckResult.Status);
                Assert.Contains($"File has already been reconstructed", secondActual.Message);
            }

            var second = await this.fileUtilities.CalculateChecksumSha256(path);

            Assert.Equal(first, second);

            await this.AssertMetadataAfter(test, path);
        }

        private async Task AssertMetadataBefore(TestCase testCase, string path)
        {
            var name = this.CurrentFileSystem.Path.GetFileName(path);
            Assert.Equal("data", name);

            using var stream = this.CurrentFileSystem.File.OpenRead(path);
            Assert.Equal(testCase.OldSize, stream.Length);

            var actualSamples = await Flac.ReadTotalSamples(stream).ToEitherAsync().MapLeft(e => e.Message);
            Assert.Equal(testCase.OldSamples, actualSamples);

            var actualFirmware = await ReadFirmwareAsync(stream);
            if (testCase.OldSamples.IsLeft)
            {
                Assert.Equal(Flac.FileTooShort, actualFirmware);
            }
            else if (testCase.ExpectNoFirmware)
            {
                actualFirmware.IsFail.Should().BeTrue();
            }
            else
            {
                actualFirmware.ThrowIfFail().Tags.Should().BeEquivalentTo(Array.Empty<string>());
            }
        }

        private async Task AssertMetadataAfter(TestCase testCase, string path)
        {
            var name = this.CurrentFileSystem.Path.GetFileName(path);
            Assert.Equal(testCase.NewName, name);

            using var stream = this.CurrentFileSystem.File.OpenRead(path);
            Assert.Equal(testCase.NewSize, stream.Length);

            var fileSizeDelta = testCase.OldSize - testCase.NewSize;
            if (fileSizeDelta != 0)
            {
                var fragment = path + ".truncated_part";
                this.CurrentFileSystem.File.Exists(fragment).Should().BeTrue($"{fragment} should exist");
                this.CurrentFileSystem.FileInfo.New(fragment).Length.Should().Be(fileSizeDelta);
            }

            var actualSamples = await Flac.ReadTotalSamples(stream).ToEitherAsync().MapLeft(e => e.Message);
            Assert.Equal(testCase.NewSamples, actualSamples);

            var actualFirmware = await ReadFirmwareAsync(stream);
            if (testCase.NewSamples.IsLeft)
            {
                Assert.Equal(Flac.FileTooShort, actualFirmware);
            }
            else if (testCase.ExpectNoFirmware)
            {
                actualFirmware.IsFail.Should().BeTrue();
            }
            else
            {
                actualFirmware.ThrowIfFail().Tags.Should().BeEquivalentTo(testCase.FirmwareTags);
            }
        }

        public class TestCase : IXunitSerializable
        {
            [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
            public TestCase()
            {
            }

            public TestCase(
                string fixtureName,
                FixStatus expectedStatus,
                string newName,
                Either<string, ulong> oldSamples,
                Either<string, ulong> newSamples,
                long oldSize,
                long newSize,
                params string[] firmwareTags)
            {
                this.FixtureName = fixtureName;
                this.ExpectedStatus = expectedStatus;
                this.NewName = newName;
                this.OldSamples = oldSamples;
                this.NewSamples = newSamples;
                this.OldSize = oldSize;
                this.NewSize = newSize;
                this.FirmwareTags = firmwareTags;
                this.ExpectNoFirmware = false;
            }

            public TestCase(
                string fixtureName,
                FixStatus expectedStatus,
                string newName,
                Either<string, ulong> oldSamples,
                Either<string, ulong> newSamples,
                long oldSize,
                long newSize,
                bool expectNoFirmware = false)
            {
                this.FixtureName = fixtureName;
                this.ExpectedStatus = expectedStatus;
                this.NewName = newName;
                this.OldSamples = oldSamples;
                this.NewSamples = newSamples;
                this.OldSize = oldSize;
                this.NewSize = newSize;
                this.FirmwareTags = Array.Empty<string>();
                this.ExpectNoFirmware = expectNoFirmware;
            }

            public string FixtureName { get; set; }

            public FixStatus ExpectedStatus { get; set; }

            public string NewName { get; set; }

            public Either<string, ulong> OldSamples { get; set; }

            public Either<string, ulong> NewSamples { get; set; }

            public long OldSize { get; set; }

            public long NewSize { get; set; }

            public string[] FirmwareTags { get; set; }

            public bool ExpectNoFirmware { get; set; }

            public override string ToString()
            {
                return this.FixtureName;
            }

            public void Deserialize(IXunitSerializationInfo info)
            {
                var text = info.GetValue<string>("Value");
                JsonConvert.PopulateObject(text, this, Helpers.JsonSettings);
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                var json = JsonConvert.SerializeObject(this, Helpers.JsonSettings);
                info.AddValue("Value", json);
            }
        }
    }
}
