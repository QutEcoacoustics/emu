// <copyright file="EmuCommandHandler.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System;
    using System.CommandLine.Invocation;
    using System.Threading.Tasks;
    using Emu.Utilities;
    using LanguageExt;
    using static Emu.EmuCommand;

    public abstract class EmuCommandHandler<T> : EmuGlobalOptions, ICommandHandler
    {
        // Note to future self:
        //  Formatters massage output into the desired shape
        //  Serializers transform said shape to desired data format
        //
        // Typically our text formatters do heavy manipulation of the shape, and need no serialization.
        // Whereas our formal data types (csv, JSON, JSON-L) need no formatting, and need serialization.

        public EmuCommandHandler()
        {
            // watch out setting things here - CLI options have not yet been bound when constructor runs
        }

        public OutputRecordWriter Writer { get; init; }

        public abstract Task<int> InvokeAsync(InvocationContext context);

        public void WriteHeader()
        {
            if (this.Writer.OutputFormat is OutputFormat.Compact)
            {
                this.Writer.WriteHeader<T>(default);
            }
            else if (this.Writer.OutputFormat is OutputFormat.Default)
            {
                this.Writer.WriteHeader(this.FormatHeader(default));
            }
            else
            {
                this.Writer.WriteHeader<T>(default);
            }
        }

        public void Write(T record)
        {
            if (this.Writer.OutputFormat is OutputFormat.Compact)
            {
                this.Writer.Write(this.FormatCompact(record));
            }
            else if (this.Writer.OutputFormat is OutputFormat.Default)
            {
                this.Writer.Write(this.FormatRecord(record));
            }
            else
            {
                this.Writer.Write<T>(record);
            }
        }

        public void WriteFooter()
        {
            if (this.Writer.OutputFormat is OutputFormat.Compact)
            {
                this.Writer.WriteFooter<T>(default);
            }
            else if (this.Writer.OutputFormat is OutputFormat.Default)
            {
                this.Writer.WriteFooter(this.FormatFooter(default));
            }
            else
            {
                this.Writer.WriteFooter<T>(default);
            }
        }

        public void WriteMessage<TMessage>(TMessage message)
        {
            this.Writer.WriteMessage(message);
        }

        public virtual object FormatHeader(T record) => null;

        public abstract object FormatRecord(T record);

        public virtual object FormatFooter(T record) => null;

        public abstract string FormatCompact(T record);
    }
}
