// <copyright file="IncorrectDataSize.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.FrontierLabs
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Audio.WAVE;
    using Emu.Utilities;
    using Range = Emu.Audio.RangeHelper.Range;

    public class IncorrectDataSize : IFixOperation
    {
        public const int ErrorAmount = 44;

        private readonly IFileSystem fileSystem;

        public IncorrectDataSize(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public static OperationInfo Metadata => new(
            WellKnownProblems.FrontierLabsProblems.IncorrectSubChunk2,
            Fixable: true,
            Safe: true,
            Automatic: true,
            typeof(IncorrectDataSize));

        public Task<CheckResult> CheckAffectedAsync(string file)
        {
            using var stream = (FileStream)this.fileSystem.File.OpenRead(file);

            var isWave = Wave.IsPcmWaveFile(stream);
            if (!isWave.IfFail(false))
            {
                return Task.FromResult(
                    new CheckResult(CheckStatus.NotApplicable, Severity.None, null));
            }

            // the first 8 bytes aren't counted in the total
            var expectedRiffLength = stream.Length - Wave.MinimumRiffHeaderLength;

            var riffChunk = Wave.FindRiffChunk(stream);
            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var formatChunk = waveChunk.Bind(w => Wave.FindFormatChunk(stream, w));
            var dataChunk = waveChunk.Bind(w => Wave.FindDataChunk(stream, w, allowOutOfBounds: true));

            // we're affected if the RIFF header is 44 bytes off
            // or if the data header is 44 bytes off.
            var badRiffLength = riffChunk.Bind<bool>(r => (r.Length - expectedRiffLength) == ErrorAmount).IfFail(false);
            var badDataLength = dataChunk.Bind<bool>(d => (d.End - stream.Length) == ErrorAmount).IfFail(false);

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

        public OperationInfo GetOperationInfo() => Metadata;

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

        public Range ModifyDataRange(Range range)
        {
            return range with
            {
                End = range.End - ErrorAmount,
            };
        }

        private string ApplyFix(Stream stream, ChunkData chunkData, DryRun dryRun)
        {
            var (riff, format, data) = chunkData;

            uint newRiffLength = (uint)stream.Length - Wave.MinimumRiffHeaderLength;
            uint newDataLength = (uint)data.Length - ErrorAmount;

            // sanity check

            //var formatData = ReadRange(stream, format);
            //var channels = Wave.GetChannels(formatData);
            //var bytesPerSample = Wave.GetBitsPerSample(formatData) / 8;
            var expectedDataLength = stream.Length - data.Start;
            Debug.Assert(newDataLength == expectedDataLength, "Amount remaining should be accurate");

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
