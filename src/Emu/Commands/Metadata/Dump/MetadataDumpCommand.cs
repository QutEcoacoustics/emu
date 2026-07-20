// <copyright file="MetadataDumpCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Metadata.Dump
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Parsing;
    using Emu.Extensions.System.CommandLine;
    using static Emu.EmuCommand;

    public class MetadataDumpCommand : Command
    {
        public MetadataDumpCommand()
            : base("dump", "show low-level metadata blocks from inside audio files")
        {
            this.AddArgument(Common.Targets);
            this.AddOption(BlockFilterOption);

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

        public static Option<string[]> BlockFilterOption { get; } =
            new Option<string[]>(
                new[] { "--blocks", "-B" },
                parseArgument: CommandLineExtensions.SplitOnComma<string>(),
                description: "Only output the selected metadata blocks (case-insensitive). Example: --blocks GUANO or --blocks GUANO,WAMD")
            {
                Arity = ArgumentArity.ZeroOrMore,
            };
    }
}
