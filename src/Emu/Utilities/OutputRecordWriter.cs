// <copyright file="OutputRecordWriter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Utilities
{
    using System;
    using System.IO;
    using Emu.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using static Emu.EmuCommand;

    /// <summary>
    /// Writes record results to an output.
    /// </summary>
    public class OutputRecordWriter : IDisposable
    {
        private readonly TextWriter sink;
        private readonly IRecordFormatter formatter;
        private IDisposable formatterContext;
        private bool isDisposed;

        public OutputRecordWriter(TextWriter sink, IRecordFormatter formatter)
        {
            this.sink = sink;
            this.formatter = formatter;
            this.formatter.Writer = this.sink;
        }

        public static Func<IServiceProvider, IRecordFormatter> FormatterResolver { get; set; } =
            (provider) =>
            {
                var options = provider.GetRequiredService<Lazy<OutputFormat>>();
                return options.Value switch
                {
                    OutputFormat.Default => provider.GetRequiredService<AnsiConsoleFormatter>(),
                    OutputFormat.Compact => provider.GetRequiredService<ToStringFormatter>(),
                    OutputFormat.JSON => provider.GetRequiredService<JsonSerializer>(),
                    OutputFormat.JSONL => provider.GetRequiredService<JsonLinesSerializer>(),
                    OutputFormat.CSV => provider.GetRequiredService<CsvSerializer>(),
                    _ => throw new InvalidOperationException(),
                };
            };

        public void WriteHeader<T>(T header)
        {
            this.formatterContext = this.formatter.WriteHeader<T>(this.formatterContext, header);
        }

        /// <summary>
        /// Serializes a single record to the output sink and
        /// if needed also writes a header.
        /// </summary>
        public void Write<T>(T record)
        {
            if (this.formatterContext == null)
            {
                // TODO: possible race condition
                if (record is string)
                {
                    throw new InvalidOperationException("Write header needs to be called first");
                }

                this.WriteHeader(record);
            }

            this.formatterContext = this.formatter.WriteRecord(this.formatterContext, record);
        }

        public void WriteMessage<T>(T message)
        {
            this.formatterContext = this.formatter.WriteMessage(this.formatterContext, message);
        }

        public void WriteFooter<T>(T footer)
        {
            this.formatterContext = this.formatter.WriteFooter<T>(this.formatterContext, footer);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            // don't write a footer if a header was never written
            if (this.formatterContext != null)
            {
                this.formatter.Dispose(this.formatterContext);
                this.formatterContext.Dispose();
            }

            this.sink?.Dispose();

            this.isDisposed = true;
        }
    }
}
