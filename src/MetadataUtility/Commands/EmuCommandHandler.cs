// <copyright file="EmuCommandHandler.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine.Invocation;
    using System.Threading.Tasks;
    using MetadataUtility.Utilities;
    using static MetadataUtility.EmuCommand;

    public abstract class EmuCommandHandler : EmuGlobalOptions, ICommandHandler
    {
        // Note to future self:
        //  Formatters massage output into the desired shape
        //  Serializers transform said shape to desired data format
        //
        // Typically our text formatters do heavy manipulation of the shape, and need no serilization.
        // Whereas our formal data types (csv, JSON, JSON-L) need no formatting, and need serialization.

        private static readonly Func<object, object> DefaultFormatter = (x) => x switch
        {
            string s => null, //discard
            _ => x,
        };

        private static readonly Func<object, object> TextFormatter = (x) =>
        {
            if (x is null)
            {
                return x;
            }
            else if (x is string s)
            {
                return s;
            }

            return ThrowUnsupported(x);
        };

        private static readonly Func<object, object> SupressMessagesTextFormatter = (x) =>
        {
            if (x is null)
            {
                return x;
            }
            else if (x is string s)
            {
                // suppress output
                return null;
            }

            return ThrowUnsupported(x);
        };

        private Func<object, object> formatter;

        public EmuCommandHandler()
        {
            // watch out setting things here - CLI options have not yet been bound when constructor runs
        }

        public Func<object, object> Formatter => this.formatter ??= this.Format switch
        {
            OutputFormat.Default => this.FormatDefault,
            OutputFormat.CSV => this.FormatCsv,
            OutputFormat.JSON => this.FormatJson,
            OutputFormat.JSONL => this.FormatJsonLines,
            OutputFormat.Compact => this.FormatCompact,
            _ => throw new NotImplementedException(),
        };

        public OutputRecordWriter Writer { get; init; }

        public abstract Task<int> InvokeAsync(InvocationContext context);

        public void WriteHeader<T>()
        {
            this.Writer.WriteHeader<T>(default);
        }

        public void Write<T>(T record)
        {
            switch (this.Formatter(record))
            {
                case null: // noop
                    break;
                case T t:
                    this.Writer.Write(t);
                    break;
                case string s:
                    this.Writer.WriteFooter(s);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void WriteFooter<T>(T record)
        {
            switch (this.Formatter(record))
            {
                case null: // noop
                    break;
                case T t:
                    this.Writer.WriteFooter(t);
                    break;
                case string s:
                    this.Writer.WriteFooter(s);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected static object ThrowUnsupported(object errorValue)
        {
            throw new InvalidOperationException($"Formatting not supported for type of {errorValue?.GetType()?.Name}");
        }

        protected virtual object FormatDefault<T>(T record) => TextFormatter(record);

        protected virtual object FormatCompact<T>(T record) => SupressMessagesTextFormatter(record);

        protected virtual object FormatJson<T>(T record) => DefaultFormatter(record);

        protected virtual object FormatJsonLines<T>(T record) => DefaultFormatter(record);

        protected virtual object FormatCsv<T>(T record) => DefaultFormatter(record);
    }
}
