// <copyright file="IRawMetadataOperation.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata
{
    using Emu.Models.Notices;
    using LanguageExt;

    /// <summary>
    /// Used to extract low-level metadata from a target - the results are not meant to be composed.
    /// </summary>
    public interface IRawMetadataOperation : IMetadataOperation
    {
        /// <summary>
        /// Extracts the metadata from the given target.
        /// </summary>
        /// <param name="information">Information about the current target.</param>
        /// <returns>An object with data.</returns>
        ValueTask<MetadataExtractionResult> ProcessFileAsync(TargetInformation information);
    }

    public record MetadataExtractionResult(object Data, Seq<Notice> Notices)
    {
        public MetadataExtractionResult(object data)
            : this(data, Seq<Notice>.Empty)
        {
        }
    }
}
