// <copyright file="EmuEntry.cs" company="QutEcoacoustics">
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
        private static readonly Func<object, object> DefaultFormatter = (x) => x switch
        {
            string s => null, //discard
            _ => x,
        };

        private static readonly Func<object, object> TextFormatter = (x) => x;
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

        public abstract Task<int> InvokeAsync(InvocationContext context);

        public OutputRecordWriter Writer { get; init; }

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

        protected object ThrowUnsupported(object errorValue)
        {
            throw new InvalidOperationException($"Formatting not supported for type of {errorValue?.GetType()?.Name}");
        }

        protected virtual object FormatDefault<T>(T record) => TextFormatter(record);

        protected virtual object FormatCompact<T>(T record) => TextFormatter(record);

        protected virtual object FormatJson<T>(T record) => DefaultFormatter(record);

        protected virtual object FormatJsonLines<T>(T record) => DefaultFormatter(record);

        protected virtual object FormatCsv<T>(T record) => DefaultFormatter(record);
    }

}
