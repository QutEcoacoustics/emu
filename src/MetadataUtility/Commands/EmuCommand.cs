// <copyright file="EmuEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine;
    using MetadataUtility.Commands.Version;

    public class EmuCommand : RootCommand
    {
        public enum LogLevel : short
        {
            None = 0,
            Critical = 1,
            Error = 2,
            Warning = 3,
            Information = 4,
            Debug = 5,
            Trace = 6,
        }

        public enum OutputFormat
        {
            Default,
            CSV,
            JSON,
            JSONL,
            Compact,
        }

        public static Option<bool> VerboseOption { get; } = new Option<bool>(
            new[] { "-v", "--verbose" },
            "Logging verbosity, specify multiple times to up verbosity");

        public static Option<bool> VeryVerboseOption { get; } = new Option<bool>(
            new[] { "-vv", "---very-verbose" },
            "Logging verbosity, specify multiple times to up verbosity");

        public static Option<LogLevel> LogLevelOption { get; } = new Option<LogLevel>(
            new string[] { "-l", "--log-level" },
            () => LogLevel.Information,
            "Set the log level");

        public static Option<OutputFormat> FormatOption { get; } = new Option<OutputFormat>(
            new string[] { "--format", "-F" },
            () => OutputFormat.Default,
            "Which format to ouput results in.");

        public static Option<string> OutOption { get; } = new Option<string>(
            new string[] { "--out" },
            () => null,
            "Where to output data. Defaults to stdout if not supplied")
            .LegalFilePathsOnly();

        public EmuCommand()
            : base("EMU - Ecoacoustic Metadata Utility")
        {
            this.AddGlobalOption(VerboseOption);
            this.AddGlobalOption(VeryVerboseOption);

            this.AddGlobalOption(LogLevelOption);

            this.AddGlobalOption(FormatOption);

            this.Add(new Command("metadata", "extract metadata"));
            this.Add(new Command("rename", "rename files to a consistent format"));
            this.Add(new FixCommand());
            this.Add(new VersionCommand());

        }
    }
}
