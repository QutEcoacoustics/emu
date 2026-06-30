// <copyright file="NullFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.OpenEcoacoustics
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Emu.Utilities;

    /// <summary>
    /// Practically identical to WildlifeAcoustics.NoData but a little more general
    /// - it is not gated on file size.
    /// </summary>
    public class NullFile : ICheckOperation
    {
        private readonly IFileSystem fileSystem;
        private readonly FileUtilities fileUtilities;

        public NullFile(IFileSystem fileSystem, FileUtilities fileUtilities)
        {
            this.fileSystem = fileSystem;
            this.fileUtilities = fileUtilities;
        }

        public static OperationInfo Metadata => new(
            WellKnownProblems.OpenEcoacousticsProblems.NullFile,
            Fixable: false,
            Safe: true,
            Automatic: false,
            typeof(NullFile),

            // most people don't care about the distinction between OE004 and OE005
            // they're both a type of empty file
            Suffix: "empty");

        public async Task<CheckResult> CheckAffectedAsync(string file)
        {
            var info = this.fileSystem.FileInfo.New(file);

            if (info.Length == 0)
            {
                return new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty);
            }

            // WA0002 (WildlifeAcoustics.NoData) handles the specific 131072-byte all-zeros case
            if (info.Length == WildlifeAcoustics.NoData.FaultFileSize)
            {
                return new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty);
            }

            var reader = this.fileSystem.File.OpenRead(file);

            var allEmpty = await this.fileUtilities.CheckForContinuousValue(reader);

            if (allEmpty)
            {
                return new CheckResult(CheckStatus.Affected, Severity.Severe, this.GetOperationInfo().Problem.Message);
            }

            return new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty);
        }

        public OperationInfo GetOperationInfo() => Metadata;
    }
}
