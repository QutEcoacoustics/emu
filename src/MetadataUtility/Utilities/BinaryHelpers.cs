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
        /// <summary>
        /// Maximum for an unsigned 36-bit integer is 68,719,476,735.
        /// </summary>
        public const ulong Unsigned36BitMaximum = (1UL << 36) - 1UL;

        /// <summary>
        /// Extracts a 36-bit integer from a 5-byte span.
        /// With a 36 bit integer there are 4 bits that are ignored, either at the end or start of the byte block.
        /// In this case we ingore the first octet, the first 4-bits.
        /// </summary>
        /// <param name="bytes">The source bytes.</param>
        /// <returns>a unsigned 64-bit integer representing the decoded 36-bit integer.</returns>
        public static ulong Read36BitUnsignedBigEndianIgnoringFirstOctet(ReadOnlySpan<byte> bytes)
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
        /// Writes a 36-bit integer to a 5 byte buffer.
        /// It ignore the first octet (4-bits) of the buffer and starts writing from the second octet onwards.
        /// </summary>
        /// <param name="bytes">The buffer to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write36BitUnsignedBigEndianIgnoringFirstOctet(Span<byte> bytes, ulong value)
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

            // we need to merge the first octet of the current byte span with this value
            bytes[0] = (byte)((bytes[0] & currentMask) | (byte)((value >> 32) & intMask));
            bytes[1] = (byte)((value >> 24) & 0xFF);
            bytes[2] = (byte)((value >> 16) & 0xFF);
            bytes[3] = (byte)((value >> 8) & 0xFF);
            bytes[4] = (byte)((value >> 0) & 0xFF);
        }
    }
}
