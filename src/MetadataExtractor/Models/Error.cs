// <copyright file="Error.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataExtractor.Models
{
    /// <inheritdoc />
    public class Error : INotice
    {
        /// <inheritdoc />
        public string Title { get; set; }

        /// <inheritdoc/>
        public string Message { get; set; }

        /// <inheritdoc/>
        public string Code { get; set; }
    }
}
