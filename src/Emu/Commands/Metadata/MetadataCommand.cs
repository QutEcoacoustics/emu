// <copyright file="MetadataCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using Emu.Commands;
    using Emu.Commands.Metadata.Dump;
    using Emu.Commands.Metadata.Show;

    public class MetadataCommand : Command
    {
        public MetadataCommand()
            : base("metadata", "extracts metadata from one or more files\nSee sub-commands for more features")
        {
            this.AddArgument(Common.Targets);
            this.AddOption(NoChecksumOption);

            this.AddCommand(new MetadataShowCommand());
            this.AddCommand(new MetadataDumpCommand());
            this.Handler = CommandHandler.Create(() =>
            {
                // noop
                // unless this is set, we get the error
                // "Required command was not provided"
                // even though we've registered the handler in our bootstrap
            });
        }

        public static Option<bool> NoChecksumOption { get; } =
            new(
                "--no-checksum",
                "Doesn't calculate checksum, important for archiving purposes but computationally expensive");
    }
}
