// <copyright file="ProgramParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Vendors.WildlifeAcoustics.Programs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Audio.Vendors.WildlifeAcoustics;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using Emu.Audio.Vendors.WildlifeAcoustics.WAMD;
    using Emu.Models;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using FluentAssertions.Equivalency;
    using FluentAssertions.Primitives;
    using LanguageExt;
    using NodaTime;
    using Shouldly;
    using Xunit.Abstractions;
    using Duration = NodaTime.Duration;

    public partial class ProgramParserTests : TestBase
    {
        public ProgramParserTests(ITestOutputHelper output)
            : base(output, realFileSystem: true)
        {
        }

        [Theory]
        [MemberData(nameof(SM4Programs))]
        public async Task CanReadSM4Program(string path)
        {
            var expected = Programs[path];
            var resovled = FixtureHelper.ResolvePath(path);
            var stream = this.CurrentFileSystem.File.OpenRead(resovled);

            var actual = await ProgramParser.GetProgramFromScheduleFileAsync(stream);

            actual.IsSucc.Should().BeTrue();

            ((SongMeter4Program)actual).Should().BeEquivalentTo(expected, o => o.WithTracing());
        }

        [Theory]
        [MemberData(nameof(SM4ProgramsInRecording))]
        public void CanReadSM4ProgramFromRecording(string path)
        {
            var expected = Programs[path];
            var resovled = FixtureHelper.ResolvePath(path);
            var stream = this.CurrentFileSystem.File.OpenRead(resovled);

            var tryWamdData = WamdParser.ExtractMetadata(stream);

            Assert.True(tryWamdData.IsSucc);

            var wamdData = (Wamd)tryWamdData;

            var actual = wamdData.DevParams;

            ((SongMeter4Program)actual).Should().BeEquivalentTo(expected);
        }

        [Theory]
        [MemberData(nameof(SM3Programs))]
        public async Task CanReadSM3Program(string path)
        {
            var expected = Programs[path];
            var resovled = FixtureHelper.ResolvePath(path);
            var stream = this.CurrentFileSystem.File.OpenRead(resovled);

            var actual = await ProgramParser.GetProgramFromScheduleFileAsync(stream);

            actual.IsSucc.Should().BeTrue();

            ((SongMeter3Program)actual).Should().BeEquivalentTo(expected);
        }

        [Theory]
        [MemberData(nameof(SM3ProgramsInRecordings))]
        public void CanReadSM3ProgramFromRecording(string path)
        {
            var expected = Programs[path];
            var resovled = FixtureHelper.ResolvePath(path);
            var stream = this.CurrentFileSystem.File.OpenRead(resovled);

            var tryWamdData = WamdParser.ExtractMetadata(stream);

            Assert.True(tryWamdData.IsSucc);

            var wamdData = (Wamd)tryWamdData;

            var actual = wamdData.DevParams;

            ((SongMeter3Program)actual).Should().BeEquivalentTo(expected);
        }
    }
}
