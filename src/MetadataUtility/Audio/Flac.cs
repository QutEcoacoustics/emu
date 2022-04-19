// <copyright file="Flac.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio
{
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Text;
    using LanguageExt;
    using LanguageExt.Common;
    using MetadataUtility.Utilities;

    public static class Flac
    {
        public const string Mime = "audio/flac";
        public const int FlacSamplesOffset = 21;
        public const int SampleRateOffset = 18;
        public const int ChannelOffset = 20;
        public const int BlockTypeOffset = 4;
        public const int MetadataBlockSize = 42;
        public const int VorbisCommentBlockNumber = 4;
        public static readonly byte[] FlacMagicNumber = new byte[] { (byte)'f', (byte)'L', (byte)'a', (byte)'C' };

        public static readonly Error FileTooShort = Error.New("Error reading file: file is not long enough to have a duration header");
        public static readonly Error FileTooShortFlac = Error.New("Error reading file: file is not long enough to have a fLaC header");
        public static readonly Error VendorStringNotFound = Error.New("Error reading file: could not find vendor string Frontier Labs in file header");
        public static readonly Error InvalidOffset = Error.New("Error reading file: an invalid offset was found");
        public static readonly Func<byte, Error> ChunkNotFound = x => Error.New($"Chunk with ID `{x}` was not found");

        /// <summary>
        /// The total samples in the stream are read from the flac header here.
        /// This can be extracted from bits 173-208 of the stream.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The total samples.</returns>
        public static Fin<ulong> ReadTotalSamples(Stream stream)
        {
            long position = stream.Seek(FlacSamplesOffset, SeekOrigin.Begin);
            Debug.Assert(position == 21, $"Expected stream.Seek position to return 21, instead returned {position}");

            Span<byte> buffer = stackalloc byte[5];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return BinaryHelpers.Read36BitUnsignedBigEndianIgnoringFirstNibble(buffer);
        }

        /// <summary>
        /// The sample rate is read from the flac header here.
        /// This can be extracted from bits 145-164 of the stream.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The sample rate.</returns>
        public static Fin<uint> ReadSampleRate(Stream stream)
        {
            long position = stream.Seek(SampleRateOffset, SeekOrigin.Begin);
            Debug.Assert(position == 18, $"Expected stream.Seek position to return 18, instead returned {position}");

            Span<byte> buffer = stackalloc byte[3];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return BinaryHelpers.Read20BitUnsignedBigEndianIgnoringLastNibble(buffer);
        }

        /// <summary>
        /// The number of channels is read from the flac header here.
        /// This can be extracted from bits 165-167 of the stream.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The number of channels.</returns>
        public static Fin<byte> ReadNumChannels(Stream stream)
        {
            long position = stream.Seek(ChannelOffset, SeekOrigin.Begin);
            Debug.Assert(position == 20, $"Expected stream.Seek position to return 20, instead returned {position}");

            Span<byte> buffer = stackalloc byte[1];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return (byte)(BinaryHelpers.Read3BitUnsignedBigEndianIgnoringFirstFourAndLastBit(buffer) + 1);
        }

        /// <summary>
        /// The bit depth is read from the flac header here.
        /// This can be extracted from bits 168-172 of the stream.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The bit depth.</returns>
        public static Fin<byte> ReadBitDepth(Stream stream)
        {
            long position = stream.Seek(ChannelOffset, SeekOrigin.Begin);
            Debug.Assert(position == 20, $"Expected stream.Seek position to return 20, instead returned {position}");

            stream.Seek(ChannelOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[2];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return (byte)(BinaryHelpers.Read5BitUnsignedBigEndianIgnoringFirstSevenAndLastFourBits(buffer) + 1);
        }

        public static Fin<Unit> WriteTotalSamples(FileStream stream, ulong sampleCount)
        {
            stream.Seek(FlacSamplesOffset, SeekOrigin.Begin);

            // we're writing over the top of other bits, so read first
            Span<byte> buffer = stackalloc byte[5];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            // flac files have an unsigned 36-bit integer for total sample duration!
            BinaryHelpers.Write36BitUnsignedBigEndianIgnoringFirstNibble(buffer, sampleCount);

            stream.Seek(FlacSamplesOffset, SeekOrigin.Begin);
            stream.Write(buffer);

            return Unit.Default;
        }

        /// <summary>
        /// Determines whether a file is a FLAC file.
        /// Checks for indicator "fLaC" at the beginning of each FLAC file.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>Boolean indicating whether the file is a flac file.</returns>
        public static Fin<bool> IsFlacFile(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[4];

            stream.Seek(0, SeekOrigin.Begin);
            var read = stream.Read(buffer);

            if (read != FlacMagicNumber.Length)
            {
                return FileTooShortFlac;
            }

            return buffer.StartsWith(FlacMagicNumber);
        }

        /// <summary>
        /// Determines whether a file header has and is large enough to store a FLAC metadata block.
        /// The block type is stored in bits 33-39 of the file stream, the first is of type "STREAMINFO".
        /// This block type corresponds to a value of 0.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>Boolean indicating whether the file has a valid metadata block.</returns>
        public static Fin<bool> HasMetadataBlock(Stream stream)
        {
            long position = stream.Seek(BlockTypeOffset, SeekOrigin.Begin);
            Debug.Assert(position == 4, $"Expected stream.Seek position to return 4, instead returned {position}");

            Span<byte> buffer = stackalloc byte[1];
            var bytesRead = stream.Read(buffer);

            if (bytesRead < buffer.Length)
            {
                return FileTooShortFlac;
            }

            return stream.Length > MetadataBlockSize && BinaryHelpers.Read7BitUnsignedBigEndianIgnoringFirstBit(buffer) == 0;
        }

        /// <summary>
        /// Scans a FLAC file for a specific block.
        /// Blocks are identified by unique byte.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <param name="targetBlockType">The block being searched for.</param>
        /// <returns>Range indicating the position of the block in the file stream.</returns>
        public static Fin<Range> ScanForChunk(Stream stream, byte targetBlockType)
        {
            const int BlockTypeLength = 1, BlockLengthLength = 3;

            int offset = BlockTypeOffset;
            uint length = 0;
            byte blockType;
            bool lastBlock = false;

            Span<byte> buffer = stackalloc byte[BlockTypeLength + BlockLengthLength];

            while (!lastBlock)
            {
                var newOffset = stream.Seek(offset, SeekOrigin.Begin);

                if (newOffset != offset)
                {
                    return InvalidOffset;
                }

                var read = stream.Read(buffer);

                if (read != (BlockTypeLength + BlockLengthLength))
                {
                    return ChunkNotFound(targetBlockType);
                }

                lastBlock = (buffer[0] >> 7) == 1;
                blockType = BinaryHelpers.Read7BitUnsignedBigEndianIgnoringFirstBit(buffer[..BlockTypeLength]);
                length = BinaryHelpers.Read24bitUnsignedBigEndian(buffer[BlockTypeLength..]);
                offset += read;

                if (blockType == targetBlockType)
                {
                    return new Range(offset, offset + length);
                }

                offset += (int)length;
            }

            return ChunkNotFound(targetBlockType);
        }

        /// <summary>
        /// Reads a given range from a FLAC file stream.
        /// </summary>
        /// <param name="stream">The FLAC file stream.</param>
        /// <param name="range">The range to read.</param>
        /// <returns>A span containing the contents of the stream in the range.</returns>
        public static ReadOnlySpan<byte> ReadRange(Stream stream, Range range)
        {
            Span<byte> buffer = new byte[range.Length];

            if (stream.Seek(range.Start, SeekOrigin.Begin) != range.Start)
            {
                throw new IOException("ReadRange: could not seek to position");
            }

            var read = stream.Read(buffer);

            if (read != range.Length)
            {
                throw new InvalidOperationException("ReadRange: read != range.Length");
            }

            return buffer;
        }

        /// <summary>
        /// Reads a given range from a FLAC file stream asynchronously.
        /// </summary>
        /// <param name="stream">The FLAC file stream.</param>
        /// <param name="range">The range to read.</param>
        /// <returns>A byte array containing the contents of the stream in the range.</returns>
        public static async ValueTask<byte[]> ReadRangeAsync(Stream stream, Range range)
        {
            byte[] buffer = new byte[range.Length];

            if (stream.Seek(range.Start, SeekOrigin.Begin) != range.Start)
            {
                throw new IOException("ReadRange: could not seek to position");
            }

            var read = await stream.ReadAsync(buffer);

            if (read != range.Length)
            {
                throw new InvalidOperationException("ReadRange: read != range.Length");
            }

            return buffer;
        }

        /// <summary>
        /// Extract vorbis comments from file.
        /// </summary>
        /// <param name="stream">The FLAC file stream.</param>
        /// <returns>A dictionary representing each comment.</returns>
        public static Fin<Dictionary<string, string>> ExtractComments(Stream stream)
        {
            Dictionary<string, string> comments = new Dictionary<string, string>();

            var vorbisChunk = Flac.ScanForChunk(stream, VorbisCommentBlockNumber);

            if (vorbisChunk.IsFail)
            {
                return (Error)vorbisChunk;
            }

            var vorbisSpan = Flac.ReadRange(stream, (Flac.Range)vorbisChunk);

            int offset = 0;
            int vendorLength = BinaryPrimitives.ReadInt32LittleEndian(vorbisSpan);

            offset += 4;

            offset += vendorLength;

            uint commentListLength = BinaryPrimitives.ReadUInt32LittleEndian(vorbisSpan[offset..]);
            offset += 4;

            uint commentLength;
            string key, value;

            // Extract each comment one by one
            for (int i = 0; i < commentListLength; i++)
            {
                commentLength = BinaryPrimitives.ReadUInt32LittleEndian(vorbisSpan[offset..]);
                offset += 4;

                int commentStart = offset;
                int commentEnd = (int)(offset + commentLength);
                offset += (int)commentLength;

                int keyValueDivder = commentStart + vorbisSpan[commentStart..commentEnd].IndexOf((byte)'=');
                key = Encoding.ASCII.GetString(vorbisSpan[commentStart..keyValueDivder]);
                value = Encoding.UTF8.GetString(vorbisSpan[(keyValueDivder + 1)..commentEnd]);

                comments.Add(key, value);
            }

            return comments;
        }

        /// <summary>
        /// Retrieve vendor string from vorbis comment block (https://xiph.org/vorbis/doc/v-comment.html).
        /// </summary>
        /// <param name="buffer">Buffer containing the beginning of the file contents.</param>
        /// <returns>The vendor string and its position in the file stream (specifically the first position following the vendor string).</returns>
        public static Fin<string> FindXiphVendorString(ReadOnlySpan<byte> buffer)
        {
            int offset = 0;
            int vendorLength = BinaryPrimitives.ReadInt32LittleEndian(buffer);

            offset += 4;

            string vendor = Encoding.UTF8.GetString(buffer[offset..(offset + vendorLength)]);

            return vendor;
        }

        public partial record Range(long Start, long End);

        public partial record Range
        {
            public long Length => this.End - this.Start;
        }
    }
}
