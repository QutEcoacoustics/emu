// <copyright file="FileUtilities.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Utilities
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions;
    using System.Security.Cryptography;
    using MetadataUtility.Extensions.Microsoft.Extensions;
    using MetadataUtility.Models;
    using Microsoft.Extensions.Logging;

    public class FileUtilities
    {
        private readonly ILogger<FileUtilities> logger;
        private readonly IFileSystem fileSystem;

        public FileUtilities(ILogger<FileUtilities> logger, IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            this.logger = logger;
        }

        public async ValueTask<string> BackupAsync(string path, DryRun dryRun)
        {
            string dest;
            var count = 0;
            do
            {
                var suffix = ".bak" + (count > 0 ? count.ToString() : string.Empty);
                dest = path + suffix;
                count++;
            }
            while (this.fileSystem.File.Exists(dest));

            using (this.logger.Measure($"Copied file from {path} to {dest}"))
            {
                await dryRun.WouldDo(
                    $"back up file to {dest}",
                    async () =>
                    {
                        using var sourceStream = this.fileSystem.File.Open(path, FileMode.Open);
                        using var destinationStream = this.fileSystem.File.Create(dest);

                        await sourceStream.CopyToAsync(destinationStream);
                    },
                    () => Task.CompletedTask);
            }

            return dest;
        }

        [RequiresUnreferencedCode("HashAlgorithm")]
        public async ValueTask<Checksum> CalculateChecksum(string path, HashAlgorithmName hashName)
        {
            ArgumentNullException.ThrowIfNull(path, nameof(path));
            ArgumentNullException.ThrowIfNull(hashName, nameof(hashName));

#pragma warning disable CS8604 // Possible null reference argument.
            var algorithm = HashAlgorithm.Create(hashName.Name);
#pragma warning restore CS8604 // Possible null reference argument.

            if (algorithm is null)
            {
                throw new ArgumentNullException(nameof(hashName), "hash name not recognized");
            }

            using var stream = this.fileSystem.File.OpenRead(path);

            var hash = await algorithm.ComputeHashAsync(stream);

            return new Checksum() { Type = hashName.Name, Value = Convert.ToHexString(hash) };
        }
    }
}
