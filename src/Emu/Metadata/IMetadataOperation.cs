// <copyright file="IMetadataOperation.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata
{
    using Emu.Models;

    /// <summary>
    /// Used to extract metadata from a target in order to populate a recording object.
    /// </summary>
    public interface IMetadataOperation
    {
        /// <summary>
        /// Gets the name of this extractor - mainly used for logging at this point.
        /// Also used to print out names of sections in the metadata dump command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Checks if this metadata extractor is applicable to the given target.
        /// </summary>
        /// <param name="information">Information about the current target.</param>
        /// <returns>True if this metadata extractor can operate on the current target.</returns>
        ValueTask<bool> CanProcessAsync(TargetInformation information);

        /// <summary>
        /// Extracts the metadata from the given target.
        /// </summary>
        /// <param name="information">Information about the current target.</param>
        /// <param name="recording">The current information extracted from the target.</param>
        /// <returns>An updated recording with the newly extracted metadata.</returns>
        ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording);

        // todo: encode success/failure of the operation?
    }
}
