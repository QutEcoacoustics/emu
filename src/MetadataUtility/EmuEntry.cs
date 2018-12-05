// <copyright file="EmuEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using DotNet.Globbing;
    using McMaster.Extensions.CommandLineUtils;
    using MetadataUtility.Filenames;
    using MetadataUtility.Serialization;
    using MetadataUtility.Utilities;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The main entry point for running EMU.
    /// </summary>
    public class EmuEntry
    {
        private static IServiceProvider serviceProvider;

        /// <summary>
        /// Run EMU with commandline arguments.
        /// </summary>
        /// <param name="args">The args array received by the executable.</param>
        public static async Task<int> Main(string[] args)
        {
            serviceProvider = BuildDependencies();

            return await ProcessArguments(args);
        }

        private static IServiceProvider BuildDependencies()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ISerializer, CsvSerializer>()
                .AddSingleton(typeof(FilenameParser), provider => FilenameParser.Default)
                .AddSingleton<FileMatcher>();

            serviceProvider = ConfigureLogging(serviceProvider);

            return serviceProvider.BuildServiceProvider();
        }

        private static IServiceCollection ConfigureLogging(IServiceCollection services)
        {
            return services.AddLogging(
                (configure) =>
                {
                    configure.SetMinimumLevel(LogLevel.Trace);
                    configure.AddConsole(
                        options => { });
                });
        }

        public static async Task<int> ProcessArguments(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();

            var targets = app.Argument<string>(
                "Recordings",
                "The recordings to process",
                multipleValues: true);

            app.OnExecute(async () =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<EmuEntry>>();

                logger.LogCritical("Critical message");
                logger.LogError("Error message");
                logger.LogWarning("Warning message");
                logger.LogInformation("Informational message");
                logger.LogDebug("Debug message");
                logger.LogTrace("Trace message");

                logger.LogInformation("Input arguments: {0}", targets.ParsedValues);

                var fileMatcher = serviceProvider.GetRequiredService<FileMatcher>();

                fileMatcher.ExpandMatches(Directory.GetCurrentDirectory(), targets.ParsedValues);

                return await Task.FromResult(1);
            });

            var processArguments = app.Execute(args);

            //Prompt.GetString("Press enter to quit");

            return processArguments;
        }

        public static async void ProcessFile(string path)
        {
        }
    }
}
