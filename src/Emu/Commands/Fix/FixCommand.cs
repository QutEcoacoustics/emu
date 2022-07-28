// <copyright file="FixCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System.CommandLine;

    public class FixCommand : Command
    {
        public FixCommand()
           : base("fix", "fix or check for problems in one or more files")
        {
            this.AddCommand(new FixListCommand());
            this.AddCommand(new FixCheckCommand());
            this.AddCommand(new FixApplyCommand());
        }
    }
}
