// <copyright file="FixCheck.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MetadataUtility.Extensions.System;
    using MetadataUtility.Fixes;
    using MetadataUtility.Utilities;
    using Microsoft.Extensions.Logging;

    public class FixCheck : EmuCommandHandler
    {
        private readonly IConsole console;
        private readonly ILogger<FixCheck> logger;
        private readonly FileMatcher fileMatcher;
        private readonly FixRegister register;

        public FixCheck(IConsole console, ILogger<FixCheck> logger, FileMatcher fileMatcher, FixRegister register, OutputRecordWriter writer)
        {
            this.console = console;
            this.logger = logger;
            this.fileMatcher = fileMatcher;
            this.register = register;
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

            this.WriteHeader<FixCheckResult>();
            this.Write("Looking for targets...");

            var files = this.fileMatcher.ExpandMatches(Directory.GetCurrentDirectory(), this.Targets);

            bool any = false;
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
                }

                this.Write(new FixCheckResult(file, results));
            }

            if (!any)
            {
                this.Write($"No files matched targets: {this.Targets.FormatInlineList()}");
            }

            return 0;
        }

        public partial record FixCheckResult(string File, Dictionary<WellKnownProblem, CheckResult> Problems);

        protected override object FormatCompact<T>(T record)
        {
            if (record is FixCheckResult f)
            {
                var summary = string.Join(' ', f.Problems.Select(kvp => kvp.Key.Id + "=" + this.Status(kvp.Value)));
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
            if (record is FixCheckResult f)
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

        private string Status(CheckResult result) => result.Status switch
        {
            CheckStatus.Affected => "BAD",
            CheckStatus.Error => "ERR",
            CheckStatus.NotApplicable => "NA",
            CheckStatus.Unaffected => "GOOD",
            _ => "?",
        };
    }
}
