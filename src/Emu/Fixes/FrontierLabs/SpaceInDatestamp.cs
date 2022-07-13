// <copyright file="SpaceInDatestamp.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.FrontierLabs
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Emu.Utilities;

    public class SpaceInDatestamp : IFixOperation
    {
        public const string ReplaceString = "${1}0${3}";
        public static readonly Regex Matcher = new("(.*\\d{6})( )(\\dT\\d{6}.*)");

        private readonly IFileSystem fileSystem;
        private readonly FileUtilities fileUtilities;

        public SpaceInDatestamp(IFileSystem fileSystem, FileUtilities fileUtilities)
        {
            this.fileSystem = fileSystem;
            this.fileUtilities = fileUtilities;
        }

        public static OperationInfo Metadata => new(
            WellKnownProblems.FrontierLabsProblems.InvalidDateStampSpaceZero,
            Fixable: true,
            Safe: true,
            Automatic: true,
            typeof(SpaceInDatestamp));

        public Task<CheckResult> CheckAffectedAsync(string file)
        {
            var fileInfo = this.fileSystem.FileInfo.FromFileName(file);
            var result = Matcher.IsMatch(fileInfo.Name) switch {
                true => new CheckResult(CheckStatus.Affected, Severity.Mild, "Space in datestamp detected"),
                false => new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty),
            };

            return Task.FromResult(result);
        }

        public OperationInfo GetOperationInfo() => Metadata;

        public async Task<FixResult> ProcessFileAsync(string file, DryRun dryRun)
        {
            var affected = await this.CheckAffectedAsync(file);

            if (affected is { Status: CheckStatus.Affected })
            {
                var fileInfo = this.fileSystem.FileInfo.FromFileName(file);
                var newBasename = Matcher.Replace(fileInfo.Name, ReplaceString);
                var newName = this.fileUtilities.Rename(file, newBasename, dryRun);

                return new FixResult(FixStatus.Fixed, affected, "Inserted `0` into datestamp", newName);
            }
            else
            {
                return new FixResult(FixStatus.NoOperation, affected, affected.Message);
            }
        }
    }
}
