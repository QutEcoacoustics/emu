// <copyright file="SpaceInDatestampTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Fixes.FrontierLabs
{
    using System;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Emu.Fixes;
    using Emu.Fixes.FrontierLabs;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class SpaceInDatestampTests : TestBase, IClassFixture<FixtureHelper.FixtureData>, IDisposable
    {
        private readonly FixtureModel fixture;
        private readonly TempFile target;
        private readonly FileUtilities fileUtilities;
        private readonly SpaceInDatestamp fixer;

        public SpaceInDatestampTests(ITestOutputHelper output, FixtureHelper.FixtureData data)
            : base(output, true)
        {
            this.fixture = data[FixtureModel.SpaceInDateStamp];
            this.target = TempFile.DuplicateExisting(this.fixture.AbsoluteFixturePath);

            this.fileUtilities = this.ServiceProvider.GetRequiredService<FileUtilities>();

            this.fixer = new SpaceInDatestamp(this.CurrentFileSystem, this.fileUtilities);
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
            info.Safe.Should().BeTrue();

            Assert.True(this.fixer is IFixOperation);
        }

        [Fact]
        public async Task CanDetectProblem()
        {
            var actual = await this.fixer.CheckAffectedAsync(this.target.Path);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("Space in datestamp detected", actual.Message);
            Assert.Equal(Severity.Mild, actual.Severity);
            Assert.Null(actual.Data);
        }

        [SkippableTheory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async Task NoOtherFixtureIsDetectedAsAPositive(FixtureModel fixture)
        {
            Skip.If(fixture.Name is FixtureModel.SpaceInDateStamp);

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Unaffected, actual.Status);

            Assert.Equal(string.Empty, actual.Message);
            Assert.Null(actual.Data);
            Assert.Equal(Severity.None, actual.Severity);
        }

        [Fact]
        public async Task CanFixProblem()
        {
            var dryRun = this.DryRunFactory(false);

            var actual = await this.fixer.ProcessFileAsync(this.target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Contains($"Inserted `0` into datestamp", actual.Message);
            Assert.NotNull(actual.NewPath);

            var basename = this.RealFileSystem.Path.GetFileName(actual.NewPath);
            Assert.DoesNotMatch(SpaceInDatestamp.Matcher, basename);

            // file was renamed
            this.target.File.Refresh();
            Assert.False(this.target.File.Exists);
            Assert.True(this.RealFileSystem.File.Exists(actual.NewPath));
        }

        [Fact]
        public async Task WillDoNothingInADryRun()
        {
            var dryRun = this.DryRunFactory(true);

            var actual = await this.fixer.ProcessFileAsync(this.target.Path, dryRun);

            Assert.Equal(FixStatus.Fixed, actual.Status);
            Assert.Contains($"Inserted `0` into datestamp", actual.Message);
            Assert.NotNull(actual.NewPath);

            var basename = this.RealFileSystem.Path.GetFileName(actual.NewPath);
            Assert.Matches(SpaceInDatestamp.Matcher, basename);

            // file was NOT renamed
            this.target.File.Refresh();
            Assert.True(this.target.File.Exists);
        }
    }
}
