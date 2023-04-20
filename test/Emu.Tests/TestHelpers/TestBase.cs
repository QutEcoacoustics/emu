// <copyright file="TestBase.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine.Parsing;
    using System.IO;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.IO.Pipelines;
    using System.Linq;
    using Divergic.Logging.Xunit;
    using Emu.Filenames;
    using Emu.Serialization;
    using Emu.Utilities;
    using FluentAssertions.Equivalency.Tracing;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    using Xunit.Abstractions;
    using static Emu.EmuCommand;
    using static Emu.Utilities.DryRun;

    public class TestBase : IDisposable
    {
        protected static readonly Func<string, string> NormalizePath = MockUnixSupport.Path;
        private readonly Parser cliParserValue;

        private readonly ITestOutputHelper xUnitOutput;
        private readonly bool realFileSystem;
        private readonly OutputFormat outputFormat;
        private readonly StringWriter cleanOutput;

        private readonly TestOutputHelperTextWriterAdapter xUnitOutputAdapter;
        private readonly TestOutputHelperITraceWriterAdapter xUnitTraceAdapter;
        private DryRunFactory dryRunFactory;

        public TestBase(ITestOutputHelper output)
            : this(output, false, OutputFormat.JSONL)
        {
        }

        public TestBase(ITestOutputHelper output, bool realFileSystem, OutputFormat outputFormat = OutputFormat.JSONL)
        {
            // parallel execution in xunit causes issues with services that assume they're running as singletons
            lock (this)
            {
                this.xUnitOutput = output ?? throw new ArgumentNullException(nameof(output));
                this.realFileSystem = realFileSystem;
                this.outputFormat = outputFormat;

                // allow writing out output to xunit log
                this.xUnitOutputAdapter ??= new(this.xUnitOutput);
                this.xUnitTraceAdapter ??= new(this.xUnitOutput);

                // also store a clean copy of the output for use in tests
                this.cleanOutput = new StringWriter();

                this.Sink = new MultiStreamWriter(this.xUnitOutputAdapter, this.cleanOutput);

                this.cliParserValue = new EmuEntry().BuildCommandLine();

                this.TestFiles = new MockFileSystem();

                // mock up an entire service stack
                var testServices = new ServiceCollection();
                var standardServices = EmuEntry.ConfigureServices(this.CurrentFileSystem);
                standardServices.Invoke(testServices);
                testServices.AddLogging(builder =>
                {
                    // send our logs to xunit log
                    builder.AddXunit(this.xUnitOutput);
                });

                // force JSONL output by default
                testServices.AddSingleton((_) => new Lazy<OutputFormat>(() => this.OutputFormat));
                testServices.AddSingleton<TextWriter>((_) => this.Sink);

                // force console width for tests to be wide enough that most lines don't wrap
                //  don't use int.max though - that will cause various full-width renderings to use int.max characters
                //  e.g. when drawing a table
                // also force no colors - most tests don't want to deal with ANSI codes
                // for any tests that do test fromatting we can override this provider
                this.xUnitOutput.WriteLine("WARN: [TestBase] AnsiConsoleFormatter width set to 512, and color support is disabled");
                testServices.AddSingleton(new AnsiConsoleFormatter(512, ColorSystemSupport.NoColors));

                this.ServiceProvider = testServices.BuildServiceProvider();
            }
        }

        public TextWriter Sink { get; }

        public TestOutputHelperTextWriterAdapter TestOutput => this.xUnitOutputAdapter;

        public ITraceWriter TestTraceOutput => this.xUnitTraceAdapter;

        public IFileSystem CurrentFileSystem => this.realFileSystem ? this.RealFileSystem : this.TestFiles;

        public MockFileSystem TestFiles { get; }

        public IFileSystem RealFileSystem => FixtureHelper.RealFileSystem;

        public List<ICacheLogger> Loggers { get; } = new();

        public ServiceProvider ServiceProvider { get; }

        public DryRunFactory DryRunFactory =>
            this.dryRunFactory ??= this.ServiceProvider.GetRequiredService<DryRunFactory>();

        public virtual OutputFormat OutputFormat => this.outputFormat;

        public string AllOutput => this.cleanOutput.ToString();

        public FilenameParser FilenameParser => new(
            this.TestFiles,
            this.ServiceProvider.GetRequiredService<FilenameGenerator>());

        public Parser CliParser => this.cliParserValue;

        public TextReader GetAllOutputReader() => new StringReader(this.AllOutput);

        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected ILogger<T> BuildLogger<T>()
        {
            var logger = this.xUnitOutput.BuildLoggerFor<T>(Microsoft.Extensions.Logging.LogLevel.Trace);
            this.Loggers.Add(logger);
            return logger;
        }

        protected OutputRecordWriter GetOutputRecordWriter()
        {
            return this.ServiceProvider.GetRequiredService<OutputRecordWriter>();
        }

        protected string ResolvePath(string path) => this.TestFiles.Path.GetFullPath(path);

        protected string[] ResolvePaths(params string[] paths) => paths.Map(this.TestFiles.Path.GetFullPath).ToArray();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var logger in this.Loggers)
                {
                    (logger as IDisposable)?.Dispose();
                }
            }
        }
    }
}
