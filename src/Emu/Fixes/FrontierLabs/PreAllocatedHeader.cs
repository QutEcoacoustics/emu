// <copyright file="PreAllocatedHeader.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.FrontierLabs
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using static Emu.Audio.Vendors.FrontierLabs;

    public class PreAllocatedHeader : ICheckOperation
    {
        public const string Message = "The file is a stub and has no usable data";
        private readonly IFileSystem fileSystem;

        public PreAllocatedHeader(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public static OperationInfo Metadata => new(
            WellKnownProblems.FrontierLabsProblems.PreAllocatedHeader,
            Fixable: false,
            Safe: true,
            Automatic: false,
            typeof(PreAllocatedHeader),
            Suffix: "stub");

        public Task<CheckResult> CheckAffectedAsync(string file)
        {
            using var stream = (FileStream)this.fileSystem.File.OpenRead(file);

            var result = IsPreallocatedHeader(stream, file) switch
            {
                true => new CheckResult(CheckStatus.Affected, Severity.Severe, Message),
                false when stream.Length == 0 => new CheckResult(CheckStatus.NotApplicable, Severity.None, string.Empty),
                false => new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty),

            };

            return Task.FromResult(result);
        }

        public OperationInfo GetOperationInfo() => Metadata;
    }
}
