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
    using Emu.Extensions.System;
    using Emu.Fixes;
    using Emu.Utilities;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    using static Emu.Cli.SpectreUtils;
    using static Emu.Utilities.DryRun;
    using static LanguageExt.Prelude;

    public class FixApply : EmuCommandHandler<Emu.FixApply.FixApplyResult>
    {
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
                throw new Exception("A fix argument must be provided");
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
            foreach (var (_, file) in files)
            {
                any = true;
                await this.ProcessFile(file, dryRun, fixes);
            }

            if (!any)
            {
                this.WriteMessage($"No files matched targets: {this.Targets.FormatInlineList()}");
            }

            return ExitCodes.Success;
        }

        public async ValueTask ProcessFile(string file, DryRun dryRun, ICheckOperation[] operations)
        {
            async Task<OpAndCheck> Check(ICheckOperation operation) => new(operation, await operation.CheckAffectedAsync(file));
            var checkResults = await operations.SequenceParallel(Check);

            // first check for errors
            var errors = checkResults.Exists(pair => pair.CheckResult.Status == CheckStatus.Error);
            if (errors)
            {
                this.logger.LogWarning("Errors encountered while checking file: {file}", file);

                this.Write(new FixApplyResult(file, AllNoop(checkResults)));

                // early exit!
                return;
            }

            // then check if any are affected
            var affected = checkResults.Exists(pair => pair.CheckResult.Status == CheckStatus.Affected);
            if (!affected)
            {
                this.logger.LogDebug("No problems found for file: {file}", file);

                this.Write(new FixApplyResult(file, AllNoop(checkResults)));

                // early exit!
                return;
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
                if (this.HasFileBeenRenamed(file))
                {
                    renamed = false;
                    message = "Already has been renamed as an error file";
                    newPath = file;
                }
                else
                {
                    var result = this.ApplyRename(file, dryRun, firstError.Operation);
                    renamed = result.IsSome;
                    newPath = result.IfNone(file);

                    message = renamed ? ("Renamed to: " + newPath) : null;
                }

                var fixResults = AllNoop(rest);
                fixResults.Add(
                    firstError.Operation.GetOperationInfo().Problem,
                    new FixResult(
                        renamed ? FixStatus.Renamed : FixStatus.NotFixed,
                        firstError.CheckResult,
                        message));

                this.Write(new FixApplyResult(newPath, fixResults));

                // early exit!
                return;
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
                    file = result.NewPath;
                    Debug.Assert(this.fileSystem.File.Exists(file), "sanity check that we're still pointing to a real file");
                }

                results.Add(operation.GetOperationInfo().Problem, result);
            }

            this.Write(new FixApplyResult(file, results, backup));
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

            builder.AppendFormat("File {0}:\n", MarkupPath(f.File));
            if (f.BackupFile != null)
            {
                builder.AppendFormat("\tBacked up to {0}", f.BackupFile.EscapeMarkup());
            }

            foreach (var report in f.Problems)
            {
                builder.AppendFormat("\t- {0} is ", report.Key.Id);
                builder.AppendFormat("{0} {1}.\n", report.Value.CheckResult.Status, report.Value.CheckResult.Message.EscapeMarkup());
                builder.AppendFormat("\t  Action taken: {0}. {1}\n", report.Value.Status, report.Value.Message.EscapeMarkup());
            }

            return builder.ToString();
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
                var info = operation.GetOperationInfo();
                var suffix = info.Suffix.IfNone(info.Problem.Id);
                var basename = this.fileSystem.Path.GetFileName(file);
                var newName = $"{basename}.error_{suffix}";
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
