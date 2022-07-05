// <copyright file="IRecordFormatter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System;
    using System.IO;

    public interface IRecordFormatter
    {
        /// <summary>
        /// For streaming operations, write a prelude or header if necessary.
        /// </summary>
        /// <typeparam name="T">The type that will be serialized.</typeparam>
        /// <param name="context">A generic object that is preserved between invocations.</param>
        /// <param name="writer">The text writer to write to.</param>
        /// <param name="record">The object to serialize heasers for.</param>
        /// <returns>Shared context.</returns>
        IDisposable WriteHeader<T>(IDisposable context, TextWriter writer, T record);

        /// <summary>
        /// For streaming operations, write a single record.
        /// </summary>
        /// <typeparam name="T">The type that will be serialized.</typeparam>
        /// <param name="context">The shared context.</param>
        /// <param name="writer">The text writer to write to.</param>
        /// <param name="record">The object to serialize.</param>
        /// <returns>Shared context.</returns>
        IDisposable WriteRecord<T>(IDisposable context, TextWriter writer, T record);

        /// <summary>
        /// For streaming operations, write a postlude or footer if necessary.
        /// </summary>
        /// <param name="context">The shared context.</param>
        /// <param name="writer">The text writer to write to.</param>
        /// <param name="record">The object to serialize.</param>
        /// <returns>Shared context.</returns>
        IDisposable WriteFooter<T>(IDisposable context, TextWriter writer, T record);

        /// <summary>
        /// For streaming operations, write a postlude or footer if necessary.
        /// </summary>
        /// <param name="context">The shared context.</param>
        /// <param name="writer">The text writer to write to.</param>
        void Dispose(IDisposable context, TextWriter writer);
    }
}
