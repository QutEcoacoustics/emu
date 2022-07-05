// <copyright file="FixCheckCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System.CommandLine;
    using Emu.Commands;

    public class FixCheckCommand : Command
    {
        public FixCheckCommand()
            : base("check", "check if a file needs a fix")
        {
            this.AddArgument(Common.Targets);
            this.AddOption(Common.Fixes);
            this.AddOption(new Option<bool>(new string[] { "--all" }, "Check for all well known problems"));
        }
    }
}
