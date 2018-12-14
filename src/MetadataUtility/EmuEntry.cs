// <copyright file="EmuEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNet.Globbing;
    using McMaster.Extensions.CommandLineUtils;
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
            // It's really important to dispose all the services we build.
            // Things like the logger run in  a background thread and won't flush the rest of their messages
            // if a program quits... unless the service is disposed of.
            using (serviceProvider = BuildDependencies())
            {
                logger = serviceProvider.GetRequiredService<ILogger<EmuEntry>>();
                var processArguments = await ProcessArguments(args);
            }

            return 0;
        }

        /// <summary>
        /// Processes the command line arguments for EMU.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The status code for the program.</returns>
        public static async Task<int> ProcessArguments(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();

            var targets = app.Argument<string>(
                "Recordings",
                "The recordings to process",
                multipleValues: true);

            app.OnExecute(async () => await Execute(targets.ParsedValues));

            return await Task.FromResult(app.Execute(args));
        }

        private static ServiceProvider BuildDependencies()
        {
            var services = new ServiceCollection()
                .AddSingleton<ISerializer, CsvSerializer>()
                .AddSingleton<ISerializer, JsonSerializer>()
                .AddSingleton<OutputWriter>(collection => new OutputWriter(collection.GetRequiredService<ISerializer>(), Console.Out))
                .AddSingleton(typeof(FilenameParser), provider => FilenameParser.Default)
                .AddSingleton<FileMatcher>()
                .AddTransient<Processor>();

            services = ConfigureLogging(services);

            return services.BuildServiceProvider();
        }

        private static IServiceCollection ConfigureLogging(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .Destructure.ByTransforming<OffsetDateTime>(OffsetDateTimePattern.Rfc3339.Format)
                .Destructure.ByTransforming<LocalDateTime>(LocalDateTimePattern.ExtendedIso.Format)
                .Destructure.ByTransforming<Instant>(InstantPattern.ExtendedIso.Format)
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .WriteTo.Console(
                    theme: AnsiConsoleTheme.Literate,
                    outputTemplate: "{Timestamp:o} [{Level:w5}] <{ThreadId}> {SourceContext} {Message:lj}{NewLine}{Exception}",
                    standardErrorFromLevel: LogEventLevel.Verbose)
                .CreateLogger();

            return services.AddLogging(
                (configure) =>
                {
                    configure.AddSerilog(Log.Logger, dispose: true);
                });
        }

        private static async Task<int> Execute(IReadOnlyList<string> targets)
        {
//            logger.LogCritical("Critical message");
//            logger.LogError("Error message");
//            logger.LogWarning("Warning message");
//            logger.LogInformation("Informational message");
//            logger.LogDebug("Debug message");
//            logger.LogTrace("Trace message");

            logger.LogInformation("Input arguments: {0}", targets);

            var fileMatcher = serviceProvider.GetRequiredService<FileMatcher>();

            int count = 0;
            var allPaths = fileMatcher.ExpandMatches(Directory.GetCurrentDirectory(), targets);
            var tasks = new List<Task<Recording>>();

            // queue work
            foreach (var path in allPaths)
            {
                var processor = serviceProvider.GetRequiredService<Processor>();
                var task = processor.All(path);
                tasks.Add(task);
            }

            // wait for work
            var results = await Task.WhenAll(tasks.ToArray());

            // summarize work
            foreach (var task in results)
            {
                if (task != null)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
