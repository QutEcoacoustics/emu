// <copyright file="FixListCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine;

    public class FixListCommand : Command
    {
        public FixListCommand()
           : base("list", "list all fixes")
        {
        }
    }
}
