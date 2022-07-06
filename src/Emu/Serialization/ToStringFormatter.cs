// <copyright file="ToStringFormatter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Serialization
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

        public TextWriter Writer { get; set; }

        /// <inheritdoc />
        public IDisposable WriteHeader<T>(IDisposable context, T record)
        {
            if (record is not null)
            {
                this.Writer.WriteLine(record is string s ? s : record.ToString());
            }

            return context ?? new DummyContext();
        }

        /// <inheritdoc />
        public virtual IDisposable WriteRecord<T>(IDisposable context, T record)
        {
            this.Writer.WriteLine(record.ToString());

            return context;
        }

        /// <inheritdoc />
        public virtual IDisposable WriteMessage<T>(IDisposable context, T message)
        {
            // noop

            return context;
        }

        /// <inheritdoc />
        public IDisposable WriteFooter<T>(IDisposable context, T record)
        {
            if (record is not null)
            {
                this.Writer.WriteLine(record is string s ? s : record.ToString());
            }

            return context;
        }

        /// <inheritdoc />
        public void Dispose(IDisposable context)
        {
            // noop
        }
    }
}
