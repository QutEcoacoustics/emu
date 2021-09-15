// <copyright file="FileUtilities.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Utilities
{
    using System.Security.Cryptography;
    using MetadataUtility.Extensions.Microsoft.Extensions;
    using MetadataUtility.Models;
    using Microsoft.Extensions.Logging;

    public class FileUtilities
    {
        private readonly ILogger<FileUtilities> logger;

        public FileUtilities(ILogger<FileUtilities> logger)
        {
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
            while (File.Exists(dest));

            using var _ = this.logger.Measure($"Copied file from {path} to {dest}");

            await dryRun.WouldDo(
                $"back up file to {dest}",
                async () =>
                {
                    using var sourceStream = File.Open(path, FileMode.Open);
                    using var destinationStream = File.Create(dest);

                    await sourceStream.CopyToAsync(destinationStream);
                },
                () => Task.CompletedTask);

            return dest;
        }

        public static async ValueTask<Checksum> CalculateChecksum(string path, HashAlgorithmName hashName)
        {
            var algorithm = HashAlgorithm.Create(hashName.Name);

            if (algorithm is null)
            {
                throw new ArgumentNullException(nameof(hashName), "hash name not recognised");
            }

            using var stream = File.OpenRead(path);

            var hash = await algorithm.ComputeHashAsync(stream);

            return new Checksum() { Type = hashName.Name, Value = Convert.ToHexString(hash) };
        }
    }
}
