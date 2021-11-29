// <copyright file="DryRun.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Utilities
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Logging;

    public class DryRun : IDisposable
    {
        private readonly bool isDryRun;
        private readonly ILogger<DryRun> logger;

        public DryRun(bool isDryRun, ILogger<DryRun> logger)
        {
            this.isDryRun = isDryRun;
            this.logger = logger;
        }

        public bool IsDryRun => this.isDryRun;

        public FileAccess FileAccess => this.isDryRun ? FileAccess.Read : FileAccess.ReadWrite;

        public T WouldDo<T>(string message, Func<T> callback, Func<T> dryCallback = default)
        {
            if (this.IsDryRun)
            {
                using var _ = this.logger.BeginScope("dry run would");
                this.logger.LogInformation(message);
                return dryCallback is null ? default : dryCallback();
            }
            else
            {
                return callback();
            }
        }

        public void WouldDo(string message, Action callback, Action dryCallback = default)
        {
            if (this.IsDryRun)
            {
                using var _ = this.logger.BeginScope("dry run would");
                this.logger.LogInformation(message);
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

        public void Dispose()
        {
            if (this.isDryRun)
            {
                this.logger.LogInformation("This was a dry run, no changes were made");
            }
        }
    }
}
