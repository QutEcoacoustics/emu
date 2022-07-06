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
    using Microsoft.Extensions.Logging;

    public class FixCheck : EmuCommandHandler<FixCheck.FixCheckResult>
    {
        private readonly IConsole console;
        private readonly ILogger<FixCheck> logger;
        private readonly FileMatcher fileMatcher;
        private readonly FixRegister register;
        private readonly IFileSystem fileSystem;

        public FixCheck(IConsole console, ILogger<FixCheck> logger, FileMatcher fileMatcher, FixRegister register, OutputRecordWriter writer, IFileSystem fileSystem)
        {
            this.console = console;
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

            this.WriteFooter();

            if (!any)
            {
                this.WriteMessage($"No files matched targets: {this.Targets.FormatInlineList()}");
            }

            return ExitCodes.Success;
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
            builder.AppendFormat("File {0}:\n", f.File);
            foreach (var report in f.Problems)
            {
                builder.AppendFormat("\t- {0}: {1}. {2}\n", report.Key.Id, report.Value.Status, report.Value.Message);
            }

            return builder.ToString();
        }

        private string Status(CheckResult result) => result.Status switch
        {
            CheckStatus.Affected => "BAD",
            CheckStatus.Error => "ERR",
            CheckStatus.NotApplicable => "NA",
            CheckStatus.Unaffected => "GOOD",
            _ => "?",
        };

        public partial record FixCheckResult(string File, Dictionary<WellKnownProblem, CheckResult> Problems);
    }
}
