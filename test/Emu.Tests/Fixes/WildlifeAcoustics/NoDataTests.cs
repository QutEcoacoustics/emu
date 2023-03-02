// <copyright file="NoDataTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Fixes.WildlifeAcoustics
{
    using System;
    using System.Threading.Tasks;
    using Emu.Fixes;
    using Emu.Fixes.WildlifeAcoustics;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using LanguageExt;
    using Xunit;
    using Xunit.Abstractions;
    using static LanguageExt.Prelude;

    public class NoDataTests : TestBase, IClassFixture<FixtureData>
    {
        private readonly NoData fixer;
        private readonly FixtureData data;

        public NoDataTests(ITestOutputHelper output, FixtureData data)
            : base(output, realFileSystem: true)
        {
            this.fixer = this.ServiceProvider.GetRequiredService<NoData>();
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
        public async Task CanDetectNoDataFiles()
        {
            var fixture = this.data[FixtureModel.NoDataHeader];

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("The file has only null bytes and has no usable data.", actual.Message);

            Assert.Null(actual.Data);

            Assert.Equal(Severity.Severe, actual.Severity);
        }

        [Fact]
        public async Task CanDetectNoDataFiles2()
        {
            var fixture = this.data[FixtureModel.NoDataHeader2];

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("The file has only null bytes and has no usable data.", actual.Message);

            Assert.Null(actual.Data);

            Assert.Equal(Severity.Severe, actual.Severity);
        }

        [Fact]
        public async Task ItDoesNotConsiderEmptyFilesAFault()
        {
            using var target = new TempFile();

            target.Path.Touch(this.RealFileSystem);

            var actual = await this.fixer.CheckAffectedAsync(target.Path);

            Assert.Equal(CheckStatus.Unaffected, actual.Status);
            Assert.Equal(string.Empty, actual.Message);
            Assert.Null(actual.Data);
            Assert.Equal(Severity.None, actual.Severity);
        }

        [SkippableTheory]
        [ClassData(typeof(FixtureData))]
        public async Task NoOtherFixtureIsDetectedAsAPositive(FixtureModel fixture)
        {
            Skip.If(fixture.Name is FixtureModel.NoDataHeader or FixtureModel.NoDataHeader2);

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Unaffected, actual.Status);
            Assert.Equal(string.Empty, actual.Message);
            Assert.Null(actual.Data);
            Assert.Equal(Severity.None, actual.Severity);
        }
    }
}
