// <copyright file="ProgramParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Vendors.WildlifeAcoustics.Programs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes;
    using Emu.Audio.Vendors.WildlifeAcoustics.WAMD;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using FluentAssertions.LanguageExt;
    using FluentAssertions.Primitives;
    using LanguageExt;

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
            var resolved = FixtureHelper.ResolvePath(path);
            var stream = this.CurrentFileSystem.File.OpenRead(resolved);

            var actual = await ProgramParser.GetProgramFromScheduleFileAsync(stream);

            actual.IsSucc.Should().BeTrue();

            ((SongMeter4Program)actual).Should().BeEquivalentTo(expected, o => o.WithTracing());
        }

        [Theory]
        [MemberData(nameof(SM4ProgramsInRecording))]
        public void CanReadSM4ProgramFromRecording(string path)
        {
            var expected = Programs[path];
            var resolved = FixtureHelper.ResolvePath(path);
            var stream = this.CurrentFileSystem.File.OpenRead(resolved);

            var tryWamdData = WamdParser.ExtractMetadata(stream);

            Assert.True(tryWamdData.IsSucc);

            (var wamdData, _) = tryWamdData.ThrowIfFail();

            var actual = wamdData.DevParams;

            ((SongMeter4Program)actual).Should().BeEquivalentTo(expected);
        }

        [Theory]
        [MemberData(nameof(SM3Programs))]
        public async Task CanReadSM3Program(string path)
        {
            var expected = Programs[path];
            var resolved = FixtureHelper.ResolvePath(path);
            var stream = this.CurrentFileSystem.File.OpenRead(resolved);

            var actual = await ProgramParser.GetProgramFromScheduleFileAsync(stream);

            actual.Should().BeSuccess();

            ((SongMeter3Program)actual).Should().BeEquivalentTo(expected);
        }

        [Theory]
        [MemberData(nameof(SM3ProgramsInRecordings))]
        public void CanReadSM3ProgramFromRecording(string path)
        {
            var expected = Programs[path];
            var resolved = FixtureHelper.ResolvePath(path);
            var stream = this.CurrentFileSystem.File.OpenRead(resolved);

            var tryWamdData = WamdParser.ExtractMetadata(stream);

            Assert.True(tryWamdData.IsSucc);

            (var wamdData, _) = tryWamdData.ThrowIfFail();

            var actual = wamdData.DevParams;

            ((SongMeter3Program)actual).Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void CanReadAdvancedScheduleWhenSectionIsCompletelyFilled()
        {
            var bytes = new byte[1124];
            "SM4P"u8.CopyTo(bytes.AsSpan(0, 4));

            // model = SM4
            bytes[604] = 0;
            bytes[605] = 0;

            const uint repeatRaw = (uint)AdvancedScheduleEntryType.REPEAT << 26;
            for (var i = 0; i < 99; i++)
            {
                var offset = 728 + (i * 4);
                bytes[offset + 0] = unchecked((byte)(repeatRaw >> 16));
                bytes[offset + 1] = unchecked((byte)(repeatRaw >> 24));
                bytes[offset + 2] = unchecked((byte)(repeatRaw >> 0));
                bytes[offset + 3] = unchecked((byte)(repeatRaw >> 8));
            }

            var actual = ProgramParser.Parse(bytes);

            actual.Should().BeSuccess();
            var schedule = ((SongMeter4Program)actual).AdvancedSchedule;
            schedule.Count.Should().Be(99);
            schedule.All(x => x is Repeat).Should().BeTrue();
        }
    }
}
