// <copyright file="MainArgs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Cli
{
    using System;
    using System.Collections.Generic;
    using McMaster.Extensions.CommandLineUtils;
    using MetadataUtility.Dates;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    /// <summary>
    /// The command line arguments provided to EMU.
    /// </summary>
    public class MainArgs
    {
        private readonly Lazy<IReadOnlyCollection<string>> targets;
        private readonly CommandOption<string> utcOffsetHint;
        private readonly CommandOption dryRun;
        private readonly CommandOption<bool> verbosity;
        private readonly CommandOption<LogLevel> logLevel;
        private readonly CommandOption rename;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainArgs"/> class.
        /// </summary>
        public MainArgs(
            CommandArgument<string> targets,
            CommandOption<string> utcOffsetHint,
            CommandOption rename,
            CommandOption dryRun,
            CommandOption<bool> verbosity,
            CommandOption<LogLevel> logLevel)
        {
            this.targets = new Lazy<IReadOnlyCollection<string>>(() => targets.ParsedValues);
            this.utcOffsetHint = utcOffsetHint;
            this.rename = rename;
            this.dryRun = dryRun;
            this.verbosity = verbosity;
            this.logLevel = logLevel;
        }

        /// <summary>
        /// Gets the targets to process - these should be globs or actual files.
        /// </summary>
        public IReadOnlyCollection<string> Targets => this.targets.Value;

        /// <summary>
        /// Gets the UTC offset to use if the date extracted from the recording is ambiguous.
        /// </summary>
        public Offset? UtcOffsetHint
        {
            get
            {
                if (this.utcOffsetHint.HasValue())
                {
                    if (Parsing.TryParseOffset(this.utcOffsetHint.Value(), out var offset))
                    {
                        return offset;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to rename the files according to the recommended name.
        /// </summary>
        public bool Rename => this.rename.HasValue();

        /// <summary>
        /// Gets a value indicating whether to do a dry run. In a dry run no changes are made.
        /// </summary>
        public bool DryRun => this.dryRun.HasValue();

        /// <summary>
        /// Gets the log level (the verbosity) that should be used when writing logs.
        /// </summary>
        public LogLevel Verbosity
        {
            get
            {
                if (this.logLevel.HasValue())
                {
                    return this.logLevel.ParsedValue;
                }
                else
                {
                    switch (this.verbosity.Values.Count)
                    {
                        case 0:
                            return LogLevel.Information;
                        case 1:
                            return LogLevel.Debug;
                        default:
                            return LogLevel.Trace;
                    }
                }
            }
        }
    }
}
