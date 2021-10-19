// <copyright file="FixApplyCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine;
    using MetadataUtility.Commands;
    using MetadataUtility.Extensions.System.CommandLine;

    public class FixApplyCommand : Command
    {
        public FixApplyCommand()
            : base("apply", "apply one or more fixes to a file. Currently only supports automatic fixes.")
        {
            this.AddArgument(CommonArguments.Targets);

            this.AddOption(CommonArguments.Fixes);

            this.AddOption(new Option<bool>(new string[] { "-n", "--dry-run" }, "Do a \"dry run\" by simulating any change that writes data"));

            this.AddOption(new Option<bool>(new string[] { "--backup" }, "Backup the original file before writing any changes"));
        }
    }
}
