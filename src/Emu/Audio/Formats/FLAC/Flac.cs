// <copyright file="Flac.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio
{
    using System.Buffers;
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.IO.Pipelines;
    using System.Text;
    using Emu.Utilities;
    using LanguageExt;
    using LanguageExt.Common;

    public static partial class Flac
    {
        public const string Mime = "audio/flac";
        public const string Extension = ".flac";

        public const int FlacSamplesOffset = 21;
        public const int SampleRateOffset = 18;
        public const int BlockSizeOffset = 8;
        public const int FrameSizeOffset = 12;
        public const int ChannelOffset = 20;
        public const int BlockTypeOffset = 4;
        public const int MD5Offset = 26; // bytes

        public const int MetadataBlockSize = 42;
        public const int VorbisCommentBlockNumber = 4;

        public static readonly byte[] FlacMagicNumber = new byte[] { (byte)'f', (byte)'L', (byte)'a', (byte)'C' };

        public static readonly Error FileTooShort = Error.New("Error reading file: file is not long enough to read metadata");
        public static readonly Error FileTooShortFlac = Error.New("Error reading file: file is not long enough to have a fLaC header");
        public static readonly Error VendorStringNotFound = Error.New("Error reading file: could not find vendor string Frontier Labs in file header");
        public static readonly Error InvalidOffset = Error.New("Error reading file: an invalid offset was found");
        public static readonly Func<MetadataBlockType, Error> ChunkNotFound = x => Error.New($"Chunk with ID `{x}` was not found");
        public static readonly Error BadMetadataSeek = Error.New("Could not seek to the next metadata block");
        public static readonly Error BadFrameSeek = Error.New("Could not seek to the required position while scanning for frames");
        public static readonly Error CountSamplesBlockSize = Error.New("CountSamples only works on files that have a fixed block size");
        public static readonly Error CountSamplesNotEnoughFrames = Error.New("Could not find enough frames to count samples for");
        public static readonly Error CountSamplesNotFixed = Error.New("Found a mix of fixed and variable size frames; this is not allowed");
        public static readonly Error CountSamplesNotConsecutive = Error.New("Found non-consecutive frame");

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
        /// Scans through a FLAC file counting the number of samples in each frame.
        /// </summary>
        /// <remarks>
        /// This method is useful when dealing with a file that as an invalid number
        /// of samples encoded in the STREAMINFO block.
        /// See https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </remarks>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The count of samples.</returns>
        public static async Task<Fin<ulong>> CountSamplesAsync(Stream stream)
        {
            var blockSizes = ReadBlockSizes(stream).Bind(CheckBlockSize);
            if (blockSizes.IsFail)
            {
                return (Error)blockSizes;
            }

            var frameSizes = ReadFrameSizes(stream);

            if (frameSizes.IsFail)
            {
                return (Error)frameSizes;
            }

            var sampleRate = ReadSampleRate(stream);

            if (sampleRate.IsFail)
            {
                return (Error)sampleRate;
            }

            var sampleSize = ReadBitDepth(stream);

            if (sampleSize.IsFail)
            {
                return (Error)sampleSize;
            }

            // OK, here's the theory:
            // Get the last two frames in the file.
            // Assuming a fixed blocksize (validated above) the last frame will
            // have the frame number we can use to calculate all previous samples.
            // The last frame will have the remaining fragemnt.
            // We're going to scan near the end of the file, by at least two of the
            // largest frames according to the metadata. Doing it this way means
            // we'll avoid scanning most of the file.

            var largest = frameSizes.ThrowIfFail().Maximum;

            // some encoders do not set the block size, so if this is zero guess at a larger value
            if (largest == 0)
            {
                // largest possible block size in samples * a random bit depth (16 bits is 2 bytes);
                // this is a huge overestimate but it caters for raw encodings of chunks of the largest frame size
                largest = 32_768 * 2;
            }

            var offset = stream.Length - (largest * 3);

            // ensure offset is in a range that makes sense
            var frameStart = FindFrameStart(stream);
            if (frameStart.Case is long f)
            {
                if (offset < f)
                {
                    offset = f;
                }
            }
            else
            {
                return (Error)frameStart;
            }

            var frames = await EnumerateFrames(stream, sampleRate.ThrowIfFail(), sampleSize.ThrowIfFail(), blockSizes.ThrowIfFail().Minimum, offset).ToArrayAsync();

            if (frames.Length < 2)
            {
                return CountSamplesNotEnoughFrames;
            }

            var penultimate = frames[^2];
            var ultimate = frames[^1];

            var result = from pu in penultimate
                         from u in ultimate
                         select ((ulong)(GetFrameNumber(pu) + 1) * pu.Header.BlockSize) + u.Header.BlockSize;

            return result;

            Fin<(ushort Minimum, ushort Maximum)> CheckBlockSize((ushort Minimum, ushort Maximum) b)
            {
                return b.Minimum != b.Maximum ? CountSamplesBlockSize : b;
            }

            uint GetFrameNumber(Frame f) => f.Header.FrameNumber ?? throw new NotSupportedException("This check is not support for FLAC files with variable size blocks. File a bug report on the emu repository.");
        }

        /// <summary>
        /// The block sizes extracted from bits 64-96 of the stream.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The minimum and maximum block sizes in samples.</returns>
        public static Fin<(ushort Minimum, ushort Maximum)> ReadBlockSizes(Stream stream)
        {
            long position = stream.Seek(BlockSizeOffset, SeekOrigin.Begin);
            Debug.Assert(position == BlockSizeOffset, $"Expected stream.Seek position to return {BlockSizeOffset}, instead returned {position}");

            Span<byte> buffer = stackalloc byte[4];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return (BinaryPrimitives.ReadUInt16BigEndian(buffer), BinaryPrimitives.ReadUInt16BigEndian(buffer[2..]));
        }

        /// <summary>
        /// The frame sizes extracted from bits 96-144 of the stream.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The minimum and maximum frame sizes in bytes.</returns>
        public static Fin<(uint Minimum, uint Maximum)> ReadFrameSizes(Stream stream)
        {
            long position = stream.Seek(FrameSizeOffset, SeekOrigin.Begin);
            Debug.Assert(position == FrameSizeOffset, $"Expected stream.Seek position to return {FrameSizeOffset}, instead returned {position}");

            Span<byte> buffer = stackalloc byte[6];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return (BinaryHelpers.Read24bitUnsignedBigEndian(buffer), BinaryHelpers.Read24bitUnsignedBigEndian(buffer[3..]));
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
        public static Fin<byte> ReadNumberChannels(Stream stream)
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

        /// <summary>
        /// Returns the embedded MD5 checksum of the file.
        /// </summary>
        /// <param name="stream">The FLAC file stream.</param>
        /// <returns>The MD5 value as bytes.</returns>
        public static Fin<byte[]> ReadMD5(Stream stream)
        {
            long position = stream.Seek(MD5Offset, SeekOrigin.Begin);
            Debug.Assert(position == 26, $"Expected stream.Seek position to return 26, instead returned {position}");

            Span<byte> buffer = stackalloc byte[16];
            int bytesRead = stream.Read(buffer);
            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return buffer.ToArray();
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

            return buffer.StartsWith(FlacMagicNumber) switch
            {
                false => false,
                true => HasMetadataBlock(stream),
            };
        }

        /// <summary>
        /// Scans a FLAC file for a specific block.
        /// Blocks are identified by unique byte.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <param name="targetBlockType">The block being searched for.</param>
        /// <returns>Range indicating the position of the block in the file stream.</returns>
        public static Fin<RangeHelper.Range> ScanForChunk(Stream stream, MetadataBlockType targetBlockType)
        {
            foreach (var block in EnumerateMetadataBlocks(stream))
            {
                if (block.Case is MetadataBlock m && m.Type == targetBlockType)
                {
                    return new RangeHelper.Range(m.Offset, m.Offset + m.Length);
                }
                else if (block.IsFail)
                {
                    return (Error)block;
                }
            }

            return ChunkNotFound(targetBlockType);
        }

        /// <summary>
        /// Finds the offset of the first byte of the first FRAME structure.
        /// Assumes stream is already validated as a FLAC file.
        /// </summary>
        /// <param name="stream">The stream to scan.</param>
        /// <returns>The offset of the first byte of the first FRAME.</returns>
        public static Fin<long> FindFrameStart(Stream stream)
        {
            var last = EnumerateMetadataBlocks(stream).Last();

            Debug.Assert(!last.IsSucc || last.ThrowIfFail().Last, "Last metadata block should have last flag set");

            return last.Map(b => b.Offset + b.Length);
        }

        public static IEnumerable<Fin<MetadataBlock>> EnumerateMetadataBlocks(Stream stream)
        {
            const int BlockTypeLength = 1, BlockLengthLength = 3;

            int offset = BlockTypeOffset;
            byte blockType;
            bool lastBlock = false;

            var buffer = new byte[BlockTypeLength + BlockLengthLength];

            while (!lastBlock)
            {
                var newOffset = stream.Seek(offset, SeekOrigin.Begin);

                if (newOffset != offset)
                {
                    yield return InvalidOffset;
                    yield break;
                }

                var read = stream.Read(buffer);
                offset += read;

                if (read != (BlockTypeLength + BlockLengthLength))
                {
                    yield return BadMetadataSeek;
                    yield break;
                }

                lastBlock = (buffer[0] >> 7) == 1;
                blockType = BinaryHelpers.Read7BitUnsignedBigEndianIgnoringFirstBit(buffer[..BlockTypeLength]);
                var length = BinaryHelpers.Read24bitUnsignedBigEndian(buffer[BlockTypeLength..]);

                yield return new MetadataBlock(
                    (MetadataBlockType)blockType,
                    lastBlock,
                    offset,
                    length);

                offset += (int)length;
            }
        }

        /// <summary>
        /// Scans the file for FRAME_HEADER sync codes and returns a <see cref="FrameHeader" /> each time
        /// a frame is found.
        /// </summary>
        /// <param name="stream">The stream of the flac file.</param>
        /// <param name="streamInfoSampleRate">The sample rate as in the STREAMINFO block.</param>
        /// <param name="streamInfoSampleSize">The sample size as in the STREAMINFO block.</param>
        /// <param name="fixedBlockSize">The fixed block size if known.</param>
        /// <param name="offset">
        /// An optional offset to start scanning from.
        /// If <value>null</value> the scan will start from the first frame that occurs after the metadata blocks.
        /// </param>
        /// <returns>A lazyily generated list of FrameHeaders if any are found.</returns>
        public static async IAsyncEnumerable<Fin<Frame>> EnumerateFrames(Stream stream, uint streamInfoSampleRate, byte streamInfoSampleSize, ushort? fixedBlockSize, long? offset = null)
        {
            long start;
            if (offset is null)
            {
                var frameStart = FindFrameStart(stream);
                if (frameStart.IsFail)
                {
                    yield return (Error)frameStart;
                    yield break;
                }

                start = (long)frameStart;
            }
            else
            {
                start = offset.Value;
            }

            var position = stream.Seek(start, SeekOrigin.Begin);
            if (position != start)
            {
                yield return BadFrameSeek;
                yield break;
            }

            var pipe = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: true));

            ReadResult result;
            uint frameCounter = 0;
            long streamOffset = start;
            Frame lastFrame = default;
            while (true)
            {
                result = await pipe.ReadAsync();

                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }

                var frames = ScanBuffer(result.Buffer, out var consumed, out var examined, ref frameCounter);
                foreach (var frame in frames)
                {
                    yield return frame;
                }

                streamOffset += result.Buffer.GetSequenceOffset(consumed);
                pipe.AdvanceTo(consumed, examined);
            }

            yield break;

            IEnumerable<Frame> ScanBuffer(ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined, ref uint frameCounter)
            {
                var reader = new SequenceReader<byte>(buffer);
                Span<byte> frameBytes = stackalloc byte[FrameHeader.FrameHeaderMaxSize];
                var result = Seq<Frame>.Empty;

                while (!reader.End)
                {
                    var found = reader.TryAdvanceTo(FrameHeader.SyncCodeByteOne, false);
                    if (found)
                    {
                        if (reader.Remaining < FrameHeader.FrameHeaderMaxSize)
                        {
                            // our frame is (potentially) split over a buffer
                            // break early but mark consumed as not the entire buffer. The next pipe.ReadAsync should include
                            // our remaining span prepended onto the next buffer
                            break;
                        }

                        var peek = reader.TryPeek(1, out var second);
                        Debug.Assert(peek, "Peeking should always work");
                        if (second == FrameHeader.SyncCodeByteTwoA || second == FrameHeader.SyncCodeByteTwoB)
                        {
                            reader.UnreadSequence.Slice(0, FrameHeader.FrameHeaderMaxSize).CopyTo(frameBytes);

                            var header = FrameHeader.Parse(frameBytes, streamInfoSampleRate, streamInfoSampleSize, out int frameSize);

                            header = header.Bind(CheckConsecutive);

                            // discard any frame that does not pass muster - the crc check in particular is useful for weeding out
                            // byte sequences from subframes that fool the sync code.
                            var offset = streamOffset + buffer.GetSequenceOffset(reader.Position);
                            if (header.IsSucc)
                            {
                                var frameHeader = (FrameHeader)header;

                                // if we start seeking midway through the file set the index to to the frame number
                                frameCounter = frameCounter == 0 ? (frameHeader.FrameNumber ?? frameCounter + 1) : frameCounter + 1;
                                var frame = new Frame(frameCounter, offset, frameHeader);
                                result = result.Add(frame);
                                lastFrame = frame;

                                reader.Advance(frameSize);
                            }
                            else
                            {
#if DEBUG
                                Debug.WriteLine($"Frame discarded: offset:{offset},index:{frameCounter}, {header}");
#endif

                                // only advance the two scanned bytes. There still exists the potential for
                                // a frame to exist within this header.
                                reader.Advance(2);
                            }
                        }
                        else if (second == FrameHeader.SyncCodeByteOne)
                        {
                            // the next byte is the first sync byte, don't move past it!
                            reader.Advance(1);
                        }
                        else
                        {
                            // the 0xFF byte and the second byte which failed the second check
                            reader.Advance(2);
                        }
                    }
                    else
                    {
                        // delimitter not found within buffer, advance to end
                        reader.Advance(reader.Remaining);
                    }
                }

                consumed = reader.Position;
                examined = buffer.End;
                return result;
            }

            Fin<FrameHeader> CheckConsecutive(FrameHeader h)
            {
                if (lastFrame == default)
                {
                    // skip;
                    return h;
                }

                if (lastFrame.Header.BlockingStrategy == FrameBlockingStrategy.Fixed)
                {
                    if (h.BlockingStrategy != FrameBlockingStrategy.Fixed)
                    {
                        return CountSamplesNotFixed;
                    }

                    if (lastFrame.Header.FrameNumber + 1 != h.FrameNumber)
                    {
                        return CountSamplesNotConsecutive;
                    }
                }
                else if (lastFrame.Header.StartingSample > h.StartingSample)
                {
                    return CountSamplesNotConsecutive;
                }

                return h;
            }
        }

        /// <summary>
        /// Extract vorbis comments from file.
        /// </summary>
        /// <param name="stream">The FLAC file stream.</param>
        /// <returns>A dictionary representing each comment.</returns>
        public static Fin<Dictionary<string, string>> ExtractComments(Stream stream)
        {
            Dictionary<string, string> comments = new Dictionary<string, string>();

            var vorbisChunk = ScanForChunk(stream, MetadataBlockType.VorbisComment);

            if (vorbisChunk.IsFail)
            {
                return (Error)vorbisChunk;
            }

            var vorbisSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)vorbisChunk);

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

            try
            {
                string vendor = Encoding.UTF8.GetString(buffer[offset..(offset + vendorLength)]);
                return vendor;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Error.New("Invalid UTF8 string", (Exception)ex);
            }
        }

        /// <summary>
        /// Determines whether a file header has and is large enough to store a FLAC metadata block.
        /// The block type is stored in bits 33-39 of the file stream, the first is of type "STREAMINFO".
        /// This block type corresponds to a value of 0.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>Boolean indicating whether the file has a valid metadata block.</returns>
        private static Fin<bool> HasMetadataBlock(Stream stream)
        {
            // AT 2022: This is basically just a check for a 0 at index 4 which can be
            // extremely common. This leads to false positives for other sample files.
            // Made this method private and included it into IsFlacFile because the two definitions
            // are inseperable anyway.

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

        public record struct MetadataBlock(MetadataBlockType Type, bool Last, long Offset, uint Length);
    }
}
