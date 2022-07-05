// <copyright file="Wave.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio
{
    using System.Buffers.Binary;
    using System.Text;
    using LanguageExt;
    using LanguageExt.Common;

    // http://soundfile.sapp.org/doc/WaveFormat/
    // http://www-mmsp.ece.mcgill.ca/Documents/AudioFormats/WAVE/WAVE.html
    // https://docs.microsoft.com/en-us/windows-hardware/drivers/audio/extensible-wave-format-descriptors
    // https://docs.microsoft.com/en-us/windows/desktop/api/mmreg/ns-mmreg-twaveformatex
    // https://tools.ietf.org/html/rfc2361
    // https://tools.ietf.org/html/draft-ema-vpim-wav-00
    // https://sites.google.com/site/musicgapi/technical-documents/wav-file-format#fact
    // https://web.archive.org/web/20081201144551/http://music.calarts.edu/~tre/PeakChunk.html
    // https://icculus.org/SDL_sound/downloads/external_documentation/wavecomp.htm
    // https://www.aelius.com/njh/wavemetatools/doc/riffmci.pdf
    public static class Wave
    {
        public const string Mime = "audio/wave";

        public const int MinimumRiffHeaderLength = 8;
        public const int FL005ErrorBytes = 44;

        public static readonly byte[] RiffMagicNumber = new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
        public static readonly byte[] WaveMagicNumber = new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' };
        public static readonly byte[] FormatChunkId = new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' };
        public static readonly byte[] DataChunkId = new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' };

        public static readonly Error FileTooShortRiff = Error.New("Error reading file: file is not long enough to have RIFF/WAVE header");
        public static readonly Error FileNotWave = Error.New("Error reading file: file is not a RIFF/WAVE file");
        public static readonly Error InvalidFileData = Error.New("Error reading file: no valid file data was found");
        public static readonly Error InvalidOffset = Error.New("Error reading file: an invalid offset was found");
        public static readonly Error InvalidChunk = Error.New("Error reading chunk: chunk size exceeds file size");

        public enum Format : ushort
        {
            Pcm = 1,

            Float = 3,

            Extensible = 0xFFFE,
        }

        /// <summary>
        /// Finds a RIFF header if present and returns the range of the RIFF chunk if found.
        /// </summary>
        /// <param name="stream">The file to read.</param>
        /// <returns>
        /// The range of the RIFF chunk if found, otherwise an error.
        /// The range offsets are relative to the start of the stream.
        /// </returns>
        public static Fin<RangeHelper.Range> FindRiffChunk(Stream stream)
        {
            if (stream.Length < MinimumRiffHeaderLength)
            {
                return FileTooShortRiff;
            }

            stream.Position = 0;
            Span<byte> buffer = stackalloc byte[MinimumRiffHeaderLength];
            var offset = stream.Read(buffer);

            if (offset != MinimumRiffHeaderLength)
            {
                return FileTooShortRiff;
            }

            if (!buffer.Slice(0, 4).SequenceEqual(RiffMagicNumber))
            {
                return InvalidFileData;
            }

            var length = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(4));

            return new RangeHelper.Range(offset, offset + length);
        }

        public static Fin<RangeHelper.Range> FindWaveChunk(Stream stream, RangeHelper.Range riffChunk)
        {
            if (riffChunk.Length < WaveMagicNumber.Length)
            {
                return FileTooShortRiff;
            }

            var offset = riffChunk.Start;
            var newOffset = stream.Seek(offset, SeekOrigin.Begin);
            if (newOffset != offset)
            {
                return InvalidOffset;
            }

            // read the first chunk type
            Span<byte> buffer = stackalloc byte[WaveMagicNumber.Length];

            var read = stream.Read(buffer);

            if (read != WaveMagicNumber.Length)
            {
                return FileTooShortRiff;
            }

            // advance our offset counter by the 4 bytes we just read
            offset += read;

            // check whether we found our target chunk or not
            if (!WaveMagicNumber.AsSpan().SequenceEqual(buffer))
            {
                // cannot process a non wave file
                return Error.New("Cannot process a non-WAVE RIFF file.");
            }

            return new RangeHelper.Range(offset, riffChunk.End);
        }

        public static Fin<RangeHelper.Range> FindFormatChunk(Stream stream, RangeHelper.Range waveChunk)
        {
            return ScanForChunk(stream, waveChunk, FormatChunkId);
        }

        public static Fin<RangeHelper.Range> FindDataChunk(Stream stream, RangeHelper.Range waveChunk)
        {
            return ScanForChunk(stream, waveChunk, DataChunkId);
        }

        public static Fin<bool> IsWaveFile(Stream stream)
        {
            var riffChunk = FindRiffChunk(stream);

            var waveChunk = riffChunk.Bind(r => FindWaveChunk(stream, r));

            return waveChunk.IsSucc;
        }

        public static Fin<bool> IsPcmWaveFile(Stream stream)
        {
            var riffChunk = FindRiffChunk(stream);

            var waveChunk = riffChunk.Bind(r => FindWaveChunk(stream, r));

            var formatChunk = waveChunk.Bind(w => Wave.FindFormatChunk(stream, w));
            if (formatChunk.IsFail)
            {
                return (Error)formatChunk;
            }

            var formatSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)formatChunk);

            var format = Wave.GetAudioFormat(formatSpan);

            return format == Format.Pcm;
        }

        /// <summary>
        /// Determines whether a given file stream exhibits behaviour of a preallocated header file.
        /// These files act like wave files, but don't have any meaningful data.
        /// There are certain traits that identify this problem, not every preallocated header file has them all.
        /// A scoring system is used to determine whether a file fits the criteria.
        /// If three or more faults are found, the file is deemed to have the problem.
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <returns>
        /// True for a preallocated header file, false if not.
        /// </returns>
        public static Fin<bool> IsPreallocatedHeader(Stream stream)
        {
            int faults = 0;

            var riffChunk = FindRiffChunk(stream);

            if (riffChunk.IsFail)
            {
                return (Error)riffChunk;
            }

            // If the riff chunk size is incorrect, increment faults
            if (((RangeHelper.Range)riffChunk).End != stream.Length)
            {
                faults++;
            }

            // If there are less than 200 bytes in the file, increment faults
            if (stream.Length < 200)
            {
                faults++;
            }

            // If there is a flac extension, increment faults
            if (((FileStream)stream).Name.Split(".").Last().Equals("flac"))
            {
                faults++;
            }

            var waveChunk = riffChunk.Bind(r => FindWaveChunk(stream, r));
            var dataChunk = waveChunk.Bind(w => FindDataChunk(stream, w));

            if (dataChunk.IsFail)
            {
                return (Error)dataChunk;
            }

            long dataStart = ((RangeHelper.Range)dataChunk).Start;
            long dataEnd = ((RangeHelper.Range)dataChunk).End;

            // If the data is less than 4 bytes or the first 4 bytes are 0, increment faults
            if (dataEnd - dataStart < 4)
            {
                faults++;
            }
            else
            {
                Span<byte> dataBuffer = stackalloc byte[4];
                long position = stream.Seek(dataStart, SeekOrigin.Begin);

                if (position == dataStart)
                {
                    stream.Read(dataBuffer);

                    if (BinaryPrimitives.ReadInt32BigEndian(dataBuffer) == 0)
                    {
                        faults++;
                    }
                }
            }

            // If the data section is longer than the stream, increment faults
            if (dataEnd > stream.Length)
            {
                faults++;
            }

            // Return true if at least 3 pre-allocated header faults are found
            return faults >= 3;
        }

        public static uint GetSampleRate(ReadOnlySpan<byte> formatChunk)
        {
            const int sampleRateOffset = 4;
            uint sampleRate = BinaryPrimitives.ReadUInt32LittleEndian(formatChunk[sampleRateOffset..]);

            return sampleRate;
        }

        public static Format GetAudioFormat(ReadOnlySpan<byte> formatChunk)
        {
            const int audioFormatOffset = 0;
            ushort audioFormat = BinaryPrimitives.ReadUInt16LittleEndian(formatChunk[audioFormatOffset..]);

            return (Format)audioFormat;
        }

        public static ushort GetChannels(ReadOnlySpan<byte> formatChunk)
        {
            const int channelsOffset = 2;
            ushort channels = BinaryPrimitives.ReadUInt16LittleEndian(formatChunk[channelsOffset..]);

            return channels;
        }

        public static uint GetByteRate(ReadOnlySpan<byte> formatChunk)
        {
            const int byteRateOffset = 8;
            uint byteRate = BinaryPrimitives.ReadUInt32LittleEndian(formatChunk[byteRateOffset..]);

            return byteRate;
        }

        public static ushort GetBlockAlign(ReadOnlySpan<byte> formatChunk)
        {
            const int blockAlignOffset = 12;
            ushort blockAlign = BinaryPrimitives.ReadUInt16LittleEndian(formatChunk[blockAlignOffset..]);

            return blockAlign;
        }

        public static ushort GetBitsPerSample(ReadOnlySpan<byte> formatChunk)
        {
            const int bitsPerSampleOffset = 14;
            ushort bitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian(formatChunk[bitsPerSampleOffset..]);

            return bitsPerSample;
        }

        public static ulong GetTotalSamples(RangeHelper.Range dataChunk, ushort channels, ushort bitsPerSample)
        {
            // size of the data chunk
            var length = (ulong)dataChunk.Length;

            return length / (ulong)(channels * (bitsPerSample / 8));
        }

        public static int FL005Patch(int length) => length - FL005ErrorBytes;

        /// <summary>
        /// Scans a container (a range of bytes) for a sub-chunk with the given chunk ID.
        /// The target chunk may be in any position within it's siblings.
        /// Best case: only 8 bytes are ready from the stream (i.e. the target chunk is the first in the container).
        /// Worst case: numSubChunks * 8 bytes are read from the stream (i.e. the target chunk is at the last one in the container).
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="container">The subset of the stream to read from.</param>
        /// <param name="targetChunkId">The target chunk to look for.</param>
        /// <returns>An error if the chunk was not found, or a Range of the target chunk if it was found.</returns>
        public static Fin<RangeHelper.Range> ScanForChunk(Stream stream, RangeHelper.Range container, ReadOnlySpan<byte> targetChunkId)
        {
            const int ChunkIdLength = 4;
            const int ChunkLengthLength = 4;

            /*
            Check to ensure the given container range fits within the bounds of the file.
            Commented out for now since this check can cause files affected by FL005 to crash.
            https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL005.md

            if (container.End > stream.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(container), "container.End must be less than or equal to stream.Length");
            }
            */

            // check if the container is long enough to contain the chunk
            if (stream.Length < (ChunkIdLength + ChunkLengthLength + container.Start))
            {
                return FileTooShort(targetChunkId);
            }

            var offset = container.Start;
            Span<byte> buffer = stackalloc byte[ChunkIdLength + ChunkLengthLength];

            while (offset < container.End)
            {
                // seek to the start of the nth child in the container
                var newOffset = stream.Seek(offset, SeekOrigin.Begin);
                if (newOffset != offset)
                {
                    return InvalidOffset;
                }

                // read the chunk id and it's size
                var read = stream.Read(buffer);

                if (read != (ChunkIdLength + ChunkLengthLength))
                {
                    return ChunkNotFound(targetChunkId);
                }

                var chunkId = buffer[..ChunkIdLength];
                var length = BinaryPrimitives.ReadInt32LittleEndian(buffer[ChunkIdLength..]);

                // advance our offset counter by the 8 bytes we just read
                offset += read;

                // check the chunk length falls within the bounds of the file
                if (offset + length > stream.Length)
                {
                    // FL005 is detected - decrement length by 44 for an accurate value
                    // TODO: Fix this problem rather than cover it up
                    if (offset + length - stream.Length == FL005ErrorBytes)
                    {
                        length = FL005Patch(length);
                    }
                    else
                    {
                        return InvalidChunk;
                    }
                }

                // check whether we found our target chunk or not
                if (targetChunkId.SequenceEqual(chunkId))
                {
                    // success, stop here and return the range of the chunk
                    return new RangeHelper.Range(offset, offset + length);
                }

                // advance our offset counter by the length of the chunk to look for the next sibling
                offset += length;
            }

            return ChunkNotFound(targetChunkId);
        }

        private static Error FileTooShort(ReadOnlySpan<byte> chunkName) =>
            Error.New($"Error reading file: file is not long enough to have a {Encoding.ASCII.GetString(chunkName)} header");

        private static Error ChunkNotFound(ReadOnlySpan<byte> chunkName) =>
            Error.New($"Error reading file: a {Encoding.ASCII.GetString(chunkName)} chunk was not found");
    }
}
