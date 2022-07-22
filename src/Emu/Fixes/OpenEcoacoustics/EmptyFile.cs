// <copyright file="EmptyFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.OpenEcoacoustics
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;

    public class EmptyFile : ICheckOperation
    {
        private readonly IFileSystem fileSystem;

        public EmptyFile(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public static OperationInfo Metadata => new(
            WellKnownProblems.OpenEcoacousticsProblems.EmptyFile,
            Fixable: false,
            Safe: true,
            Automatic: false,
            typeof(EmptyFile),
            Suffix: "empty");

        public Task<CheckResult> CheckAffectedAsync(string file)
        {
            var info = this.fileSystem.FileInfo.FromFileName(file);

            var result = info.Length switch
            {
                0 => new CheckResult(CheckStatus.Affected, Severity.Severe, this.GetOperationInfo().Problem.Message),
                _ => new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty),

            };

            return Task.FromResult(result);
        }

        public OperationInfo GetOperationInfo() => Metadata;
    }
}
