// <copyright file="Wamd.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio
{
    using System.Buffers.Binary;
    using System.Text;
    using LanguageExt;
    using LanguageExt.Common;

    public static class Wamd
    {
        public const string Mime = "audio/wave";

        public const int MinimumRiffHeaderLength = 8;

        public static readonly byte[] RiffMagicNumber = new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
        public static readonly byte[] WaveMagicNumber = new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' };
        public static readonly byte[] FormatChunkId = new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' };
        public static readonly byte[] DataChunkId = new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' };
        public static readonly byte[] WamdChunkId = new byte[] { (byte)'w', (byte)'a', (byte)'m', (byte)'d' };

        public static readonly Error FileTooShortRiff = Error.New("Error reading file: file is not long enough to have RIFF/WAVE header");
        public static readonly Error FileNotWave = Error.New("Error reading file: file is not a RIFF/WAVE file");

        public static readonly Error InvalidFileData = Error.New("Error reading file: no valid file data was found");

        public static readonly Error InvalidOffset = Error.New("Error reading file: an invalid offset was found");

        public static Fin<bool> IsWildlifeAcousticsWaveFile(Stream stream)
        {
            //Get the "wamd" chunk
            var wamdChunk = FindWamdChunk(stream);

            //If a "wamd" chunk is present, then the file is a Wildlife Acoustics file
            if (wamdChunk.IsSucc)
            {
                return true;
            }

            return InvalidFileData;
        }

        public static Fin<Range> FindRiffChunk(Stream stream)
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

            return new Range(offset, offset + length);
        }

        public static Fin<Range> FindWaveChunk(Stream stream, Range riffChunk)
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

            return new Range(offset, riffChunk.End);
        }

        public static Fin<Range> FindWamdChunk(Stream stream)
        {
            var riffChunk = FindRiffChunk(stream);
            var waveChunk = FindWaveChunk(stream, (Wamd.Range)riffChunk);

            return ScanForChunk(stream, (Wamd.Range)waveChunk, WamdChunkId);
        }

        public static ReadOnlySpan<byte> ReadWamdChunk(Stream stream, Range wamdRange)
        {
            Span<byte> buffer = new byte[wamdRange.Length];

            if (stream.Seek(wamdRange.Start, SeekOrigin.Begin) != wamdRange.Start)
            {
                throw new IOException("ReadRange: could not seek to position");
            }

            var read = stream.Read(buffer);

            if (read != wamdRange.Length)
            {
                throw new InvalidOperationException("ReadRange: read != range.Length");
            }

            return buffer;
        }

        public static ushort GetVersion(ReadOnlySpan<byte> wamdChunk)
        {
            const int versionDataOffset = 6;
            ushort versionData = BinaryPrimitives.ReadUInt16LittleEndian(wamdChunk[versionDataOffset..]);

            return versionData;
        }

        public static ushort GetSubchunkId(ReadOnlySpan<byte> wamdChunk)
        {
            const int idOffset = 0;
            ushort id = BinaryPrimitives.ReadUInt16LittleEndian(wamdChunk[idOffset..]);

            return id;
        }

        private static Error FileTooShort(ReadOnlySpan<byte> chunkName) =>
            Error.New($"Error reading file: file is not long enough to have a {Encoding.ASCII.GetString(chunkName)} header");

        private static Error ChunkNotFound(ReadOnlySpan<byte> chunkName) =>
            Error.New($"Error reading file: a {Encoding.ASCII.GetString(chunkName)} chunk was not found");

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
        private static Fin<Range> ScanForChunk(Stream stream, Range container, ReadOnlySpan<byte> targetChunkId)
        {
            const int ChunkIdLength = 4;
            const int ChunkLengthLength = 4;

            if (container.End > stream.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(container), "container.End must be less than or equal to stream.Length");
            }

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

                // check whether we found our target chunk or not
                if (targetChunkId.SequenceEqual(chunkId))
                {
                    // success, stop here and return the range of the chunk
                    return new Range(offset, offset + length);
                }

                // advance our offset counter by the length of the chunk to look for the next sibling
                offset += length;
            }

            return ChunkNotFound(targetChunkId);
        }

        public partial record Range(long Start, long End);

        public partial record Range
        {
            public long Length => this.End - this.Start;
        }
    }
}
