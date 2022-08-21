// <copyright file="CheckStatus.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes
{
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
}
