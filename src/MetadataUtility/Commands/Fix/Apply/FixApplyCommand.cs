// <copyright file="FixApplyCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine;

    public class FixApplyCommand : Command
    {
        public FixApplyCommand()
            : base("apply", "apply one or more fixes to a file. Currently only supports automatic fixes.")
        {
            this.AddArgument(new Argument<string[]>("targets") { Arity = ArgumentArity.OneOrMore });

            this.AddOption(new Option<string>(new string[] { "-f", "--fix" }, "The ID of a well known problem to check for. See `emu fix list`")
            {
                Arity = ArgumentArity.OneOrMore,
            });

            this.AddOption(new Option<bool>(new string[] { "-n", "--dry-run" }, "Do a \"dry run\" by simualting any change that writes data"));

            this.AddOption(new Option<bool>(new string[] { "-I", "--in-place" }, "Change the files in place"));

            this.AddOption(new Option<bool>(new string[] { "--backup" }, "Backup the original file before writing any changes"));
        }
    }
}
