// <copyright file="Common.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands
{
    using System.CommandLine;
    using Emu.Extensions.System.CommandLine;

    public static class Common
    {
        public static Argument<string[]> Targets { get; } = new(
            "targets",
            "One more glob patterns for files to process. E.g. '**/*.mp3'.")
        { Arity = ArgumentArity.OneOrMore };

        public static Option<bool> DryRun { get; } = new(
            new string[] { "-n", "--dry-run" },
            "Do a \"dry run\" by simulating any change that writes data.");

        public static Option<string[]> Fixes { get; } = new Option<string[]>(
            "--fix",
            CommandLineExtensions.SplitOnComma<string>(),
            description: "The ID of a well known problem to check for. See `emu fix list`.")
        {
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = false,
        }.WithAlias("-f");
    }
}
