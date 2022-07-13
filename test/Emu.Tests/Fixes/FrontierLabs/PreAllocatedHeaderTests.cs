// <copyright file="PreAllocatedHeaderTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Fixes.FrontierLabs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
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

    public class PreAllocatedHeaderTests : TestBase, IClassFixture<FixtureHelper.FixtureData>
    {
        private readonly PreAllocatedHeader fixer;
        private readonly FixtureHelper.FixtureData data;

        public PreAllocatedHeaderTests(ITestOutputHelper output, FixtureHelper.FixtureData data)
            : base(output, realFileSystem: true)
        {
            this.fixer = this.ServiceProvider.GetRequiredService<PreAllocatedHeader>();
            this.data = data;
        }

        [Fact]
        public void ItsMetadataIsCorrect()
        {
            var info = this.fixer.GetOperationInfo();

            info.Fixable.Should().BeFalse();
            info.Automatic.Should().BeFalse();
            info.Safe.Should().BeTrue();

            Assert.False(this.fixer is IFixOperation);
        }

        [Fact]
        public async Task CanDetectPreAllocatedFiles()
        {
            var fixture = this.data[FixtureModel.PreAllocatedHeader];

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("The file is a stub and has no usable data", actual.Message);

            Assert.Null(actual.Data);

            Assert.Equal(Severity.Severe, actual.Severity);
        }

        [Fact]
        public async Task CanDetectPreAllocatedFiles2()
        {
            var fixture = this.data[FixtureModel.PreAllocatedHeader];

            var actual = await this.fixer.CheckAffectedAsync(fixture.AbsoluteFixturePath);

            Assert.Equal(CheckStatus.Affected, actual.Status);
            Assert.Contains("The file is a stub and has no usable data", actual.Message);

            Assert.Null(actual.Data);

            Assert.Equal(Severity.Severe, actual.Severity);
        }

        [Fact]
        public async Task ItDoesNotConsiderEmptyFilesAFault()
        {
            using var target = new TempFile();

            target.Path.Touch(this.RealFileSystem);

            var actual = await this.fixer.CheckAffectedAsync(target.Path);

            Assert.Equal(CheckStatus.NotApplicable, actual.Status);
            Assert.Equal(string.Empty, actual.Message);
            Assert.Null(actual.Data);
            Assert.Equal(Severity.None, actual.Severity);
        }

        [SkippableTheory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async Task NoOtherFixtureIsDetectedAsAPositive(FixtureModel fixture)
        {
            Skip.If(fixture.Name is FixtureModel.PreAllocatedHeader or FixtureModel.PreAllocatedHeader2);

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
