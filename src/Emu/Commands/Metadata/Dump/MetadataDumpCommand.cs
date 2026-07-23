// <copyright file="MetadataDumpCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Metadata.Dump
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Parsing;
    using System.Linq;
    using Emu.Extensions.System.CommandLine;
    using static Emu.EmuCommand;

    public class MetadataDumpCommand : Command
    {
        public MetadataDumpCommand()
            : base("dump", "show low-level metadata blocks from inside audio files")
        {
            // HACK (metadata command backwards compatibility): dump uses a distinct argument symbol from metadata/show, even though
            // the token name is still "targets". Reusing Common.Targets in both parent
            // and subcommand causes collisions in System.CommandLine's symbol maps.
            this.AddArgument(DumpTargets);
            this.AddOption(BlockFilterOption);

            this.AddValidator(commandResult =>
            {
                // HACK (metadata command backwards compatibility): inspect parent argument tokens directly instead of FindResultFor.
                // FindResultFor can trigger duplicate-key failures when parent and child
                // argument names are the same.
                var parentTargets = commandResult.Parent?
                    .Children
                    .OfType<ArgumentResult>()
                    .FirstOrDefault(x => x.Argument == Common.Targets);

                if (parentTargets is { Tokens.Count: > 0 })
                {
                    // Behavior contract: metadata x dump y is invalid; dump targets must
                    // be supplied only after the dump subcommand.
                    return "Targets for `metadata dump` must be provided after `dump` only.";
                }

                var result = commandResult.FindResultFor(FormatOption);
                if (result?.GetValueOrDefault<OutputFormat>() == OutputFormat.CSV)
                {
                    return "CSV output is not supported for this command because the data is too variable to be effectively shown in a table.";
                }

                return null;
            });
        }

        // HACK (metadata command backwards compatibility): separate symbol with same public token name to keep CLI UX while avoiding
        // parent/subcommand symbol collision issues in parser internals.
        public static Argument<string[]> DumpTargets { get; } = new(
                "targets",
                "One more glob patterns for files to process. E.g. '**/*.wav'.")
        { Arity = ArgumentArity.OneOrMore };

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
