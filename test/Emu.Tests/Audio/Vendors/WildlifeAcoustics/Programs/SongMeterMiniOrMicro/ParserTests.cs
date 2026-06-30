// <copyright file="ParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro;

using System.Threading.Tasks;
using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
using Emu.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro;
using Emu.Audio.Vendors.WildlifeAcoustics.WAMD;
using Emu.Tests.TestHelpers;
using FluentAssertions;
using FluentAssertions.LanguageExt;
using FluentAssertions.Primitives;
using LanguageExt;
using Xunit.Abstractions;

public partial class ParserTests : TestBase
{
    public ParserTests(ITestOutputHelper output)
        : base(output, realFileSystem: true)
    {
    }

    [Theory]
    [MemberData(nameof(ProgramsInFiles))]
    public void CanReadConfigAndScheduleFromFile(string path, Program expected)
    {
        var resolved = FixtureHelper.ResolvePath(path);
        var stream = this.CurrentFileSystem.File.OpenRead(resolved);

        var tryWamdData = WamdParser.ExtractMetadata(stream);
        Assert.True(tryWamdData.IsSucc);

        (var wamdData, var notices) = tryWamdData.ThrowIfFail();

        var actual = new Program()
        {
            Configuration = wamdData.SmmConfig,
            Schedule = wamdData.SmmSchedule,
        };

        actual.Should().BeEquivalentTo(expected, o => o.WithTracing());
    }

    [Theory]
    [MemberData(nameof(ConfigFiles))]
    public async Task CanReadConfigFromFile(string path, Program expected)
    {
        var resolved = FixtureHelper.ResolvePath(path);
        var stream = this.CurrentFileSystem.File.OpenRead(resolved);

        var result = await Parser.GetProgramFromConfigFileAsync(stream);

        result.IsSucc.Should().BeTrue();

        var actual = result.ThrowIfFail();

        actual.Should().BeEquivalentTo(expected, o => o.WithTracing());
    }
}
