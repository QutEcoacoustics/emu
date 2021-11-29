// <copyright file="IFixOperation.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Fixes
{
    using System.Threading.Tasks;
    using MetadataUtility.Utilities;

    public interface ICheckOperation
    {
        static abstract OperationInfo Metadata { get; }

        Task<CheckResult> CheckAffectedAsync(string file);

        OperationInfo GetOperationInfo();
    }

    public interface IFixOperation : ICheckOperation
    {
        Task<FixResult> ProcessFileAsync(string file, DryRun dryRun, bool backup);
    }

    public enum CheckStatus
    {
        /// <summary>
        /// The target is affected by the problem.
        /// </summary>
        Affected,

        /// <summary>
        /// The target is not affected by the problem.
        /// </summary>
        Unaffected,

        /// <summary>
        /// The problem is not applicable to this target.
        /// </summary>
        NotApplicable,

        /// <summary>
        /// The target was once affected by the problem but has since been repaired.
        /// </summary>
        Repaired,

        /// <summary>
        /// Some error occurred while checking if the target was affected by the problem.
        /// </summary>
        Error,
    }

    public enum FixStatus
    {
        NoOperation,
        Fixed,
        NotFixed
    }

    public enum Severity
    {
        /// <summary>
        /// Not affected by the problem.
        /// </summary>
        None,

        /// <summary>
        /// Affected by the problem but the file is likely still useful.
        /// </summary>
        Mild,

        /// <summary>
        /// Affected by the problem and the file is only usable after repair or with tolerant tools.
        /// </summary>
        Moderate,

        /// <summary>
        /// The file is affected by the problem and is either corrrupt or has no usable data.
        /// </summary>
        Severe,
    }

    public record CheckResult(CheckStatus Status, Severity Severity, string Message, object Data = default);
    public record FixResult(FixStatus Status, CheckResult CheckResult, string Message);
}
