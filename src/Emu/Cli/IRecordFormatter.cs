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
        /// Gets or sets the TextWriter to write to.
        /// </summary>
        /// <value>the TextWriter to write to.</value>
        TextWriter Writer { get; set; }

        /// <summary>
        /// For streaming operations, write a prelude or header if necessary.
        /// </summary>
        /// <typeparam name="T">The type that will be serialized.</typeparam>
        /// <param name="context">A generic object that is preserved between invocations.</param>
        /// <param name="record">The object to serialize headers for.</param>
        /// <returns>Shared context.</returns>
        IDisposable WriteHeader<T>(IDisposable context, T record);

        /// <summary>
        /// For streaming operations, write a single record.
        /// </summary>
        /// <typeparam name="T">The type that will be serialized.</typeparam>
        /// <param name="context">The shared context.</param>
        /// <param name="record">The object to serialize.</param>
        /// <returns>Shared context.</returns>
        IDisposable WriteRecord<T>(IDisposable context, T record);

        /// <summary>
        /// For user experience, write something we expect a user to read interactively.
        /// Most formatters will should discard this call.
        /// </summary>
        /// <typeparam name="T">The type that will be serialized.</typeparam>
        /// <param name="context">The shared context.</param>
        /// <param name="message">The message to serialize.</param>
        /// <returns>Shared context.</returns>
        IDisposable WriteMessage<T>(IDisposable context, T message);

        /// <summary>
        /// For streaming operations, write a postlude or footer if necessary.
        /// </summary>
        /// <param name="context">The shared context.</param>
        /// <param name="record">The object to serialize.</param>
        /// <returns>Shared context.</returns>
        IDisposable WriteFooter<T>(IDisposable context, T record);

        /// <summary>
        /// For streaming operations, write a postlude or footer if necessary.
        /// </summary>
        /// <param name="context">The shared context.</param>
        void Dispose(IDisposable context);
    }
}
