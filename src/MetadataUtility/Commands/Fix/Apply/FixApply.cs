// <copyright file="FixApply.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine.Invocation;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using LanguageExt;
    using MetadataUtility.Cli;
    using MetadataUtility.Extensions.System;
    using MetadataUtility.Fixes;
    using MetadataUtility.Utilities;
    using Microsoft.Extensions.Logging;

    public class FixApply : EmuCommandHandler
    {
        private readonly ILogger<FixApply> logger;
        private readonly ILogger<DryRun> dryRunLogger;
        private readonly FileMatcher fileMatcher;
        private readonly FixRegister register;

        public FixApply(ILogger<FixApply> logger, ILogger<DryRun> dryRunLogger, FileMatcher fileMatcher, FixRegister register, OutputRecordWriter writer)
        {
            this.logger = logger;
            this.dryRunLogger = dryRunLogger;
            this.fileMatcher = fileMatcher;
            this.register = register;
            this.Writer = writer;
        }

        public string[] Targets { get; set; }

        public string[] Fix { get; set; }

        public bool DryRun { get; set; }

        public bool Backup { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            // resolve fixes
            IFixOperation[] fixes = null;
            if (this.Fix is null or { Length: 0 })
            {
                throw new Exception("A fix argument must be provided");
            }

            fixes = this.Fix.Select(x => this.register.Resolve(x)).ToArray();

            this.logger.LogDebug("Input targets: {targets}", this.Targets);

            var files = this.fileMatcher.ExpandMatches(Directory.GetCurrentDirectory(), this.Targets);

            this.WriteHeader<FixResult>();
            this.Write("Looking for targets...");

            using var dryRun = new DryRun(this.DryRun, this.dryRunLogger);

            bool any = false;
            foreach (var (_, file) in files)
            {
                any = true;
                await this.ProcessFile(file, dryRun, fixes);
            }

            if (!any)
            {
                this.Write($"No files matched targets: {this.Targets.FormatInlineList()}");
            }

            return ExitCodes.Success;
        }

        public async ValueTask ProcessFile(string file, DryRun dryRun, IFixOperation[] fixes)
        {
            var results = new Dictionary<WellKnownProblem, FixResult>(fixes.Length);
            foreach (var fix in fixes)
            {
                var fixMetadata = fix.GetOperationInfo();
                this.logger.LogDebug("Fixing {path} with {fixer}", file, fixMetadata.Problem.Id);
                var result = await fix.ProcessFileAsync(file, dryRun, this.Backup);

                results[fixMetadata.Problem] = result;
            }

            this.Write(new FixApplyResult(file, results));
        }

        public partial record FixApplyResult(string File, Dictionary<WellKnownProblem, FixResult> Problems);

        protected override object FormatCompact<T>(T record)
        {
            if (record is FixApplyResult f)
            {
                var summary = string.Join(
                    ' ',
                    f.Problems.Select(kvp => kvp.Key.Id + "=" + Status(kvp.Value)));
                return $"{f.File}\t{summary}";
            }
            else if (record is null)
            {
                return record;
            }
            else if (record is string)
            {
                // suppress output
                return null;
            }

            return ThrowUnsupported(record);
        }

        protected override object FormatDefault<T>(T record)
        {
            if (record is FixApplyResult f)
            {
                StringBuilder builder = new();
                builder.AppendFormat("File {0}:\n", f.File);
                foreach (var report in f.Problems)
                {
                    builder.AppendFormat("\t- {0}: {1}. {2}\n", report.Key.Id, report.Value.Status, report.Value.Message);
                }

                return builder.ToString();
            }
            else if (record is null)
            {
                return record;
            }
            else if (record is string s)
            {
                return s;
            }

            return ThrowUnsupported(record);
        }

        private static string Status(FixResult result) => result.Status switch
        {
            FixStatus.NotFixed => "ERR",
            FixStatus.Fixed => "FIXED",
            FixStatus.NoOperation => "NOOP",
            _ => "?",
        };
    }
}
