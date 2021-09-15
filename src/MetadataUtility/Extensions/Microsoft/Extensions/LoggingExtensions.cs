// <copyright file="LoggingExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Extensions.Microsoft.Extensions
{
    using global::Microsoft.Extensions.Logging;
    using global::System;
    using global::System.Diagnostics;

    public static class LoggingExtensions
    {
        public static MeasureStopWatch<T> Measure<T>(this ILogger<T> logger, string name, LogLevel level = LogLevel.Debug)
        {
            return new MeasureStopWatch<T>(logger, name, level);
        }

        public sealed class MeasureStopWatch<T> : IDisposable
        {
            private readonly Stopwatch stopWatch;
            private readonly ILogger<T> logger;
            private readonly string name;
            private readonly LogLevel level;

            public MeasureStopWatch(ILogger<T> logger, string name, LogLevel level)
            {
                this.name = name;
                this.level = level;
                this.stopWatch = Stopwatch.StartNew();
                this.logger = logger;
            }

            public void Dispose()
            {
                this.stopWatch.Stop();
                this.logger.Log(this.level, "{name} took {time}", this.name, this.stopWatch.Elapsed);
            }

            public TimeSpan Stop()
            {
                this.stopWatch.Stop();
                return this.stopWatch.Elapsed;
            }
        }
    }
}
