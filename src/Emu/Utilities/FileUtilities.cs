// <copyright file="FileUtilities.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Utilities
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions;
    using System.Numerics;
    using System.Runtime.Intrinsics.X86;
    using System.Security.Cryptography;
    using Emu.Extensions.Microsoft.Extensions;
    using Emu.Models;
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

        public string Rename(string path, string newBasename, DryRun dryRun)
        {
            var newPath = this.fileSystem.Path.Combine(this.fileSystem.Path.GetDirectoryName(path), newBasename);
            using (this.logger.Measure($"Renamed file from {path} to {newPath}"))
            {
                dryRun.WouldDo(
                    $"rename file to {newPath}",
                    () => this.fileSystem.File.Move(path, newPath, overwrite: false));
            }

            return dryRun.IsDryRun ? path : newPath;
        }

        public async ValueTask<Checksum> CalculateChecksumSha256(string path)
        {
            ArgumentNullException.ThrowIfNull(path, nameof(path));

            using var stream = this.fileSystem.File.OpenRead(path);
            return await this.CalculateChecksumSha256(stream);
        }

        public async ValueTask<Checksum> CalculateChecksumSha256(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            using var hasher = SHA256.Create();

            var hash = await hasher.ComputeHashAsync(stream);

            return new Checksum() { Type = HashAlgorithmName.SHA256.Name, Value = hash.ToHexString() };
        }

        public async ValueTask<bool> CheckForContinuousValue(Stream stream, int offset = 0, int? count = null, Vector<byte> target = default)
        {
            if (offset < 0)
            {
                throw new ArgumentException("must be greater than 0", nameof(offset));
            }

            if (count is not null and < 0)
            {
                throw new ArgumentException("must be greater than 0", nameof(count));
            }

            var position = stream.Seek(offset, SeekOrigin.Begin);
            if (position != offset)
            {
                throw new InvalidOperationException("Cannot seek to offset");
            }

            var buffer = new byte[4096];
            var end = count is null ? stream.Length : position + count;

            while (position < end)
            {
                // the buffer could potentially be filled beyond the end of the range we're scanning
                // if so only read up to the end of our limit
                var limit = Math.Min(buffer.Length, (int)(end - position));
                int read = await stream.ReadAsync(buffer, 0, limit);
                position += read;

                if (read > end)
                {
                    read = (int)end;
                }

                for (int i = 0; i < read; i += Vector<byte>.Count)
                {
                    var upper = Math.Min(read - i, Vector<byte>.Count);
                    var v = new Vector<byte>(buffer.AsSpan(i, upper));

                    var equal = v == target;
                    if (!equal)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
