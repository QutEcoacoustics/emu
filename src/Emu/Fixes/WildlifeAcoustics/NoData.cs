// <copyright file="NoData.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.WildlifeAcoustics
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Emu.Utilities;

    public class NoData : ICheckOperation
    {
        private const int FaultFileSize = 131_072;
        private readonly IFileSystem fileSystem;
        private readonly FileUtilities fileUtilities;

        public NoData(IFileSystem fileSystem, FileUtilities fileUtilities)
        {
            this.fileSystem = fileSystem;
            this.fileUtilities = fileUtilities;
        }

        public static OperationInfo Metadata { get; } = new(
            WellKnownProblems.WildlifeAcoustics.NoData,
            Fixable: false,
            Safe: true,
            Automatic: false,
            typeof(NoData),
            Suffix: "empty");

        public async Task<CheckResult> CheckAffectedAsync(string file)
        {
            using var reader = this.fileSystem.File.OpenRead(file);

            var hasLength = reader.Length == FaultFileSize;

            if (!hasLength)
            {
                return new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty);
            }

            var allEmpty = await this.fileUtilities.CheckForContinuousValue(reader);

            return allEmpty switch
            {
                true => new CheckResult(CheckStatus.Affected, Severity.Severe, "The file has only null bytes and has no usable data."),
                false => new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty),
            };
        }

        public OperationInfo GetOperationInfo() => Metadata;
    }
}
