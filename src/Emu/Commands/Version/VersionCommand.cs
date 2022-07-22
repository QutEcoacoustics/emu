// <copyright file="VersionCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Version
{
    using System.CommandLine;

    public class VersionCommand : Command
    {
        public const string Summary = @"Show EMU's version.
Same as --version but honours output format selection.";

        public VersionCommand()
            : base("version", Summary)
        {
        }
    }
}
