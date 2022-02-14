// <copyright file="ICheckOperation.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Fixes
{
    using System.Threading.Tasks;

    public interface ICheckOperation
    {
        static abstract OperationInfo Metadata { get; }

        Task<CheckResult> CheckAffectedAsync(string file);

        OperationInfo GetOperationInfo();
    }

    public record CheckResult(CheckStatus Status, Severity Severity, string Message, object Data = default);
}
