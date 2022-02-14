// <copyright file="MetadataCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine;
    using MetadataUtility.Cli;
    using MetadataUtility.Commands;
    using NodaTime;

    public class MetadataCommand : Command
    {
        public MetadataCommand()
            : base("metadata", "extracts metadata from one or more files.")
        {
            this.AddArgument(Common.Targets);

            // this.AddOption(new Option<bool>(
            //     new string[] { "--save" },
            //     "Saves normalized metadata back into the file"
            // ));

            //this.AddOption(CommonArguments.DryRun);

            // this.AddOption(
            //     new Option<Offset?>(
            //         "--offset",
            //         UtcOffsetOption.Parser,
            //         description: "Adds a UTC offset to the datestamp. Only affects local datestamps without an offset. Use this convert a local date to a global date.")
            //     .ValidUtcOffset());
        }
    }
}
