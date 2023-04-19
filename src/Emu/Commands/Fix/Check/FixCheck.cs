// <copyright file="FixCheck.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Extensions.System;
    using Emu.Fixes;
    using Emu.Utilities;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using MoreLinq.Extensions;
    using Spectre.Console;
    using static Emu.Cli.SpectreUtils;
    using static Emu.Fixes.CheckStatus;

    public class FixCheck : EmuCommandHandler<FixCheck.FixCheckResult>
    {
        private const string TotalsRow = "Totals";

        private readonly ILogger<FixCheck> logger;
        private readonly FileMatcher fileMatcher;
        private readonly FixRegister register;
        private readonly IFileSystem fileSystem;

        public FixCheck(ILogger<FixCheck> logger, FileMatcher fileMatcher, FixRegister register, OutputRecordWriter writer, IFileSystem fileSystem)
        {
            this.logger = logger;
            this.fileMatcher = fileMatcher;
            this.register = register;
            this.fileSystem = fileSystem;
            this.Writer = writer;
        }

        public string[] Targets { get; set; }

        public string[] Fix { get; set; }

        public bool All { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            // resolve fixes
            ICheckOperation[] fixes = null;
            if (this.All)
            {
                fixes = this.register.ResolveAllChecks().ToArray();
            }
            else
            {
                if (this.Fix is null or { Length: 0 })
                {
                    throw new Exception("A fix argument must be provided");
                }

                fixes = this.Fix.Select(x => this.register.ResolveCheck(x)).ToArray();
            }

            this.logger.LogDebug("Input targets: {0}", this.Targets);

            this.WriteMessage("Looking for targets...");
            this.WriteHeader();

            var files = this.fileMatcher.ExpandMatches(
                this.fileSystem.Directory.GetCurrentDirectory(),
                this.Targets);

            bool any = false;
            Map<string, Map<CheckStatus, int>> stats = default;
            int count = 0;
            foreach (var (_, file) in files)
            {
                any = true;
                var results = new Dictionary<WellKnownProblem, CheckResult>(fixes.Length);
                foreach (var fix in fixes)
                {
                    var fixMetadata = fix.GetOperationInfo();
                    this.logger.LogDebug("Checking {path} with {fixer}", file, fixMetadata.Problem.Id);
                    var result = await fix.CheckAffectedAsync(file);
                    results[fixMetadata.Problem] = result;

                    stats = stats
                        .AddOrUpdate(fixMetadata.Problem.Id, result.Status, Some, None)
                        .AddOrUpdate(TotalsRow, result.Status, Some, None);
                }

                count++;

                this.Write(new FixCheckResult(file, results));
            }

            this.WriteFooter();

            this.WriteMessage(MarkupRule($"Summary for {MarkupNumber(count.ToString())} files"));
            this.WriteMessage(this.CreateSummaryTable(stats));

            if (!any)
            {
                this.WriteMessage($"No files matched targets: {this.Targets.FormatInlineList()}");
            }

            return ExitCodes.Success;

            static int Some(int previous) => previous + 1;
            static int None() => 1;
        }

        public override string FormatCompact(FixCheck.FixCheckResult record)
        {
            var f = record;
            var summary = string.Join(' ', f.Problems.Select(kvp => kvp.Key.Id + "=" + this.Status(kvp.Value)));
            return $"{f.File}\t{summary}";
        }

        public override object FormatRecord(FixCheck.FixCheckResult record)
        {
            var f = record;

            StringBuilder builder = new();
            builder.AppendFormat(MarkupFileSection(f.File));
            foreach (var report in f.Problems)
            {
                builder.AppendFormat("\t- {0}: {1}. {2}\n", report.Key.Id, report.Value.Status, report.Value.Message.EscapeMarkup());
            }

            return builder.ToString();
        }

        public Table CreateSummaryTable(Map<string, Map<CheckStatus, int>> stats)
        {
            if (stats.Count == 0)
            {
                return null;
            }

            var totals = stats[TotalsRow];
            stats = stats.Remove(TotalsRow);

            var table = new Table();
            table.AddColumn("ID", x => x.Footer(TotalsRow));
            table.AddColumn(nameof(Affected), Footer(Affected));
            table.AddColumn(nameof(Unaffected), Footer(Unaffected));
            table.AddColumn(nameof(NotApplicable), Footer(NotApplicable));
            table.AddColumn(nameof(Repaired), Footer(Repaired));
            table.AddColumn(nameof(Error), Footer(Error));

            foreach (var (key, value) in stats)
            {
                table.AddRow(
                    key,
                    FormatValue(value, Affected),
                    FormatValue(value, Unaffected),
                    FormatValue(value, NotApplicable),
                    FormatValue(value, Repaired),
                    FormatValue(value, Error));
            }

            return table;

            string FormatValue(Map<CheckStatus, int> map, CheckStatus status)
                => MarkupNumber(map.Find(status).IfNone(0).ToString());
            Action<TableColumn> Footer(CheckStatus status) => (TableColumn column) => column.Footer(
                FormatValue(totals, status));
        }

        private string Status(CheckResult result) => result.Status switch
        {
            Affected => "BAD",
            Error => "ERR",
            NotApplicable => "NA",
            Unaffected => "GOOD",
            _ => "?",
        };

        public partial record FixCheckResult(string File, Dictionary<WellKnownProblem, CheckResult> Problems);
    }
}
