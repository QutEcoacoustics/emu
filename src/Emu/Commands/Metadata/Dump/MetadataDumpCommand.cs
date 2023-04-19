// <copyright file="MetadataDumpCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Metadata.Dump
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Parsing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using static Emu.EmuCommand;

    public class MetadataDumpCommand : Command
    {
        public MetadataDumpCommand()
            : base("dump", "show low-level metdata blocks from inside audio files")
        {
            this.AddArgument(Common.Targets);

            this.AddValidator(commandResult =>
            {
                var result = commandResult.FindResultFor(FormatOption);
                if (result?.GetValueOrDefault<OutputFormat>() == OutputFormat.CSV)
                {
                    return "CSV output is not supported for this command because the data is too variable to be effectively shown in a table.";
                }

                return null;
            });
        }
    }
}
