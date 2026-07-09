// <copyright file="CorruptDataTests.cs" company="QutEcoacoustics">
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

    public class CorruptDataTests : TestBase, IClassFixture<FixtureData>
    {
        private readonly CorruptData fixer;
        private readonly FixtureData data;

        public CorruptDataTests(ITestOutputHelper output, FixtureData data)
            : base(output, realFileSystem: true)
        {
            this.fixer = this.ServiceProvider.GetRequiredService<CorruptData>();
            this.data = data;
        }

        [Fact]
        public void ItsMetadataIsCorrect()
        {
            var info = this.fixer.GetOperationInfo();

            info.Fixable.Should().BeFalse();
            info.Automatic.Should().BeFalse();
            info.Safe.Should().BeTrue();
            info.Suffix.Should<Option<string>>().Be(Some("corrupt"));

            Assert.False(this.fixer is IFixOperation);
        }

        [Theory]
        [InlineData(FixtureModel.Wa004Sample1)]
        [InlineData(FixtureModel.Wa004Sample2)]
        [InlineData(FixtureModel.Wa004Sample3)]
        public async Task CanDetectCorruptFiles(string fixtureName)
        {
            var fixture = this.data[fixtureName];

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("no valid RIFF/WAVE header", actual.Message);
            Assert.Null(actual.Data);
            Assert.Equal(Severity.Severe, actual.Severity);
        }

        [Theory]
        [InlineData(FixtureModel.SongMeterMiniNormalFile1)]
        [InlineData(FixtureModel.SongMeterMiniNormalFile2)]
        public async Task DoesNotDetectNormalFiles(string fixtureName)
        {
            var fixture = this.data[fixtureName];

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Unaffected, actual.Status);
            Assert.Equal(string.Empty, actual.Message);
            Assert.Null(actual.Data);
            Assert.Equal(Severity.None, actual.Severity);
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
            Skip.If(fixture.Name is FixtureModel.Wa004Sample1 or FixtureModel.Wa004Sample2 or FixtureModel.Wa004Sample3);

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Unaffected, actual.Status);
            Assert.Equal(string.Empty, actual.Message);
            Assert.Null(actual.Data);
            Assert.Equal(Severity.None, actual.Severity);
        }
    }
}
