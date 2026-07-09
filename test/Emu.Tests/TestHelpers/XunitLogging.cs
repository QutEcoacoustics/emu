// <copyright file="XunitLogging.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public record LogEntry(LogLevel Level, string Message, Exception Exception);

    public interface ICacheLogger : ILogger, IDisposable
    {
        IReadOnlyList<LogEntry> Entries { get; }
    }

    public interface ICacheLogger<T> : ICacheLogger, ILogger<T>
    {
    }

    public class CacheLogger<T> : ICacheLogger<T>
    {
        private readonly ITestOutputHelper output;
        private readonly List<LogEntry> entries = new();

        public CacheLogger(ITestOutputHelper output)
        {
            this.output = output;
        }

        public IReadOnlyList<LogEntry> Entries => this.entries;

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            this.entries.Add(new LogEntry(logLevel, message, exception));

            try
            {
                this.output.WriteLine($"[{logLevel}] {message}");
                if (exception != null)
                {
                    this.output.WriteLine(exception.ToString());
                }
            }
            catch (InvalidOperationException)
            {
                // test output helper may be disposed
            }
        }

        public void Dispose()
        {
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }

    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper output;

        public XunitLoggerProvider(ITestOutputHelper output)
        {
            this.output = output;
        }

        public ILogger CreateLogger(string categoryName) => new CacheLogger<object>(this.output);

        public void Dispose()
        {
        }
    }

    public static class XunitLoggingExtensions
    {
        public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, ITestOutputHelper output)
        {
            builder.AddProvider(new XunitLoggerProvider(output));
            return builder;
        }

        public static ICacheLogger<T> BuildLoggerFor<T>(this ITestOutputHelper output, LogLevel minLevel = LogLevel.Trace)
        {
            return new CacheLogger<T>(output);
        }
    }
}
