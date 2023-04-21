// <copyright file="CuesCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Cues
{
    using System.CommandLine;

    public class CuesCommand : Command
    {
        public CuesCommand()
            : base("cues", "Extract cue points from wave files")
        {
            this.AddArgument(Common.Targets);

            this.AddOption(new Option<bool>(new string[] { "--export" }, "Export the cues to a file next to the target"));
        }
    }
}
