// <copyright file="CuesTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands.Cues
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Commands.Cues;
    using Emu.Serialization;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using MoreLinq;
    using Rationals;
    using Xunit;
    using Xunit.Abstractions;

    public class CuesTests : TestBase, IClassFixture<FixtureData>
    {
        private readonly Cues command;
        private readonly FixtureData data;
        private readonly JsonLinesSerializer serializer;

        public CuesTests(ITestOutputHelper output, FixtureData data)
            : base(output, true)
        {
            this.command = new Cues(
                this.BuildLogger<Cues>(),
                this.RealFileSystem,
                this.ServiceProvider.GetRequiredService<FileMatcher>(),
                this.GetOutputRecordWriter());
            this.data = data;

            this.serializer = this.ServiceProvider.GetRequiredService<JsonLinesSerializer>();
        }

        [Fact]
        public async Task RunsSuccessfully()
        {
            this.command.Targets = new string[]
            {
                this.data[FixtureModel.GenericWaveWithCueChunk].AbsoluteFixturePath,
                this.data[FixtureModel.GenericWaveWithCueAndLabelChunks].AbsoluteFixturePath,
            };

            var result = await this.command.InvokeAsync(null);
            result.Should().Be(ExitCodes.Success);

            using var reader = new StringReader(this.AllOutput);
            var cues = this.serializer.Deserialize<CueResult>(reader).ToArray();

            cues.Should().HaveCount(9);
            cues.Select(c => c.Position).Should().Equal(new[]
            {
                9.642000,
                12.928000,
                16.768000,
                20.224000,
                11.346000,
                16.122000,
                16.386000,
                19.770000,
                23.802000,
            }.Select(d => Rational.Approximate(d)));

            cues.Select(c => c.Cue.Label).Should().Equal(new[]
            {
                null,
                null,
                null,
                null,
                "MARK_01",
                "MARK_02",
                "MARK_03",
                "MARK_04",
                "MARK_05",
            });
        }

        [Fact]
        public async Task HasAnExportOption()
        {
            var targets = new string[]
            {
                this.data[FixtureModel.GenericWaveWithCueChunk].AbsoluteFixturePath,
                this.data[FixtureModel.GenericWaveWithCueAndLabelChunks].AbsoluteFixturePath,
            }.Select(p => TempFile.DuplicateExisting(p)).ToArray();

            this.command.Targets = targets.Select(x => x.Path).ToArray();
            this.command.Export = true;

            var result = await this.command.InvokeAsync(null);
            result.Should().Be(ExitCodes.Success);

            var sony = ReadCue(targets.First(x => x.File.Name.Contains("Sony")).Directory);
            sony.Should().Be(@"9.642000	<no label>
12.928000	<no label>
16.768000	<no label>
20.224000	<no label>
".NormalizeLineEndings());

            var tascam = ReadCue(targets.First(x => x.File.Name.Contains("Tascam")).Directory);
            tascam.Should().Be(@"11.346000	MARK_01
16.122000	MARK_02
16.386000	MARK_03
19.770000	MARK_04
23.802000	MARK_05
".NormalizeLineEndings());

            string ReadCue(DirectoryInfo info)
            {
                var cueFile = info.GetFiles("*.cue.txt").Single();
                return this.CurrentFileSystem.File.ReadAllText(cueFile.FullName);
            }

            targets.ForEach(x => x.Dispose());
        }
    }
}
