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
        /// Gets or sets a short title for the notice.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets a detailed message for the notice.
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// Gets or sets a unique idnetifying code for the notice.
        /// </summary>
        /// <remarks>
        /// The code is used for well known problems and allows linking to
        /// an associated problem.
        /// </remarks>
        string Code { get; set; }
    }
}
