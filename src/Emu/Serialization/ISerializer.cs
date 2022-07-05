// <copyright file="ISerializer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Serialization
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Serialization for EMU types.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Convert the given objects to a string.
        /// </summary>
        /// <param name="objects">The objects to convert.</param>
        /// <returns>A string representation of the recording.</returns>
        string Serialize<T>(IEnumerable<T> objects);

        /// <summary>
        /// Convert the given objects to a string.
        /// </summary>
        /// <param name="writer">The text stream to write the result to.</param>
        /// <param name="objects">The objects to convert.</param>
        void Serialize<T>(TextWriter writer, IEnumerable<T> objects);

        /// <summary>
        /// Convert the text to objects.
        /// </summary>
        IEnumerable<T> Deserialize<T>(TextReader reader);
    }
}
