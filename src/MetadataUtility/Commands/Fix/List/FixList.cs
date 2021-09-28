// <copyright file="FixList.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine.Invocation;
    using System.Threading.Tasks;
    using MetadataUtility.Fixes;
    using MetadataUtility.Utilities;

    public class FixList : EmuCommandHandler
    {
        public FixList(OutputRecordWriter writer)
        {
            this.Writer = writer;
        }

        public override Task<int> InvokeAsync(InvocationContext context)
        {
            this.WriteHeader<OperationInfo>();
            this.Write("Problems that can be fixed:");

            foreach (var fix in FixRegister.All)
            {
                this.Write(fix);
            }

            this.WriteFooter(@"
Use `emu fix apply` to apply a fix to target files:

    emu fix apply --fix XX001 *.wav

Or use `--fix-all` to apply all known fixes:

    emu fix apply --fix-all XX001 *.wav
");
            return Task.FromResult(0);
        }

        protected override object FormatCompact<T>(T record)
        {
            if (record is OperationInfo fix)
            {
                var problem = fix.Problem;

                var line = $"{problem.Id}\t{problem.Title}\tfixable={fix.Fixable}\tsafe={fix.Safe}\tautomatic-fix={fix.Automatic}";
                return line;
            }
            else if (record is string)
            {
                // suppress output in compact mode
                return null;
            }

            return ThrowUnsupported(record);
        }

        protected override object FormatDefault<T>(T record)
        {
            if (record is OperationInfo fix)
            {
                var problem = fix.Problem;

                return $"\t- {problem.Id} {problem.Title}\tSafe: {fix.Safe}\tAutomatic fix:{fix.Automatic}";
            }
            else if (record is string s)
            {
                return s;
            }

            return ThrowUnsupported(record);
        }
    }
}
