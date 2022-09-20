// <copyright file="FrameHeader.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio
{
    using System.Buffers;
    using System.Buffers.Binary;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;
    using System.Text;
    using Emu.Audio.Formats.FLAC;
    using InvertedTomato.IO;
    using LanguageExt;
    using LanguageExt.Common;
    using static System.Collections.Specialized.BitVector32;

    public readonly partial record struct FrameHeader(
        FrameBlockingStrategy BlockingStrategy,
        ushort BlockSize,
        uint SampleRate,
        FrameChannelAssignment ChannelAssignment,
        byte SampleSize,
        uint? FrameNumber,
        ulong? StartingSample,
        byte Crc8);

    /// <summary>
    /// https://xiph.org/flac/format.html#frame_header.
    /// </summary>
    public readonly partial record struct FrameHeader
    {
        /// <summary>
        ///  Constant, 14 bits '11111111111110'.
        /// </summary>
        public const int SyncCodeConstant = 0b1111_1111_1111_10;

        /// <summary>
        /// First whole byte of the sync code.
        /// </summary>
        public const byte SyncCodeByteOne = 0xFF;

        /// <summary>
        /// Second whole byte of the sync code including fixed size blocking strategy.
        /// </summary>
        public const byte SyncCodeByteTwoA = 0b1111_1000;

        /// <summary>
        /// Second whole byte of the sync code including variable size blocking strategy.
        /// </summary>
        public const byte SyncCodeByteTwoB = 0b1111_1001;

        /// <summary>
        /// The maximum size of a frame header is 16 bytes.
        /// </summary>
        public const int FrameHeaderMaxSize = 16;

        private static readonly Section SyncCodeMask = CreateSection(14);
        private static readonly Section ReservedMask = CreateSection(1, SyncCodeMask);
        private static readonly Section BlockingStrategyMask = CreateSection(1, ReservedMask);
        private static readonly Section BlockSizeMask = CreateSection(4, BlockingStrategyMask);
        private static readonly Section SampleRateMask = CreateSection(4, BlockSizeMask);
        private static readonly Section ChannelAssignmentMask = CreateSection(4, SampleRateMask);
        private static readonly Section SampleSizeMask = CreateSection(3, ChannelAssignmentMask);
        private static readonly Section NextReservedMask = CreateSection(1, SampleSizeMask);

        private static readonly Error BadUTF8Number = Error.New("Invalid Frame Header: UTF-8 coded number cannot be read");
        private static readonly Func<byte, byte, Error> BadCrc = (calculated, crc) => Error.New($"Invalid Frame Header: CRC8 did not match. Calculated 0x{calculated:X}, Embedded 0x{crc:X}");
        private static readonly Error BadSyncCode = Error.New("Invalid Frame Header: Sync code not found at start of span");
        private static readonly Error BadFrameBlockSize0 = Error.New("Invalid Frame Header: Block size 0 is reserved");
        private static readonly Error BadFrameSampleRate = Error.New("Invalid Frame Header: Sample rate 15 is invalid");
        private static readonly Error BadFrameBitDepth = Error.New("Invalid Frame Header: Bit depth is reserved");
        private static readonly Error BadFrameChannelAssignment = Error.New("Invalid Frame Header: channel assignment is reserved");

        public static Fin<FrameHeader> Parse(ReadOnlySpan<byte> slice, uint streamInfoSampleRate, byte streamInfoSampleSize, out int consumed)
        {
            if (slice.Length != FrameHeaderMaxSize)
            {
                throw new ArgumentException("16 bytes is required to parse a frame header", nameof(slice));
            }

            consumed = 0;

            // See https://xiph.org/flac/format.html#frame
            // See https://stackoverflow.com/a/11145067/224512
            var one = slice[consumed++];
            var two = slice[consumed++];

            var syncCode = (one << 8 | (two & 0b11111100)) >> 2;
            if (syncCode != SyncCodeConstant)
            {
                return BadSyncCode;
            }

            var three = slice[consumed++];
            var four = slice[consumed++];

            var blockingStrategy = (FrameBlockingStrategy)(two & 0b00000001);
            var blockSizeCode = (three & 0b11110000) >> 4;
            var sampleRateCode = three & 0b00001111;
            var channelAssignment = ParseChannelAssignment((four & 0b11110000) >> 4);

            if (channelAssignment.IsFail)
            {
                return (Error)channelAssignment;
            }

            var sampleSize = ParseSampleSize((four & 0b00001110) >> 1, streamInfoSampleSize);

            if (sampleSize.IsFail)
            {
                return (Error)sampleSize;
            }

            var sampleOrFrameNumber = FlacUTF8Coding.Utf8Decode(slice[consumed..], out var utf8Consumed);
            if (sampleOrFrameNumber.IsFail)
            {
                return BadUTF8Number;
            }

            consumed += utf8Consumed;

            var blockSize = ParseBlockSize(blockSizeCode, slice, ref consumed);

            if (blockSize.IsFail)
            {
                return (Error)blockSize;
            }

            var sampleRate = ParseSampleRate(sampleRateCode, slice, ref consumed, streamInfoSampleRate);

            if (sampleRate.IsFail)
            {
                return (Error)sampleRate;
            }

            var crc8 = slice[consumed];
            consumed++;

            // verify the crc
            // we don't really need to check the crc but since the frame structure is of variable size
            // and has no end marker it's nice to validate we've read the correct data so far.
            var calcuated = CrcAlgorithm.CreateCrc8().Append(slice[0..(consumed - 1)].ToArray()).ToUInt64();
            if ((calcuated & crc8) != calcuated)
            {
                return BadCrc((byte)calcuated, crc8);
            }

            // "UTF-8" coded frame number
            uint? frameNumber = blockingStrategy == FrameBlockingStrategy.Fixed ? (uint)sampleOrFrameNumber.ThrowIfFail() : null;

            // "UTF-8" coded sample number
            ulong? sampleNumber = blockingStrategy == FrameBlockingStrategy.Variable ? (ulong)sampleOrFrameNumber.ThrowIfFail() : null;

            return new FrameHeader(
                blockingStrategy,
                blockSize.ThrowIfFail(),
                sampleRate.ThrowIfFail(),
                channelAssignment.ThrowIfFail(),
                sampleSize.ThrowIfFail(),
                frameNumber,
                sampleNumber,
                crc8);
        }

        private static Fin<ushort> ParseBlockSize(int blockSizeCode, ReadOnlySpan<byte> bytes, ref int offset)
        {
            return blockSizeCode switch
            {
                0b0000 => BadFrameBlockSize0,
                0b0001 => 192,
                0b0010 => 576,
                0b0011 => 1152,
                0b0100 => 2304,
                0b0101 => 4608,

                // get 8-bit value from end of header
                0b0110 => (ushort)(bytes.ReadByte(ref offset) + 1),

                // get 16-bit value from end of header
                0b0111 => (ushort)(bytes.ReadUInt16BigEndian(ref offset) + 1),
                0b1000 => 256,
                0b1001 => 512,
                0b1010 => 1024,
                0b1011 => 2048,
                0b1100 => 4096,
                0b1101 => 8192,
                0b1110 => 16384,
                0b1111 => 32768,
                _ => throw new InvalidOperationException("Frame block size value cannot exceed 15"),
            };
        }

        private static Fin<uint> ParseSampleRate(
            int sampleRateCode,
            ReadOnlySpan<byte> bytes,
            ref int offset,
            uint streamInfoSampleRate)
        {
            return sampleRateCode switch
            {
                // get sample rate from STREAMINFO
                0b0000 => streamInfoSampleRate,
                0b0001 => 88_200,
                0b0010 => 176_400,
                0b0011 => 192_000,
                0b0100 => 8000,
                0b0101 => 16_000,
                0b0110 => 22_050,
                0b0111 => 24_000,
                0b1000 => 32_000,
                0b1001 => 44_100,
                0b1010 => 48_000,
                0b1011 => 96_000,

                // get 8-bit sample rate (in kHz) from end of header
                0b1100 => (uint)(bytes.ReadByte(ref offset) * 1000),

                // get 16 bit sample rate (in Hz) from end of header
                0b1101 => bytes.ReadUInt16BigEndian(ref offset),

                //  get 16 bit sample rate (in tens of Hz) from end of header
                0b1110 => (uint)(bytes.ReadUInt16BigEndian(ref offset) * 10),
                0b1111 => BadFrameSampleRate,
                _ => throw new InvalidOperationException("Frame block size value cannot exceed 15"),
            };
        }

        private static Fin<byte> ParseSampleSize(int bitDepthCode, byte streamInfoBitDepth)
        {
            return bitDepthCode switch
            {
                // get bit depth from STREAMINFO
                0b0000 => streamInfoBitDepth,
                0b0001 => 8,
                0b0010 => 12,
                0b0011 => BadFrameBitDepth,
                0b0100 => 16,
                0b0101 => 20,
                0b0110 => 24,
                0b0111 => BadFrameBitDepth,
                _ => throw new InvalidOperationException("Frame block size value cannot exceed 7"),
            };
        }

        private static Fin<FrameChannelAssignment> ParseChannelAssignment(int channelAssignmentCode)
        {
            return channelAssignmentCode switch
            {
                >= 0b0000 and <= 0b0111 => (FrameChannelAssignment)(channelAssignmentCode + 1),
                0b1000 => FrameChannelAssignment.LeftPlusSideStereo,
                0b1001 => FrameChannelAssignment.RightPlusSideStereo,
                0b1010 => FrameChannelAssignment.MidPlusSideStereo,
                >= 0b1011 and <= 0b1111 => BadFrameChannelAssignment,
                _ => throw new InvalidOperationException("Frame channel assignment cannot exceed 15"),
            };
        }
    }
}
