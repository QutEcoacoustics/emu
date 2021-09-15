// <copyright file="ISerializer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Serialization
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
        /// For streaming operations, write a prelude or header if necessary.
        /// </summary>
        /// <typeparam name="T">The type that will be serialized.</typeparam>
        /// <param name="writer">The text writer to write to.</param>
        /// <returns>Shared context.</returns>
        //IDisposable WriteHeader<T>(TextWriter writer);

        /// <summary>
        /// For streaming operations, write a single record.
        /// </summary>
        /// <typeparam name="T">The type that will be serialized.</typeparam>
        /// <param name="context">The shared context.</param>
        /// <param name="writer">The text writer to write to.</param>
        /// <param name="record">The object to serialize.</param>
        /// <returns>Shared context.</returns>
        //IDisposable WriteRecord<T>(IDisposable context, TextWriter writer, T record);

        /// <summary>
        /// For streaming operations, write a postlude or footer if necessary.
        /// </summary>
        /// <typeparam name="T">The type that will be serialized.</typeparam>
        /// <param name="context">The shared context.</param>
        /// <param name="writer">The text writer to write to.</param>
        /// <returns>Shared context.</returns>
        //IDisposable WriteFooter<T>(IDisposable context, TextWriter writer);

        /// <summary>
        /// Convert the text to objects.
        /// </summary>
        IEnumerable<T> Deserialize<T>(TextReader reader);
    }
}
