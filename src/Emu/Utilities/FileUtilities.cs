// <copyright file="FileUtilities.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Utilities
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Numerics;
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
            var newPath = this.fileSystem.Path.Combine(this.fileSystem.Path.GetDirectoryName(path)!, newBasename);
            using (this.logger.Measure($"Renamed file from {path} to {newPath}"))
            {
                dryRun.WouldDo(
                    $"rename file to {newPath}",
                    () => this.fileSystem.File.Move(path, newPath, overwrite: false));
            }

            return newPath;
        }

        public long Truncate(Stream stream, long newLength, DryRun dryRun)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            if (newLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newLength), "Cannot truncate a file to a negative length");
            }

            if (newLength > stream.Length)
            {
                throw new ArgumentOutOfRangeException($"Cannot truncate a file when new length ({newLength}) is longer than current length ({stream.Length})");
            }

            dryRun.WouldDo(
                $"Truncated file to {newLength} bytes (was {stream.Length} bytes)",
                () =>
                {
                    if (!(stream.CanWrite && stream.CanSeek))
                    {
                        throw new NotSupportedException("Stream must support writing and seeking for truncation");
                    }

                    stream.SetLength(newLength);
                });

            return stream.Length;
        }

        public async Task TruncateSplitAsync(Stream source, Stream destination, long splitPoint, DryRun dryRun)
        {
            if (splitPoint < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(splitPoint), "Cannot truncate a file to a negative length");
            }

            if (splitPoint > source.Length)
            {
                throw new ArgumentOutOfRangeException($"Cannot truncate a file when new length ({splitPoint}) is longer than current length ({source.Length})");
            }

            await dryRun.WouldDoAsync(
                $"Split file at {splitPoint} bytes (was {source.Length} bytes)",
                DoIt);

            async Task DoIt()
            {
                CheckStream(source, nameof(source));
                CheckStream(destination, nameof(destination));

                source.Position = splitPoint;
                Debug.Assert(source.Position == splitPoint, "Position should be set");

                await source.CopyToAsync(destination);

                await destination.FlushAsync();

                source.SetLength(splitPoint);
            }

            void CheckStream(Stream stream, string name)
            {
                ArgumentNullException.ThrowIfNull(stream, nameof(name));

                if (!(stream.CanWrite && stream.CanSeek))
                {
                    throw new NotSupportedException("Stream must support writing and seeking for truncation");
                }
            }
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

            if (stream.Position != 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var hash = await hasher.ComputeHashAsync(stream);

            return new Checksum() { Type = HashAlgorithmName.SHA256.Name, Value = hash.ToHexString() };
        }

        public async ValueTask<bool> CheckForContinuousValue(Stream stream, long offset = 0, long? count = null, Vector<byte> target = default)
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
            var end = count switch
            {
                null => stream.Length,
                long i => Math.Min(stream.Length, position + i),
            };

            while (position < end)
            {
                // the buffer could potentially be filled beyond the end of the range we're scanning
                // if so only read up to the end of our limit
                var limit = Math.Min(buffer.Length, (int)(end - position));
                int read = await stream.ReadAsync(buffer.AsMemory(0, limit));
                position += read;

                if (read > end)
                {
                    read = (int)end;
                }

                for (int i = 0; i < read; i += Vector<byte>.Count)
                {
                    int upper = Math.Min(read - i, Vector<byte>.Count);
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
