// <copyright file="INotice.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
    /// <summary>
    /// Represents a notice associated with an audio recording.
    /// </summary>
    public interface INotice
    {
        /// <summary>
        /// Gets the problem that generated this notice.
        /// </summary>
        WellKnownProblem Problem { get; }
    }
}
