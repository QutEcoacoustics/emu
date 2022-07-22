// <copyright file="RenameCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System.CommandLine;
    using Emu.Cli;
    using Emu.Commands;
    using NodaTime;

    public class RenameCommand : Command, IHelpPostScript
    {
        public RenameCommand()
            : base("rename", "rename one or more files.")
        {
            this.AddArgument(Common.Targets);

            this.AddOption(Common.DryRun);

            this.AddOption(new Option<string>(
                new string[] { "-t", "--template" },
                "Provide a template for the rename. You can template any field that is output from the metadata command."));

            this.AddOption(new Option<DirectoryInfo>(new string[] { "--copy-to" }, "Create copies of the files and move them to this directory."));

            this.AddOption(new Option<bool>(new string[] { "--flatten" }, "Flattens (removes directories) files into the target directory."));

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

            this.AddOption(new Option<bool>(
                new string[] { "-m", "--scan-metadata" },
                "Scan the files contents (and surrounding directories) for extra metadata, not just the file name. This makes the rename slower."));
        }

        public string PostScript
        {
            get
            {
                var example = "--template=\"MyNewName_{StartDate}_{DurationSeconds}{Extension}\"";
                return $@"
The `--template` option allows you to customize the naming of the file.
Use any field output by the `metadata` command as a template placeholder.
We recommend using the `--scan-metadata` option to scan the file contents for extra metadata when using a template.
You can mix place holders and literal text. Wrap placeholders in curly braces ({{, }}).
E.g. `{example}`
";
            }
        }
    }
}
