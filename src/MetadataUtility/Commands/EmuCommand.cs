// <copyright file="EmuCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.CommandLine;
    using System.CommandLine.Parsing;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions;
    using MetadataUtility.Commands.Metadata;
    using MetadataUtility.Commands.Version;
    using MetadataUtility.Extensions.System.CommandLine;

    public class EmuCommand : RootCommand
    {
        public EmuCommand()
            : base("EMU - Ecoacoustic Metadata Utility")
        {
            this.AddGlobalOption(VerboseOption);
            this.AddGlobalOption(VeryVerboseOption);
            this.AddGlobalOption(LogLevelOption);
            this.AddGlobalOption(FormatOption);
            this.AddGlobalOption(OutOption);
            this.AddGlobalOption(ClobberOption);

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
            new string[] { "--output", "-O" },
            () => null,
            "Where to output data. Defaults to stdout if not supplied")
            .LegalFilePathsOnly()
            .WithValidator(OutputValidiator);

        public static Option<bool> ClobberOption { get; } = new Option<bool>(
            new string[] { "--clobber", "-C" },
            "Overwrites output file, used in junction with --output. No effect for standard out");

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

        [SuppressMessage(
            "System.IO.Abstractions",
            "IO0002:Replace File class with IFileSystem.File for improved testability",
            Justification = "We can't inject IFileSystem at this stage.")]
        private static string OutputValidiator(OptionResult optionResult)
        {
            ArgumentNullException.ThrowIfNull(optionResult);

            var outPath = optionResult?.GetValueOrDefault<string>();

            if (outPath is null)
            {
                return default;
            }

            var commandResult = optionResult?.Parent as CommandResult;
            var clobber = commandResult?.FindResultFor(ClobberOption)?.GetValueOrDefault<bool>() ?? false;

            if (clobber)
            {
                return default;
            }

            if (File.Exists(outPath))
            {
                return $"Will not overwrite existing output file {outPath}, use --clobber option or select a different name";
            }

            return default;
        }
    }
}
