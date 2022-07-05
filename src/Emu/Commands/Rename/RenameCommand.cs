// <copyright file="RenameCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System.CommandLine;
    using Emu.Cli;
    using Emu.Commands;
    using NodaTime;

    public class RenameCommand : Command
    {
        public RenameCommand()
            : base("rename", "rename one or more files.")
        {
            this.AddArgument(Common.Targets);

            this.AddOption(Common.DryRun);

            this.AddOption(new Option<DirectoryInfo>(new string[] { "--copy-to" }, "Create copies of the original files and move them to this directory"));

            this.AddOption(new Option<bool>(new string[] { "--flatten" }, "Flattens files so all files in folders are moved up to the target directory"));

            this.AddOption(
                new Option<Offset?>(
                    "--new-offset",
                    UtcOffsetOption.Parser,
                    description: "Changes the UTC offset of the datestamp. Use this to change the datestamp to your another timezone. Note: does not change the instant the timestamp occurs at.")
                .ValidUtcOffset());

            this.AddOption(
                new Option<Offset?>(
                    "--offset",
                    UtcOffsetOption.Parser,
                    description: "Adds a UTC offset to the datestamp. Only affects local datestamps without an offset. Use this convert a local date to a global date.")
                .ValidUtcOffset());
        }
    }
}
