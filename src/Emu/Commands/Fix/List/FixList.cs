// <copyright file="FixList.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System.CommandLine.Invocation;
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Fixes;
    using Emu.Utilities;
    using Spectre.Console;
    using static Emu.Cli.SpectreUtils;

    public class FixList : EmuCommandHandler<OperationInfo>
    {
        private Table table;

        public FixList(OutputRecordWriter writer)
        {
            this.Writer = writer;
            this.table = null;
        }

        public override Task<int> InvokeAsync(InvocationContext context)
        {
            this.WriteHeader();

            foreach (var fix in FixRegister.All)
            {
                this.Write(fix);
            }

            this.WriteFooter();
            this.WriteMessage(
$@"Use {MarkupCode("emu fix check")} or {MarkupCode("emu fix check --all")} to check all known fixes:
{MarkupCodeBlock("emu fix check --fix XX001 *.wav")}
Use {MarkupCode("emu fix apply")} to apply a fix to target files:
{MarkupCodeBlock("emu fix apply --fix XX001 *.wav")}");

            return Task.FromResult(ExitCodes.Success);
        }

        public override object FormatHeader(OperationInfo record)
        {
            this.table = new Table();
            this.table.AddColumn("ID");
            this.table.AddColumn("Description");
            this.table.AddColumn("Fixable");
            this.table.AddColumn("Safe");
            this.table.AddColumn("URL");

            return MarkupEmu("EMU can fix these problems:");
        }

        public override object FormatRecord(OperationInfo record)
        {
            var problem = record.Problem;

            this.table.AddRow(
                problem.Id,
                problem.Title,
                MarkupBool(record.Fixable),
                MarkupBool(record.Safe),
                MarkupLink(problem.Url));

            return null;
        }

        public override object FormatFooter(OperationInfo record)
        {
            return this.table;
        }

        public override string FormatCompact(OperationInfo record)
        {
            var problem = record.Problem;
            return $"{problem.Id}\tfixable={record.Fixable}\tsafe={record.Safe}\tautomatic-fix={record.Automatic}\t{problem.Title}";
        }
    }
}
