// <copyright file="IncorrectDataSize.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.FrontierLabs
{
    using System;
    using System.Buffers.Binary;
    using System.IO;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Emu.Audio.WAVE;
    using Emu.Utilities;
    using Range = Emu.Audio.RangeHelper.Range;

    public abstract partial class IncorrectDataSize : IFixOperation
    {
        private readonly IFileSystem fileSystem;

        public IncorrectDataSize(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public Task<CheckResult> CheckAffectedAsync(string file)
        {
            using var stream = (FileStream)this.fileSystem.File.OpenRead(file);

            var isWave = Wave.IsPcmWaveFile(stream);
            if (!isWave.IfFail(false))
            {
                return Task.FromResult(
                    new CheckResult(CheckStatus.NotApplicable, Severity.None, null));
            }

            var riffChunk = Wave.FindRiffChunk(stream);
            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var formatChunk = waveChunk.Bind(w => Wave.FindFormatChunk(stream, w));
            var dataChunk = waveChunk.Bind(w => Wave.FindDataChunk(stream, w, allowOutOfBounds: true));

            var badRiffLength = riffChunk.Bind<bool>(r => this.CheckIfRiffSizeBad(r, stream.Length)).IfFail(false);
            var badDataLength = dataChunk.Bind<bool>(d => this.CheckIfDataSizeBad(d, stream.Length)).IfFail(false);

            // this problem targets files produced by older firmwares
            // unfortunately this means there's no space in file
            // to mark our fix with a tag, nor read such a tag
            // here to report a repaired status
            var result = (badRiffLength && badDataLength) switch
            {
                true => new CheckResult(
                    CheckStatus.Affected,
                    Severity.Mild,
                    "RIFF length and data length are incorrect",
                    new ChunkData(riffChunk.ThrowIfFail(), formatChunk.ThrowIfFail(), dataChunk.ThrowIfFail())),
                false => new CheckResult(CheckStatus.Unaffected, Severity.None, null),
            };

            return Task.FromResult(result);
        }

        public abstract OperationInfo GetOperationInfo();

        public async Task<FixResult> ProcessFileAsync(string file, DryRun dryRun)
        {
            var affected = await this.CheckAffectedAsync(file);

            if (affected is { Status: CheckStatus.Affected })
            {
                using var stream = (FileStream)this.fileSystem.File.Open(file, FileMode.Open, dryRun.FileAccess);

                var message = this.ApplyFix(stream, (ChunkData)affected.Data, dryRun);

                return new FixResult(FixStatus.Fixed, affected, message, null);
            }
            else
            {
                return new FixResult(FixStatus.NoOperation, affected, null);
            }
        }

        protected abstract bool CheckIfDataSizeBad(Range dataRange, long streamLength);

        protected abstract bool CheckIfRiffSizeBad(Range riffRange, long streamLength);

        private string ApplyFix(Stream stream, ChunkData chunkData, DryRun dryRun)
        {
            var (riff, format, data) = chunkData;

            uint newRiffLength = (uint)stream.Length - Wave.MinimumRiffHeaderLength;

            // this invariant could be false if there were chunks after the start of data
            // but
            // FL files don't include additional chunks (for which this problem occurs)
            // and we have no way to detect such chunks without scanning for a known list of them.
            //
            // Note:
            // We are relying on strong check detection to prevent unnecessary modification of files that
            // do not match our problem criteria.
            uint newDataLength = (uint)(stream.Length - data.Start);

            // finally write the changes
            var position = stream.Seek(Wave.RiffLengthOffset, SeekOrigin.Begin);
            if (position != Wave.RiffLengthOffset)
            {
                throw new InvalidOperationException("Could not seek stream");
            }

            dryRun.WouldDo(
                $"update RIFF length to {newRiffLength}",
                () =>
                {
                    Span<byte> buffer = stackalloc byte[sizeof(uint)];

                    BinaryPrimitives.WriteUInt32LittleEndian(buffer, newRiffLength);

                    stream.Write(buffer);
                });

            var dataSizePos = data.Start - sizeof(uint);
            position = stream.Seek(dataSizePos, SeekOrigin.Begin);
            if (position != dataSizePos)
            {
                throw new InvalidOperationException("Could not seek stream");
            }

            dryRun.WouldDo(
                $"update data length to {newDataLength}",
                () =>
                {
                    Span<byte> buffer = stackalloc byte[sizeof(uint)];

                    BinaryPrimitives.WriteUInt32LittleEndian(buffer, newDataLength);

                    stream.Write(buffer);
                });

            return $"RIFF length set to {newRiffLength} (was {riff.Length}). data length set to {newDataLength} (was {data.Length})";
        }

        private record ChunkData(Range RiffChunk, Range FormatChunk, Range DataChunk);
    }
}
