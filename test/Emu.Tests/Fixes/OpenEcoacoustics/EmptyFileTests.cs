// <copyright file="EmptyFileTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Fixes.OpenEcoacoustics
{
    using System;
    using System.Threading.Tasks;
    using Emu.Fixes;
    using Emu.Fixes.OpenEcoacoustics;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using LanguageExt;
    using Xunit;
    using Xunit.Abstractions;
    using static LanguageExt.Prelude;

    public class EmptyFileTests : TestBase, IClassFixture<FixtureHelper.FixtureData>
    {
        private readonly EmptyFile fixer;
        private readonly FixtureHelper.FixtureData data;

        public EmptyFileTests(ITestOutputHelper output, FixtureHelper.FixtureData data)
            : base(output, realFileSystem: true)
        {
            this.fixer = this.ServiceProvider.GetRequiredService<EmptyFile>();
            this.data = data;
        }

        [Fact]
        public void ItsMetadataIsCorrect()
        {
            var info = this.fixer.GetOperationInfo();

            info.Fixable.Should().BeFalse();
            info.Automatic.Should().BeFalse();
            info.Safe.Should().BeTrue();
            info.Suffix.Should<Option<string>>().Be(Some("empty"));

            Assert.False(this.fixer is IFixOperation);
        }

        [Fact]
        public async Task CanDetectPreAllocatedFiles()
        {
            var fixture = this.data[FixtureModel.TwoLogFiles1];

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("The file is empty - it has a size of 0 bytes", actual.Message);

            Assert.Null(actual.Data);

            Assert.Equal(Severity.Severe, actual.Severity);
        }

        [SkippableTheory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async Task NoOtherFixtureIsDetectedAsAPositive(FixtureModel fixture)
        {
            Skip.If(fixture.Record.FileSizeBytes == 0);

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            if (fixture.ToFileInfo(this.RealFileSystem).Length == 0)
            {
                Assert.Equal(CheckStatus.NotApplicable, actual.Status);
            }
            else
            {
                Assert.Equal(CheckStatus.Unaffected, actual.Status);
            }

            Assert.Equal(string.Empty, actual.Message);
            Assert.Null(actual.Data);
            Assert.Equal(Severity.None, actual.Severity);
        }
    }
}
