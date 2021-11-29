// <copyright file="PowerFailure.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Fixes.FrontierLabs
{
    using System.Threading.Tasks;

    public class PowerFailure : ICheckOperation
    {
        public static OperationInfo Metadata => new(
            WellKnownProblems.FrontierLabs.StubFile,
            Fixable: false,
            Safe: true,
            Automatic: true,
            typeof(PowerFailure));

        public async Task<CheckResult> CheckAffectedAsync(string file)
        {
            using var stream = File.OpenRead(file);

            // we'll use a couple metrics here
            // is it a stub recording?
            return (await Audio.Vendors.FrontierLabs.IsDefaultStubRecording(stream)) switch
            {
                true => new CheckResult(CheckStatus.Affected, Severity.Severe, "The file empty and there is no usable data"),
                false => new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty),
            };
        }

        public OperationInfo GetOperationInfo() => Metadata;
    }
}
