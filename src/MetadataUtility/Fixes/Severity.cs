// <copyright file="Severity.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Fixes
{
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
}
