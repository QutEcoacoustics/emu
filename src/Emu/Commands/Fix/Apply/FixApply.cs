// <copyright file="FixApply.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Fixes;
    using Emu.Utilities;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    using static Emu.Cli.SpectreUtils;
    using static Emu.Fixes.FixStatus;
    using static Emu.Utilities.DryRun;
    using static LanguageExt.Prelude;

    public class FixApply : EmuCommandHandler<Emu.FixApply.FixApplyResult>
    {
        private const string TotalsRow = "Totals";

        private static readonly Regex ErrorSuffix = new("\\.error_\\w+$");

        private readonly ILogger<FixApply> logger;
        private readonly DryRunFactory dryRunFactory;
        private readonly FileMatcher fileMatcher;
        private readonly FixRegister register;
        private readonly IFileSystem fileSystem;
        private readonly FileUtilities fileUtils;

        public FixApply(
            ILogger<FixApply> logger,
            DryRunFactory dryRunFactory,
            FileMatcher fileMatcher,
            FixRegister register,
            OutputRecordWriter writer,
            IFileSystem fileSystem,
            FileUtilities fileUtils)
        {
            this.logger = logger;
            this.dryRunFactory = dryRunFactory;
            this.fileMatcher = fileMatcher;
            this.register = register;
            this.fileSystem = fileSystem;
            this.fileUtils = fileUtils;
            this.Writer = writer;
        }

        public string[] Targets { get; set; }

        public string[] Fix { get; set; }

        public bool DryRun { get; set; }

        public bool Backup { get; set; }

        public bool NoRename { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            // resolve fixes
            ICheckOperation[] fixes = null;
            if (this.Fix is null or { Length: 0 })
            {
                throw new ArgumentException("A fix argument must be provided");
            }

            fixes = this.Fix.Select(x => this.register.ResolveCheck(x)).ToArray();

            this.logger.LogDebug("Input targets: {targets}", this.Targets);

            var files = this.fileMatcher.ExpandMatches(
                this.fileSystem.Directory.GetCurrentDirectory(),
                this.Targets);

            this.WriteHeader();
            this.WriteMessage("Looking for targets...");

            using var dryRun = this.dryRunFactory(this.DryRun);

            bool any = false;
            Map<string, Map<FixStatus, int>> stats = default;
            int count = 0;
            foreach (var (_, file) in files)
            {
                any = true;
                var result = await this.ProcessFile(file, dryRun, fixes);

                this.Write(result);

                stats = result.Problems.Aggregate(
                    stats,
                    (stats, kvp) => stats
                        .AddOrUpdate(kvp.Key.Id, kvp.Value.Status, Some, None)
                        .AddOrUpdate(TotalsRow, kvp.Value.Status, Some, None));

                count++;
            }

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

        public async ValueTask<FixApplyResult> ProcessFile(string file, DryRun dryRun, ICheckOperation[] operations)
        {
            async Task<OpAndCheck> Check(ICheckOperation operation) => new(operation, await operation.CheckAffectedAsync(file));
            var checkResults = await operations.SequenceParallel(Check);

            // first check for errors
            var errors = checkResults.Exists(pair => pair.CheckResult.Status == CheckStatus.Error);
            if (errors)
            {
                this.logger.LogWarning("Errors encountered while checking file: {file}", file);

                // early exit!
                return new FixApplyResult(file, AllNoop(checkResults));
            }

            // then check if any are affected
            var affected = checkResults.Exists(pair => pair.CheckResult.Status == CheckStatus.Affected);
            if (!affected)
            {
                this.logger.LogDebug("No problems found for file: {file}", file);

                // early exit!
                return new FixApplyResult(file, AllNoop(checkResults));
            }

            // check if any are not fixable
            if (checkResults.Any(IsNotFixable))
            {
                this.logger.LogDebug("Some problems are not fixable for file: {file}", file);
                var firstError = checkResults.First(IsNotFixable);
                var rest = checkResults.Except(firstError.AsEnumerable());

                Debug.Assert(firstError.CheckResult is { Status: CheckStatus.Affected }, "We should have found a problem that is affected");

                bool renamed;
                string message;
                string newPath;
                string reportedPath;
                if (this.HasFileBeenRenamed(file))
                {
                    renamed = false;
                    message = "Already has been renamed as an error file";
                    reportedPath = file;
                    newPath = null;
                }
                else
                {
                    var result = this.ApplyRename(file, dryRun, firstError.Operation);
                    renamed = result.IsSome;
                    newPath = result.IfNoneUnsafe((string)null);
                    reportedPath = result.IfNone(file);

                    message = renamed ? ("Renamed to: " + newPath) : null;
                }

                var fixResults = AllNoop(rest);
                fixResults.Add(
                    firstError.Operation.GetOperationInfo().Problem,
                    new FixResult(
                        renamed ? Renamed : NotFixed,
                        firstError.CheckResult,
                        message,
                        newPath));

                // early exit!
                return new FixApplyResult(reportedPath, fixResults);
            }

            // ok we now assume there is some mutation to do!
            // backup first!
            var backup = await this.BackupFileAsync(file, dryRun);

            // finally, apply the fixes
            var results = new Dictionary<WellKnownProblem, FixResult>(operations.Length);
            foreach (var (operation, checkResult) in checkResults)
            {
                if (operation is not IFixOperation)
                {
                    Debug.Assert(checkResult.Status is not CheckStatus.Affected, "Unfixable errors should have been dealt with already");

                    results.Add(operation.GetOperationInfo().Problem, new FixResult(FixStatus.NoOperation, checkResult, null));
                    continue;
                }

                var result = await this.ApplyFixAsync(file, dryRun, (IFixOperation)operation, checkResult);

                if (result.NewPath != null)
                {
                    file = dryRun.IsDryRun ? file : result.NewPath;
                    Debug.Assert(this.fileSystem.File.Exists(file), "sanity check that we're still pointing to a real file");
                }

                results.Add(operation.GetOperationInfo().Problem, result);
            }

            return new FixApplyResult(file, results, backup);
        }

        public override string FormatCompact(FixApplyResult record)
        {
            var summary = string.Join(
                ' ',
                record.Problems.Select(kvp => kvp.Key.Id + "=" + Status(kvp.Value)));
            return $"{record.File}\t{summary}";
        }

        public override object FormatRecord(FixApplyResult record)
        {
            var f = record;
            StringBuilder builder = new();

            builder.Append(MarkupFileSection(f.File));
            if (f.BackupFile != null)
            {
                builder.AppendFormat("\tBacked up to {0}\n", f.BackupFile.EscapeMarkup());
            }

            foreach (var report in f.Problems)
            {
                builder.AppendFormat("\t- {0} is ", report.Key.Id);
                builder.AppendFormat(
                    "{0}{2}{1}.\n",
                    report.Value.CheckResult.Status,
                    report.Value.CheckResult.Message.EscapeMarkup(),
                    string.IsNullOrEmpty(report.Value.CheckResult.Message) ? string.Empty : ": ");
                builder.AppendFormat("\t  Action taken: {0}. {1}\n", report.Value.Status, report.Value.Message.EscapeMarkup());
            }

            return builder.ToString();
        }

        public Table CreateSummaryTable(Map<string, Map<FixStatus, int>> stats)
        {
            if (stats.Count == 0)
            {
                return null;
            }

            var totals = stats[TotalsRow];
            stats = stats.Remove(TotalsRow);

            var table = new Table();
            table.AddColumn("ID", x => x.Footer(TotalsRow));
            table.AddColumn(nameof(NoOperation), Footer(NoOperation));
            table.AddColumn(nameof(Fixed), Footer(Fixed));
            table.AddColumn(nameof(NotFixed), Footer(NotFixed));
            table.AddColumn(nameof(Renamed), Footer(Renamed));

            foreach (var (key, value) in stats)
            {
                table.AddRow(
                    key,
                    FormatValue(value, NoOperation),
                    FormatValue(value, Fixed),
                    FormatValue(value, NotFixed),
                    FormatValue(value, Renamed));
            }

            return table;

            string FormatValue(Map<FixStatus, int> map, FixStatus status)
                => MarkupNumber(map.Find(status).IfNone(0).ToString());
            Action<TableColumn> Footer(FixStatus status) => (TableColumn column) => column.Footer(
                FormatValue(totals, status));
        }

        private static string Status(FixResult result) => result.Status switch
        {
            FixStatus.NotFixed => "ERR",
            FixStatus.Fixed => "FIXED",
            FixStatus.Renamed => "RENAMED",
            FixStatus.NoOperation => "NOOP",
            _ => "?",
        };

        private static bool IsNotFixable(OpAndCheck pair)
        {
            return !(pair.Operation is IFixOperation || pair.CheckResult.Severity <= Severity.Mild);
        }

        private static Dictionary<WellKnownProblem, FixResult> AllNoop(IEnumerable<OpAndCheck> checkResults)
        {
            return checkResults.ToDictionary(
                x => x.Operation.GetOperationInfo().Problem,
                x => new FixResult(FixStatus.NoOperation, x.CheckResult, null));
        }

        private Option<string> ApplyRename(string file, DryRun dryRun, ICheckOperation operation)
        {
            if (!this.NoRename)
            {
                var newName = operation.GetOperationInfo().GetErrorName(this.fileSystem, file);
                var dest = this.fileUtils.Rename(file, newName, dryRun);
                this.logger.LogDebug("File renamed up to {destination}", dest);
                return dest;
            }

            return None;
        }

        private async Task<string> BackupFileAsync(string file, DryRun dryRun)
        {
            if (this.Backup)
            {
                var backup = await this.fileUtils.BackupAsync(file, dryRun);
                this.logger.LogDebug("File backed up to {backup}", backup);
                return backup;
            }

            return null;
        }

        private async Task<FixResult> ApplyFixAsync(string file, DryRun dryRun, IFixOperation operation, CheckResult checkResult)
        {
            if (checkResult is { Status: CheckStatus.Affected })
            {
                this.logger.LogDebug("Fixing {path} with {fixer}", file, operation.GetOperationInfo().Problem.Id);
                return await operation.ProcessFileAsync(file, dryRun);
            }
            else
            {
                return new FixResult(FixStatus.NoOperation, checkResult, checkResult.Message);
            }
        }

        private bool HasFileBeenRenamed(string path) => ErrorSuffix.IsMatch(path);

        public partial record FixApplyResult(string File, IReadOnlyDictionary<WellKnownProblem, FixResult> Problems, string BackupFile = null);

        private record OpAndCheck(ICheckOperation Operation, CheckResult CheckResult);
    }
}
