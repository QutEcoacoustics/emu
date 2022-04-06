// <copyright file="BinaryHelpers.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Utilities
{
    using System;

    /// <summary>
    /// Helpers for manipulating binary values.
    /// </summary>
    public static class BinaryHelpers
    {
        public const int BitsPerByte = 8;

        /// <summary>
        /// Maximum for an unsigned 36-bit integer is 68,719,476,735.
        /// </summary>
        public const ulong Unsigned36BitMaximum = (1UL << 36) - 1UL;

        /// <summary>
        /// Extracts a 36-bit integer from a 5-byte span.
        /// With a 36 bit integer there are 4 bits that are ignored, either at the end or start of the byte block.
        /// In this case we ingore the first nibble, the first 4-bits.
        /// </summary>
        /// <param name="bytes">The source bytes.</param>
        /// <returns>an unsigned 64-bit integer representing the decoded 36-bit integer.</returns>
        public static ulong Read36BitUnsignedBigEndianIgnoringFirstNibble(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 5)
            {
                throw new ArgumentException("bytes span must at least be 5 long", nameof(bytes));
            }

            const byte mask = 0b0000_1111;

            ulong dest = (ulong)(bytes[0] & mask) << 32;
            dest |= (ulong)bytes[1] << 24;
            dest |= (ulong)bytes[2] << 16;
            dest |= (ulong)bytes[3] << 8;
            dest |= (ulong)bytes[4] << 0;

            return dest;
        }

        /// <summary>
        /// Extracts a 20-bit integer from a 3-byte span.
        /// With a 20 bit integer there are 4 bits that are ignored, either at the end or start of the byte block.
        /// In this case we ingore the last nibble, the last 4-bits.
        /// </summary>
        /// <param name="bytes">The source bytes.</param>
        /// <returns>an unsigned 32-bit integer representing the decoded 20-bit integer.</returns>
        public static uint Read20BitUnsignedBigEndianIgnoringLastNibble(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 3)
            {
                throw new ArgumentException("bytes span must at least be 3 long", nameof(bytes));
            }

            uint dest = (uint)bytes[0] << 16;
            dest |= (uint)bytes[1] << 8;
            dest |= (uint)bytes[2] << 0;

            return dest >> 4;
        }

        /// <summary>
        /// Extracts a 3-bit integer from a byte.
        /// With a 3 bit integer, 5 bits are ignored.
        /// In this case we ingore the first 4 bits and the last bit.
        /// </summary>
        /// <param name="bytes">The source byte.</param>
        /// <returns>an unsigned 8-bit integer representing the decoded 3-bit integer.</returns>
        public static byte Read3BitUnsignedBigEndianIgnoringFirstFourAndLastBit(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 1)
            {
                throw new ArgumentException("bytes span must at least be 1 long", nameof(bytes));
            }

            const byte mask = 0b0000_1111;

            byte dest = (byte)(bytes[0] & mask);

            return (byte)(dest >> 1);
        }

        /// <summary>
        /// Extracts a 5-bit integer from a 2-byte span.
        /// With a 5 bit integer, 11 bits are ignored.
        /// In this case we ingore the first 7 bits and the last 4 bits.
        /// </summary>
        /// <param name="bytes">The source bytes.</param>
        /// <returns>an unsigned 8-bit integer representing the decoded 5-bit integer.</returns>
        public static byte Read5BitUnsignedBigEndianIgnoringFirstSevenAndLastFourBits(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 2)
            {
                throw new ArgumentException("bytes span must at least be 2 long", nameof(bytes));
            }

            const byte mask = 0b0000_0001;

            uint dest = (uint)((bytes[0] & mask) << 8);
            dest |= bytes[1];

            return (byte)(dest >> 4);
        }

        /// <summary>
        /// Extracts a 7-bit integer from a byte.
        /// With a 7 bit integer, 1 bit is ignored.
        /// In this case we ingore the last bit.
        /// </summary>
        /// <param name="bytes">The source byte.</param>
        /// <returns>an unsigned 8-bit integer representing the decoded 7-bit integer.</returns>
        public static byte Read7BitUnsignedBigEndianIgnoringFirstBit(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 1)
            {
                throw new ArgumentException("bytes span must at least be 1 long", nameof(bytes));
            }

            const byte mask = 0b0111_1111;

            byte dest = (byte)(bytes[0] & mask);

            return (byte)dest;
        }

        /// <summary>
        /// Extracts a 24-bit integer from a 3-byte span.
        /// </summary>
        /// <param name="bytes">The source bytes.</param>
        /// <returns>an unsigned 32-bit integer representing the decoded 24-bit integer.</returns>
        public static uint Read24bitUnsignedBigEndian(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 3)
            {
                throw new ArgumentException("bytes span must at least be 1 long", nameof(bytes));
            }

            int dest = bytes[0] << 16;
            dest |= bytes[1] << 8;
            dest |= bytes[2];

            return (uint)dest;
        }

        /// <summary>
        /// Writes a 36-bit integer to a 5 byte buffer.
        /// It ignore the first nibble (4-bits) of the buffer and starts writing from the second nibble onwards.
        /// </summary>
        /// <param name="bytes">The buffer to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write36BitUnsignedBigEndianIgnoringFirstNibble(Span<byte> bytes, ulong value)
        {
            if (bytes.Length < 5)
            {
                throw new ArgumentException("bytes span must at least be 5 long", nameof(bytes));
            }

            // 2^36
            if (value > Unsigned36BitMaximum)
            {
                throw new ArgumentException($"Value `{value}`is outside the representable range of a unsigned 36-bit integer", nameof(value));
            }

            const byte intMask = 0b0000_1111;
            const byte currentMask = 0b1111_0000;

            // we need to merge the first nibble of the current byte span with this value
            bytes[0] = (byte)((bytes[0] & currentMask) | (byte)((value >> 32) & intMask));
            bytes[1] = (byte)((value >> 24) & 0xFF);
            bytes[2] = (byte)((value >> 16) & 0xFF);
            bytes[3] = (byte)((value >> 8) & 0xFF);
            bytes[4] = (byte)((value >> 0) & 0xFF);
        }
    }
}
