// <copyright file="FixApplyCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System.CommandLine;
    using Emu.Commands;

    public class FixApplyCommand : Command
    {
        public const string Summary = @"
Apply one or more fixes to a file.
Automatic fixes are fixed in place.
Non-fixable problems are renamed unless --no-rename is specified.
";

        public FixApplyCommand()
            : base("apply", Summary)
        {
            this.AddArgument(Common.Targets);

            this.AddOption(Common.Fixes);

            this.AddOption(new Option<bool>(new string[] { "-n", "--dry-run" }, "Do a \"dry run\" by simulating any change that writes data"));

            this.AddOption(new Option<bool>(new string[] { "--backup" }, "Backup the original file before writing any changes"));

            this.AddOption(new Option<bool>(new string[] { "--no-rename" }, "Do not rename unfixable files"));
        }
    }
}
