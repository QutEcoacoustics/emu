// <copyright file="FixApplyTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands.Fix
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Fixes;
    using Emu.Serialization;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using Xunit.Abstractions;
    using static Emu.FixApply;

    public class FixApplyTests : TestBase, IClassFixture<FixtureHelper.FixtureData>
    {
        private readonly FixtureHelper.FixtureData data;
        private readonly FixApply command;
        private readonly FileUtilities fileUtilities;
        private readonly JsonLinesSerializer serializer;
        private TempFile target;

        public FixApplyTests(ITestOutputHelper output, FixtureHelper.FixtureData data)
            : base(output, true)
        {
            this.data = data;
            this.fileUtilities = this.ServiceProvider.GetRequiredService<FileUtilities>();

            this.command = new FixApply(
                this.BuildLogger<FixApply>(),
                this.DryRunFactory,
                this.ServiceProvider.GetRequiredService<FileMatcher>(),
                this.ServiceProvider.GetRequiredService<FixRegister>(),
                this.GetOutputRecordWriter(),
                this.CurrentFileSystem,
                this.fileUtilities);

            this.serializer = this.ServiceProvider.GetRequiredService<JsonLinesSerializer>();
        }

        public override void Dispose()
        {
            this.target?.Dispose();
            base.Dispose();
        }

        [Fact]
        public async Task WillBackupIfRequested()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug];
            this.target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);
            var backupPath = this.target.Path + ".bak";

            this.command.Backup = true;
            this.command.DryRun = false;
            this.command.Targets = new[] { this.target.Path };
            this.command.Fix = new string[] { WellKnownProblems.FrontierLabsProblems.MetadataDurationBug.Id };

            var before = await this.fileUtilities.CalculateChecksumSha256(this.target.Path);

            var result = await this.command.InvokeAsync(null);
            result.Should().Be(ExitCodes.Success);

            using var reader = new StringReader(this.AllOutput);
            var fix = this.serializer.Deserialize<FixApplyResult>(reader).ToArray();

            fix.Should().NotBeNull();
            fix.Should().HaveCount(1);
            var fixResult = fix.First();

            fixResult.BackupFile.Should().Be(backupPath);
            var problem = fixResult.Problems.First();
            problem.Key.Should().Be(WellKnownProblems.FrontierLabsProblems.MetadataDurationBug);
            problem.Value.Status.Should().Be(FixStatus.Fixed);

            // the modified file has a different hash
            var modified = await this.fileUtilities.CalculateChecksumSha256(this.target.Path);
            before.Should().NotBe(modified);

            // the backup file has the same hash as the original
            var backup = await this.fileUtilities.CalculateChecksumSha256(backupPath);
            before.Should().Be(backup);

            this.RealFileSystem.File.Delete(backupPath);
        }

        [Fact]
        public async Task WillNotBackupIfDryRun()
        {
            var fixture = this.data[FixtureModel.MetadataDurationBug];
            this.target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);
            var backupPath = this.target.Path + ".bak";

            this.command.Backup = true;
            this.command.DryRun = true;
            this.command.Targets = new[] { this.target.Path };
            this.command.Fix = new string[] { WellKnownProblems.FrontierLabsProblems.MetadataDurationBug.Id };

            var before = await this.fileUtilities.CalculateChecksumSha256(this.target.Path);

            var result = await this.command.InvokeAsync(null);
            result.Should().Be(ExitCodes.Success);

            using var reader = new StringReader(this.AllOutput);
            var fix = this.serializer.Deserialize<FixApplyResult>(reader).ToArray();

            fix.Should().NotBeNull();
            fix.Should().HaveCount(1);
            var fixResult = fix.First();

            fixResult.BackupFile.Should().Be(backupPath);
            var problem = fixResult.Problems.First();
            problem.Key.Should().Be(WellKnownProblems.FrontierLabsProblems.MetadataDurationBug);
            problem.Value.Status.Should().Be(FixStatus.Fixed);

            // the file should not have been modified
            var after = await this.fileUtilities.CalculateChecksumSha256(this.target.Path);
            before.Should().Be(after);

            // the backup file should not have been created
            this.RealFileSystem.File.Exists(fixResult.BackupFile).Should().BeFalse();
        }

        [Fact]
        public async Task WillRenameIfUnfixableErrorEncountered()
        {
            var fixture = this.data[FixtureModel.PreAllocatedHeader];
            this.target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            this.command.NoRename = false;
            this.command.Targets = new[] { this.target.Path };
            this.command.Fix = new string[] { WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader.Id };

            var result = await this.command.InvokeAsync(null);
            result.Should().Be(ExitCodes.Success);

            using var reader = new StringReader(this.AllOutput);
            var fix = this.serializer.Deserialize<FixApplyResult>(reader).Single();

            fix.BackupFile.Should().BeNull();
            var expected = this.target.Path + ".error_FL001";
            fix.File.Should().Be(expected);
            var problem = fix.Problems[WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader];

            problem.CheckResult.Message.Should().Contain("The file is a stub and has no usable data");
            problem.Message.Should().Contain("Renamed");

            problem.Status.Should().Be(FixStatus.Renamed);

            this.RealFileSystem.File.Delete(expected);
        }

        [Fact]
        public async Task WillNotRenameIfUnfixableErrorEncounteredIfNoRenameOptionSpecified()
        {
            var fixture = this.data[FixtureModel.PreAllocatedHeader];
            this.target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            this.command.NoRename = true;
            this.command.Targets = new[] { this.target.Path };
            this.command.Fix = new string[] { WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader.Id };

            var result = await this.command.InvokeAsync(null);
            result.Should().Be(ExitCodes.Success);

            using var reader = new StringReader(this.AllOutput);
            var fix = this.serializer.Deserialize<FixApplyResult>(reader).Single();

            fix.BackupFile.Should().BeNull();
            var expected = this.target.Path;
            fix.File.Should().Be(expected);
            var problem = fix.Problems[WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader];

            problem.CheckResult.Message.Should().Contain("The file is a stub and has no usable data");
            problem.Message.Should().NotContain("Renamed");

            problem.Status.Should().Be(FixStatus.NotFixed);
        }

        [Fact]
        public async Task WillNotRenameIfUnfixableErrorEncounteredIfDryRun()
        {
            var fixture = this.data[FixtureModel.PreAllocatedHeader];
            this.target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            var before = await this.fileUtilities.CalculateChecksumSha256(this.target.Path);

            this.command.Targets = new[] { this.target.Path };
            this.command.DryRun = true;
            this.command.Fix = new string[] { WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader.Id };

            var result = await this.command.InvokeAsync(null);
            result.Should().Be(ExitCodes.Success);

            using var reader = new StringReader(this.AllOutput);
            var fix = this.serializer.Deserialize<FixApplyResult>(reader).Single();

            fix.BackupFile.Should().BeNull();
            var expected = this.target.Path;
            fix.File.Should().Be(expected);
            var problem = fix.Problems[WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader];

            problem.CheckResult.Message.Should().Contain("The file is a stub and has no usable data");
            problem.Message.Should().Contain("Renamed");

            problem.Status.Should().Be(FixStatus.Renamed);

            var after = await this.fileUtilities.CalculateChecksumSha256(this.target.Path);
            before.Should().Be(after);
        }

        [Fact]
        public async Task WillRenameIfUnfixableErrorEncounteredWithMultipleFixes()
        {
            var fixture = this.data[FixtureModel.PreAllocatedHeader];
            this.target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            this.command.NoRename = false;
            this.command.Targets = new[] { this.target.Path };
            this.command.Fix = new string[]
            {
                WellKnownProblems.FrontierLabsProblems.MetadataDurationBug.Id,
                WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader.Id,
            };

            var result = await this.command.InvokeAsync(null);
            result.Should().Be(ExitCodes.Success);

            using var reader = new StringReader(this.AllOutput);
            var fix = this.serializer.Deserialize<FixApplyResult>(reader).Single();

            fix.BackupFile.Should().BeNull();
            var expected = this.target.Path + ".error_FL001";
            fix.File.Should().Be(expected);
            var problem = fix.Problems[WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader];

            problem.CheckResult.Message.Should().Contain("The file is a stub and has no usable data");
            problem.Message.Should().Contain("Renamed");
            problem.Status.Should().Be(FixStatus.Renamed);

            var otherProblem = fix.Problems[WellKnownProblems.FrontierLabsProblems.MetadataDurationBug];
            otherProblem.Status.Should().Be(FixStatus.NoOperation);

            this.RealFileSystem.File.Delete(expected);
        }

        [Fact]
        public async Task CanProcessMultipleFixesAfterARenameFix()
        {
            var fixture = this.data[FixtureModel.SpaceInDateStamp];
            this.target = TempFile.DuplicateExisting(fixture.AbsoluteFixturePath);

            this.command.NoRename = false;
            this.command.Targets = new[] { this.target.Path };
            this.command.Fix = new string[]
            {
                WellKnownProblems.FrontierLabsProblems.InvalidDateStampSpaceZero.Id,
                WellKnownProblems.FrontierLabsProblems.MetadataDurationBug.Id,
                WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader.Id,
            };

            var result = await this.command.InvokeAsync(null);
            result.Should().Be(ExitCodes.Success);

            using var reader = new StringReader(this.AllOutput);
            var fix = this.serializer.Deserialize<FixApplyResult>(reader).Single();

            fix.BackupFile.Should().BeNull();
            var expected = this.CurrentFileSystem.Path.Join(this.target.Directory.FullName, "20190607T095935+1000_REC [19.2144 152.8811].flac");
            fix.File.Should().Be(expected);

            var problem = fix.Problems[WellKnownProblems.FrontierLabsProblems.InvalidDateStampSpaceZero];

            problem.CheckResult.Message.Should().Contain("Space in datestamp detected");
            problem.Message.Should().Contain("Inserted `0` into datestamp");
            problem.Status.Should().Be(FixStatus.Fixed);

            var problem2 = fix.Problems[WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader];
            problem2.Status.Should().Be(FixStatus.NoOperation);

            var problem3 = fix.Problems[WellKnownProblems.FrontierLabsProblems.MetadataDurationBug];
            problem3.Status.Should().Be(FixStatus.NoOperation);

            this.RealFileSystem.File.Delete(expected);
        }
    }
}
