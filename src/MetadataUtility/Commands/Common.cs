// <copyright file="Common.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Commands
{
    using System.CommandLine;
    using MetadataUtility.Extensions.System.CommandLine;

    public static class CommonArguments
    {
        public static Argument<string[]> Targets { get; } = new("targets") { Arity = ArgumentArity.OneOrMore };

        public static Option<string[]> Fixes { get; } = new Option<string[]>(
            "--fix",
            CommandLineExtensions.SplitOnComma<string>(),
            description: "The ID of a well known problem to check for. See `emu fix list`")
        {
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = false,
        }.WithAlias("-f");
    }
}
