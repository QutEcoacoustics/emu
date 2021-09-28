// <copyright file="ToStringFormatter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Serialization
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc cref="IRecordFormatter"/>
    public class ToStringFormatter : IRecordFormatter
    {
        private readonly ILogger<ToStringFormatter> logger;

        public ToStringFormatter(ILogger<ToStringFormatter> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public IDisposable WriteHeader<T>(IDisposable context, TextWriter writer, T? record)
        {
            if (record is not null)
            {
                writer.WriteLine(record is string s ? s : record.ToString());
            }

            return context ?? new DummyContext();
        }

        /// <inheritdoc />
        public virtual IDisposable WriteRecord<T>(IDisposable context, TextWriter writer, T record)
        {
            writer.WriteLine(record.ToString());

            return context;
        }

        /// <inheritdoc />
        public IDisposable WriteFooter<T>(IDisposable context, TextWriter writer, T record)
        {
            if (record is not null)
            {
                writer.WriteLine(record is string s ? s : record.ToString());
            }

            return context;
        }

        /// <inheritdoc />
        public void Dispose(IDisposable context, TextWriter writer)
        {
            // noop
        }
    }
}
