// <copyright file="DryRun.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Utilities
{
    using System;
    using System.IO;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class DryRun : IDisposable
    {
        public const string LogCategoryName = "DryRunLogger";

        public static readonly Func<IServiceProvider, DryRunFactory> Factory = (provider) =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(LogCategoryName);
            return (isDryRun) => new DryRun(isDryRun, logger);
        };

        private readonly bool isDryRun;
        private readonly ILogger logger;

        private DryRun(bool isDryRun, ILogger logger)
        {
            this.isDryRun = isDryRun;
            this.logger = logger;
        }

        public delegate DryRun DryRunFactory(bool isDryRun);

        public bool IsDryRun => this.isDryRun;

        public FileAccess FileAccess => this.isDryRun ? FileAccess.Read : FileAccess.ReadWrite;

        public T WouldDo<T>(string message, Func<T> callback, Func<T> dryCallback = default)
        {
            if (this.IsDryRun)
            {
                this.logger.LogInformation("would {message}", message);
                return dryCallback is null ? default : dryCallback();
            }
            else
            {
                return callback();
            }
        }

        public async ValueTask<T> WouldDoAsync<T>(string message, Func<Task<T>> callback, Func<Task<T>> dryCallback = default)
        {
            if (this.IsDryRun)
            {
                this.logger.LogInformation("would {message}", message);
                if (dryCallback is null)
                {
                    return default;
                }
                else
                {
                    return await dryCallback();
                }
            }
            else
            {
                return await callback();
            }
        }

        public void WouldDo(string message, Action callback, Action dryCallback = default)
        {
            if (this.IsDryRun)
            {
                this.logger.LogInformation("would {message}", message);
                if (dryCallback is not null)
                {
                    dryCallback();
                }
            }
            else
            {
                callback();
            }
        }

        public async ValueTask WouldDoAsync(string message, Func<Task> callback, Func<Task> dryCallback = default)
        {
            if (this.IsDryRun)
            {
                this.logger.LogInformation("would {message}", message);
                if (dryCallback is not null)
                {
                    await dryCallback();
                }
            }
            else
            {
                await callback();
            }
        }

        public void Dispose()
        {
            if (this.isDryRun)
            {
                this.logger.LogInformation("This was a dry run, no changes were made");
            }
        }
    }
}
