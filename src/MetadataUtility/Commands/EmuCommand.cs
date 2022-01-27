// <copyright file="EmuCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine;
    using System.CommandLine.Parsing;
    using MetadataUtility.Commands.Version;
    using MetadataUtility.Commands.Metadata;

    public class EmuCommand : RootCommand
    {
        public EmuCommand()
        : base("EMU - Ecoacoustic Metadata Utility")
        {
            this.AddGlobalOption(VerboseOption);
            this.AddGlobalOption(VeryVerboseOption);

            this.AddGlobalOption(LogLevelOption);

            this.AddGlobalOption(FormatOption);

            this.Add(new MetadataCommand());
            this.Add(new RenameCommand());
            this.Add(new FixCommand());
            this.Add(new VersionCommand());
        }

        public enum LogLevel : short
        {
            None = 0,
            Crit = 1,
            Error = 2,
            Warn = 3,
            Info = 4,
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
            "Log verbosely - equivalent to log level debug");

        public static Option<bool> VeryVerboseOption { get; } = new Option<bool>(
            new[] { "-vv", "--very-verbose" },
            "Log very verbosely - equivalent to log level trace");

        public static Option<LogLevel> LogLevelOption { get; } = new Option<LogLevel>(
            new string[] { "-l", "--log-level" },
            () => LogLevel.Info,
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

        public static LogLevel GetLogLevel(ParseResult parseResult)
        {
            var verbose = parseResult.FindResultFor(VerboseOption)?.GetValueOrDefault<bool>() switch
            {
                true => LogLevel.Debug,
                _ => LogLevel.None,
            };
            var veryVerbose = parseResult.FindResultFor(VeryVerboseOption)?.GetValueOrDefault<bool>() switch
            {
                true => LogLevel.Trace,
                _ => LogLevel.None,
            };
            var logLevel = parseResult.FindResultFor(LogLevelOption)!.GetValueOrDefault<LogLevel>();

            var level = new[] { (int)logLevel, (int)verbose, (int)veryVerbose }.Max();

            return (LogLevel)level;
        }
    }
}
