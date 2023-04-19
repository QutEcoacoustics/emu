// <copyright file="MetadataShowCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Metadata.Show
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class MetadataShowCommand : Command
    {
        public MetadataShowCommand()
            : base("show", "extracts metadata from one or more files")
        {
            this.AddArgument(Common.Targets);
            this.AddOption(MetadataCommand.NoChecksumOption);
        }
    }
}
