// <copyright file="EmuEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

#pragma warning disable SA1200 // Using directives should be placed correctly
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using static LanguageExt.Prelude;
#pragma warning restore SA1200 // Using directives should be placed correctly

namespace MetadataUtility
{
    using System.CommandLine;
    using System.CommandLine.Builder;
    using System.CommandLine.Hosting;
    using System.CommandLine.Parsing;
    using System.Threading.Tasks;
    using MetadataUtility.Cli;
    using MetadataUtility.Commands.Version;
    using MetadataUtility.Extensions.System.CommandLine;
    using MetadataUtility.Filenames;
    using MetadataUtility.Fixes;
    using MetadataUtility.Serialization;
    using MetadataUtility.Utilities;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using NodaTime;
    using NodaTime.Text;
    using Serilog;
    using Serilog.Events;
    using Serilog.Sinks.SystemConsole.Themes;
    using static MetadataUtility.EmuCommand;

    /// <summary>
    /// The main entry point for running EMU.
    /// </summary>
    public partial class EmuEntry
    {
        /// <summary>
        /// Gets the RootCommand for the application.
        /// </summary>
        /// <returns>A RootCommand instance.</returns>
        public static RootCommand RootCommand { get; } = new EmuCommand();

        /// <summary>
        /// Run EMU with commandline arguments.
        /// </summary>
        /// <param name="args">The args array received by the executable.</param>
        public static async Task<int> Main(string[] args)
        {
            return await BuildCommandLine().InvokeAsync(args);
        }

        /// <summary>
        /// Creates (but does not build/finalize) a CommandLineApplication object for EMU.
        /// </summary>
        /// <returns>A CommandLineBuilder.</returns>
        public static CommandLineBuilder CreateCommandLine() =>
            new CommandLineBuilder(RootCommand)
            .UseHost(CreateHost, BuildDependencies)
            .UseDefaults()
            .UseHelpBuilder((context) => new EmuHelpBuilder(context.Console));

        /// <summary>
        /// Builds a parser the command line arguments for EMU.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The CommandLineApplication object and a binding model of arguments.</returns>
        public static Parser BuildCommandLine() => CreateCommandLine().Build();

        private static IHostBuilder CreateHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args);
        }

        private static void BuildDependencies(IHostBuilder host)
        {
            host.ConfigureServices((services) =>
            {
                services
                .AddSingleton<TextWriter>(OutputSink.Factory)
                .AddSingleton<CsvSerializer>()
                .AddSingleton<JsonSerializer>()
                .AddSingleton<JsonLinesSerializer>()
                .AddSingleton<ToStringFormatter>()
                .AddTransient<IRecordFormatter>(OutputRecordWriter.FormatterResolver)
                .AddSingleton<Lazy<OutputFormat>>(
                    (provider) => new Lazy<OutputFormat>(() => provider.GetRequiredService<EmuGlobalOptions>().Format))
                .AddTransient<OutputRecordWriter>()
                //.AddTransient<DefaultFormatters>()
                .AddSingleton(typeof(FilenameParser), _ => FilenameParser.Default)
                .AddSingleton<FileMatcher>()
                .AddSingleton<FileUtilities>()
                .AddSingleton<Renamer>()
                .AddSingleton<FilenameSuggester>()
                .AddTransient<Processor>();

                services.BindOptions<EmuGlobalOptions>();

                services.AddSingleton<FixRegister>();
                foreach (var fix in FixRegister.All)
                {
                    services.AddTransient(fix.FixClass);
                }
            });

            host.UseEmuCommand<FixListCommand, FixList>();
            host.UseEmuCommand<FixCheckCommand, FixCheck>();
            host.UseEmuCommand<FixApplyCommand, FixApply>();
            host.UseEmuCommand<VersionCommand, Version>();

            host.UseSerilog(ConfigureLogging);
        }

        private static void ConfigureLogging(HostBuilderContext context, LoggerConfiguration configuration)
        {

            // TODO: use model binding
            var parseResult = context.GetInvocationContext().ParseResult;

            var verbose = parseResult.FindResultFor(EmuCommand.VerboseOption)?.GetValueOrDefault<bool>() switch
            {
                true => EmuCommand.LogLevel.Debug,
                _ => EmuCommand.LogLevel.None,
            };
            var veryVerbose = parseResult.FindResultFor(EmuCommand.VeryVerboseOption)?.GetValueOrDefault<bool>() switch
            {
                true => EmuCommand.LogLevel.Trace,
                _ => EmuCommand.LogLevel.None,
            };
            var logLevel = parseResult.FindResultFor(EmuCommand.LogLevelOption)!.GetValueOrDefault<EmuCommand.LogLevel>();

            var max = LogEventLevel.Fatal + 1;
            var level = max - new[] { (int)logLevel, (int)verbose, (int)veryVerbose }.Max();

            Log.Error("command result {0}, verbosity {1} / {2}", level, verbose, veryVerbose);

            configuration
                 .Enrich.WithThreadId()
                 .Destructure.ByTransforming<OffsetDateTime>(OffsetDateTimePattern.Rfc3339.Format)
                 .Destructure.ByTransforming<LocalDateTime>(LocalDateTimePattern.ExtendedIso.Format)
                 .Destructure.ByTransforming<Instant>(InstantPattern.ExtendedIso.Format)
                 .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                 .MinimumLevel.Is(level)
                 .WriteTo.Console(
                     theme: AnsiConsoleTheme.Literate,
                     outputTemplate: "{Timestamp:o} [{Level:u4}] <{ThreadId}> {SourceContext} {Scope} {Message:lj}{NewLine}{Exception}",
                     standardErrorFromLevel: LogEventLevel.Verbose);
        }

        // BROKEN!
        private static async Task<int> Execute(dynamic mainArgs)
        {
            //            logger.LogCritical("Critical message");
            //            logger.LogError("Error message");
            //            logger.LogWarning("Warning message");
            //            logger.LogInformation("Informational message");
            //            logger.LogDebug("Debug message");
            //            logger.LogTrace("Trace message");
            //var targets = mainArgs.Targets;
            throw new NotImplementedException();
            //this.logger.LogInformation("Input targets: {0}", targets);

            //var fileMatcher = serviceProvider.GetRequiredService<FileMatcher>();
            //var renamer = serviceProvider.GetRequiredService<Renamer>();
            //var writer = serviceProvider.GetRequiredService<OutputWriter>();

            //int count = 0;
            //var allPaths = fileMatcher.ExpandMatches(Directory.GetCurrentDirectory(), targets);
            //var tasks = new List<Task<Recording>>();

            //// queue work
            //foreach (var path in allPaths)
            //{
            //    var processor = serviceProvider.GetRequiredService<Processor>();
            //    var task = processor.ProcessFile(path);
            //    tasks.Add(task);
            //}

            //// wait for work
            //var results = await Task.WhenAll(tasks.ToArray());

            //if (mainArgs.Rename)
            //{
            //    results = await renamer.RenameAll(results);
            //}

            //// summarize work
            //foreach (var recording in results)
            //{
            //    if (recording != null)
            //    {
            //        count++;
            //        writer.Write(recording);
            //    }
            //}

            //return count;
        }
    }
}
