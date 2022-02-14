// <copyright file="TestBase.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine.Parsing;
    using System.IO;
    using System.IO.Abstractions.TestingHelpers;
    using System.Linq;
    using Divergic.Logging.Xunit;
    using LanguageExt;
    using MetadataUtility.Filenames;
    using MetadataUtility.Serialization;
    using MetadataUtility.Utilities;
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;

    public class TestBase : IDisposable
    {
        protected static readonly Func<string, string> NormalizePath = MockUnixSupport.Path;
        private static readonly Parser CliParserValue;

        private readonly ITestOutputHelper output;
        private ILogger<DryRun> dryRunLogger;
        private OutputRecordWriter outputRecordWriter;
        private TestOutputHelperTextWriterAdapter consoleOut;

        static TestBase()
        {
            CliParserValue = EmuEntry.BuildCommandLine();
        }

        public TestBase(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
            this.TestFiles = new MockFileSystem();
        }

        public MockFileSystem TestFiles { get; }

        public List<ICacheLogger> Loggers { get; } = new();

        public ILogger<DryRun> DryRunLogger =>
            this.dryRunLogger ??= this.BuildLogger<DryRun>();

        public TextWriter ConsoleOut => this.consoleOut ??= new(this.output);

        public FilenameParser FilenameParser => new(this.TestFiles);

        public Parser CliParser => CliParserValue;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected ILogger<T> BuildLogger<T>()
        {
            var logger = this.output.BuildLoggerFor<T>(LogLevel.Trace);
            this.Loggers.Add(logger);
            return logger;
        }

        protected OutputRecordWriter GetOutputRecordWriter()
        {
            return this.outputRecordWriter ??= new OutputRecordWriter(
                this.ConsoleOut,
                new ToStringFormatter(this.BuildLogger<ToStringFormatter>()));
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
