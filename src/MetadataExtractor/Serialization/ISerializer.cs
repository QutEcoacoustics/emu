// <copyright file="ISerializer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataExtractor.Serialization
{
    using System.Collections.Generic;
    using System.IO;
    using MetadataExtractor.Models;

    /// <summary>
    /// Serialization for EMU <see cref="Recording"/> instances.
    /// </summary>
    public interface ISerializer
    {

        /// <summary>
        /// Convert the given <see cref="Recording"/> to a string.
        /// </summary>
        /// <param name="recordings">The recording to convert.</param>
        /// <returns>A string representation of the recording.</returns>
        string Serialize(IEnumerable<Recording> recordings);


        /// <summary>
        /// Convert the given <see cref="Recording"/> to a string.
        /// </summary>
        /// <param name="writer">The text stream to write the result to.</param>
        /// <param name="recording">The recording to convert.</param>
        void Serialize(TextWriter writer, IEnumerable<Recording> recording);
    }
}
