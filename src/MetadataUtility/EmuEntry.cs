// <copyright file="EmuEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNet.Globbing;
    using McMaster.Extensions.CommandLineUtils;
    using McMaster.Extensions.CommandLineUtils.Validation;
    using MetadataUtility.Cli;
    using MetadataUtility.Filenames;
    using MetadataUtility.Models;
    using MetadataUtility.Serialization;
    using MetadataUtility.Utilities;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using NodaTime.Text;
    using Serilog;
    using Serilog.Events;
    using Serilog.Formatting.Json;
    using Serilog.Sinks.SystemConsole.Themes;

    /// <summary>
    /// The main entry point for running EMU.
    /// </summary>
    public class EmuEntry
    {
        private static ServiceProvider serviceProvider;
        private static ILogger<EmuEntry> logger;

        /// <summary>
        /// Run EMU with commandline arguments.
        /// </summary>
        /// <param name="args">The args array received by the executable.</param>
        public static async Task<int> Main(string[] args)
        {
            var (app, main) = ProcessArguments(args);

            // It's really important to dispose all the services we build.
            // Things like the logger run in  a background thread and won't flush the rest of their messages
            // if a program quits... unless the service is disposed of.
            using (serviceProvider = BuildDependencies(main))
            {
                logger = serviceProvider.GetRequiredService<ILogger<EmuEntry>>();

                await Task.FromResult(app.Execute(args));
            }

            return 0;
        }

        /// <summary>
        /// Processes the command line arguments for EMU.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The CommandLineApplication object and a binding model of arguments.</returns>
        public static (CommandLineApplication app, MainArgs mainArgs) ProcessArguments(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();

            var targets = app.Argument<string>(
                "Recordings",
                "The recordings to process",
                multipleValues: true);

            var utcOffsetHint = app.Option<string>(
                "-z|--utc-offset-hint",
                "The number of hours from UTC the sensor's clock was configured to",
                CommandOptionType.SingleValue);
            utcOffsetHint.Validators.Add(new OffsetValidator());

            var rename = app.Option(
                "--rename",
                "Rename files using the recommended name (use --dry-run to test)",
                CommandOptionType.NoValue);

            var dryRun = app.Option(
                "-n|--dry-run",
                "Do not make any changes (good for testing)",
                CommandOptionType.NoValue);

            var verbosity = app.Option<bool>(
                "-v|--verbose",
                "Specify logging verbosity, -v for verbose, -vv for very verbose",
                CommandOptionType.NoValue);

            var logLevel = app.Option<LogLevel>(
                "-l|--log-level <LOG_LEVEL>",
                "Specify logging verbosity",
                CommandOptionType.SingleValue);

            var mainArgs = new MainArgs(targets, utcOffsetHint, rename, dryRun, verbosity, logLevel);
            app.Parse(args);
            app.OnExecute(async () => await Execute(mainArgs));

            return (app, mainArgs);
        }

        private static ServiceProvider BuildDependencies(MainArgs main)
        {
            var services = new ServiceCollection()
                .AddSingleton<MainArgs>(_ => main)
                .AddSingleton<ISerializer, CsvSerializer>()
                .AddSingleton<ISerializer, JsonSerializer>()
                .AddSingleton<OutputWriter>(collection => new OutputWriter(collection.GetRequiredService<ISerializer>(), Console.Out))
                .AddSingleton(typeof(FilenameParser), provider => FilenameParser.Default)
                .AddSingleton<FileMatcher>()
                .AddSingleton<Renamer>()
                .AddSingleton<FilenameSuggester>()
                .AddTransient<Processor>();

            services = ConfigureLogging(services, main.Verbosity);

            return services.BuildServiceProvider();
        }

        private static IServiceCollection ConfigureLogging(IServiceCollection services, LogLevel logLevel)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .Destructure.ByTransforming<OffsetDateTime>(OffsetDateTimePattern.Rfc3339.Format)
                .Destructure.ByTransforming<LocalDateTime>(LocalDateTimePattern.ExtendedIso.Format)
                .Destructure.ByTransforming<Instant>(InstantPattern.ExtendedIso.Format)
                ////.MinimumLevel.Is(logLevel.)
                .WriteTo.Console(
                    theme: AnsiConsoleTheme.Literate,
                    outputTemplate: "{Timestamp:o} [{Level:w5}] <{ThreadId}> {SourceContext} {Message:lj}{NewLine}{Exception}",
                    standardErrorFromLevel: LogEventLevel.Verbose)
                .CreateLogger();

            return services.AddLogging(
                (configure) =>
                {
                    configure.SetMinimumLevel(logLevel);
                    configure.AddSerilog(Log.Logger, dispose: true);
                });
        }

        private static async Task<int> Execute(MainArgs mainArgs)
        {
//            logger.LogCritical("Critical message");
//            logger.LogError("Error message");
//            logger.LogWarning("Warning message");
//            logger.LogInformation("Informational message");
//            logger.LogDebug("Debug message");
//            logger.LogTrace("Trace message");
            var targets = mainArgs.Targets;

            logger.LogInformation("Input targets: {0}", targets);

            var fileMatcher = serviceProvider.GetRequiredService<FileMatcher>();
            var renamer = serviceProvider.GetRequiredService<Renamer>();
            var writer = serviceProvider.GetRequiredService<OutputWriter>();

            int count = 0;
            var allPaths = fileMatcher.ExpandMatches(Directory.GetCurrentDirectory(), targets);
            var tasks = new List<Task<Recording>>();

            // queue work
            foreach (var path in allPaths)
            {
                var processor = serviceProvider.GetRequiredService<Processor>();
                var task = processor.ProcessFile(path);
                tasks.Add(task);
            }

            // wait for work
            var results = await Task.WhenAll(tasks.ToArray());

            if (mainArgs.Rename)
            {
                results = await renamer.RenameAll(results);
            }

            // summarize work
            foreach (var recording in results)
            {
                if (recording != null)
                {
                    count++;
                    writer.Write(recording);
                }
            }

            return count;
        }
    }
}
