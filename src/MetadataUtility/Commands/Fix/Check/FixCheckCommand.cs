// <copyright file="EmuEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine;

    public class FixCheckCommand : Command
    {
        public FixCheckCommand()
            : base("check", "check if a file needs a fix")
        {
            this.AddArgument(new Argument<string[]>("targets") { Arity = ArgumentArity.OneOrMore });
            this.AddOption(new Option<string>(new string[] { "-f", "--fix" }, "The ID of a well known problem to check for. See `emu fix list`")
            {
                Arity = ArgumentArity.OneOrMore,
            });
            this.AddOption(new Option<bool>(new string[] { "--all" }, "Check for all well known problems"));
        }
    }
}
