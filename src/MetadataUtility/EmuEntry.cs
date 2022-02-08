// <copyright file="EmuEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

#pragma warning disable SA1200 // Using directives should be placed correctly
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;

#pragma warning restore SA1200 // Using directives should be placed correctly

namespace MetadataUtility
{
    using System.CommandLine;
    using System.CommandLine.Builder;
    using System.CommandLine.Hosting;
    using System.CommandLine.Parsing;
    using System.IO.Abstractions;
    using MetadataUtility.Cli;
    using MetadataUtility.Commands.Rename;
    using MetadataUtility.Commands.Metadata;
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
    using System.Runtime.CompilerServices;
    using System.Diagnostics;

    /// <summary>
    /// The main entry point for running EMU.
    /// </summary>
    public partial class EmuEntry
    {
        private static Parser builtCommandLine = null;

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
        private static CommandLineBuilder CreateCommandLine() =>
            new CommandLineBuilder(RootCommand)
            .UseHost(CreateHost, BuildDependencies)
            .UseDefaults()
            .UseHelpBuilder((context) => new EmuHelpBuilder(context.Console));

        /// <summary>
        /// Builds a parser the command line arguments for EMU.
        /// </summary>
        /// <returns>The CommandLineApplication object and a binding model of arguments.</returns>
        public static Parser BuildCommandLine([CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            // using var file = File.AppendText("F:\\Work\\GitHub\\emu\\log.txt");
            Console.WriteLine($"{DateTime.Now} Building command line {sourceFilePath}:{sourceLineNumber} {memberName}");
            Trace.WriteLine($"{DateTime.Now} Building command line {sourceFilePath}:{sourceLineNumber} {memberName}");
            return builtCommandLine ??= CreateCommandLine().Build();
        }

        private static IHostBuilder CreateHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args);
        }

        private static void BuildDependencies(IHostBuilder host)
        {
            host.ConfigureServices((services) =>
            {
                services
                .AddSingleton<OutputSink>()
                .AddSingleton<TextWriter>(OutputSink.Create)
                .AddSingleton<CsvSerializer>()
                .AddSingleton<JsonSerializer>()
                .AddSingleton<JsonLinesSerializer>()
                .AddSingleton<ToStringFormatter>()
                .AddTransient<IRecordFormatter>(OutputRecordWriter.FormatterResolver)
                .AddSingleton<Lazy<OutputFormat>>(
                    (provider) => new Lazy<OutputFormat>(() => provider.GetRequiredService<EmuGlobalOptions>().Format))
                .AddTransient<OutputRecordWriter>()

                //.AddTransient<DefaultFormatters>()
                .AddSingleton<IFileSystem>(_ => new FileSystem())
                .AddSingleton<FileMatcher>()
                .AddSingleton<FileUtilities>()
                .AddSingleton<FilenameSuggester>()
                .AddSingleton(provider => new FilenameParser(provider.GetRequiredService<IFileSystem>()));

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
            host.UseEmuCommand<RenameCommand, Rename>();
            host.UseEmuCommand<MetadataCommand, Metadata>();
            host.UseEmuCommand<VersionCommand, Version>();

            host.UseSerilog(ConfigureLogging);
        }

        private static void ConfigureLogging(HostBuilderContext context, LoggerConfiguration configuration)
        {
            // TODO: use model binding
            var parseResult = context.GetInvocationContext().ParseResult;

            var max = LogEventLevel.Fatal + 1;
            var level = max - (int)GetLogLevel(parseResult);

            //Log.Error("command result {0}, verbosity {1} / {2}", level, verbose, veryVerbose);

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
    }
}
