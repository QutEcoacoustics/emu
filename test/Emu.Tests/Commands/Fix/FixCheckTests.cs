// <copyright file="FixCheckTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Commands.Fix
{
    using System;
    using System.CommandLine;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Fixes;
    using Emu.Metadata;
    using Emu.Serialization;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using Xunit.Abstractions;
    using static Emu.EmuCommand;
    using static Emu.FixCheck;

    public class FixCheckTests
    {
        public class DefaultFormatTests : TestBase, IClassFixture<FixtureData>
        {
            private readonly FixCheck command;
            private readonly FixtureData data;

            public DefaultFormatTests(ITestOutputHelper output, FixtureData data)
                : base(output, true, Emu.EmuCommand.OutputFormat.Default)
            {
                var formatter = new AnsiConsoleFormatter(this.BuildLogger<AnsiConsoleFormatter>());
                var writer = new OutputRecordWriter(
                    this.ServiceProvider.GetRequiredService<TextWriter>(),
                    formatter,
                    new Lazy<OutputFormat>(() => this.OutputFormat));

                formatter.ColorSystemSupport = Spectre.Console.ColorSystemSupport.NoColors;

                this.command = new FixCheck(
                    this.BuildLogger<FixCheck>(),
                    this.ServiceProvider.GetRequiredService<FileMatcher>(),
                    this.ServiceProvider.GetRequiredService<FixRegister>(),
                    writer,
                    this.CurrentFileSystem)
                {
                };
                this.data = data;
            }

            [Fact]
            public async Task DefaultFormatEmitsASummaryTable()
            {
                var fixture = this.data[FixtureModel.SpaceInDateStamp];

                this.command.Targets = new[] { fixture.AbsoluteFixturePath };
                this.command.Fix = new string[]
                {
                    WellKnownProblems.FrontierLabsProblems.InvalidDateStampSpaceZero.Id,
                    WellKnownProblems.FrontierLabsProblems.MetadataDurationBug.Id,
                    WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader.Id,
                };

                var result = await this.command.InvokeAsync(null);
                result.Should().Be(ExitCodes.Success);

                this.AllOutput.Should().Contain("│ ID     │ Affected │ Unaffected │ NotApplicable │ Repaired │ Error │");
                this.AllOutput.Should().Contain("│ FL008  │ 1        │ 0          │ 0             │ 0        │ 0     │");
                this.AllOutput.Should().Contain("│ FL010  │ 0        │ 1          │ 0             │ 0        │ 0     │");
                this.AllOutput.Should().Contain("│ FL001  │ 0        │ 1          │ 0             │ 0        │ 0     │");
                this.AllOutput.Should().Contain("│ Totals │ 1        │ 2          │ 0             │ 0        │ 0     │");
            }

            [Fact]
            public async Task DefaultHandlesNoTargets()
            {
                this.command.Targets = new string[] { };
                this.command.Fix = new string[]
                {
                    WellKnownProblems.FrontierLabsProblems.InvalidDateStampSpaceZero.Id,
                    WellKnownProblems.FrontierLabsProblems.MetadataDurationBug.Id,
                    WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader.Id,
                };

                var result = await this.command.InvokeAsync(null);
                result.Should().Be(ExitCodes.Success);

                this.AllOutput.Should().Contain("── Summary for 0 files ──");
                this.AllOutput.Should().Contain("No files matched targets: ");
            }
        }
    }
}
