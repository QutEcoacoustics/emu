// <copyright file="BinaryHelpers.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Utilities
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.CompilerServices;

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
                throw new ArgumentException("bytes span must at least be 3 long", nameof(bytes));
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

        /// <summary>
        /// Read the highest 6 bits of a 32-bit integer as it's own integer.
        /// </summary>
        /// <param name="value">The value to extract the number from.</param>
        /// <returns>A 6-bit number in a uint32 slot.</returns>
        public static byte ReadHighest6Bits(uint value)
        {
            return (byte)((value >> 26) & 0b111111);
        }

        /// <summary>
        /// Read an integer out of a subset of 4 bytes.
        /// </summary>
        /// <param name="value">The 4 byte uint to read from.</param>
        /// <param name="lowBit">The index of the lowest bit to read from (inclusive).</param>
        /// <param name="highBit">The index of the highest bit to read from (exclusive).</param>
        /// <returns>The read value shifted right by <paramref name="lowBit"/>s.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadBitRange(uint value, byte lowBit, byte highBit)
        {
            // push 1 up to the bit width (which will produce a number that is one followed by zeroes)
            //   1 << 8 = 0b1_0000_0000
            // then subtract one to cycle all lower bits to one. Now we have a string of ones the right width.
            //   0b1_0000_0000 - 1 =  0b1111_1111
            uint highMask = (1u << (highBit - lowBit)) - 1u;
            return (value >> lowBit) & highMask;
        }

        /// <summary>
        /// Write a integer into a subset of 4 bytes.
        /// </summary>
        /// <param name="destination">The destination to merge the value into.</param>
        /// <param name="lowBit">The index of the lowest bit to write to(inclusive).</param>
        /// <param name="highBit">The index of the highest bit to write to(exclusive).</param>
        /// <param name="value">The value to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBitRange(ref uint destination, byte lowBit, byte highBit, uint value)
        {
            uint mask = ((1u << (highBit - lowBit)) - 1u) << lowBit;

            // shift value into right spot
            // only keep bits in range of int width
            uint shifted = (value << lowBit) & mask;

            // zero out destiantion bits and merge with existing value
            destination = (destination & ~mask) | shifted;
        }

        /// <summary>
        /// Read an integer out of a subset of 8 bytes.
        /// </summary>
        /// <param name="value">The 8 byte ulong to read from.</param>
        /// <param name="lowBit">The index of the lowest bit to read from (inclusive).</param>
        /// <param name="highBit">The index of the highest bit to read from (exclusive).</param>
        /// <returns>The read value shifted right by <paramref name="lowBit"/>s.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadBitRange(ulong value, byte lowBit, byte highBit)
        {
            ulong highMask = (1ul << (highBit - lowBit)) - 1ul;
            return (value >> lowBit) & highMask;
        }

        /// <summary>
        /// Write a integer into a subset of 8 bytes.
        /// </summary>
        /// <param name="destination">The destination to merge the value into.</param>
        /// <param name="lowBit">The index of the lowest bit to write to(inclusive).</param>
        /// <param name="highBit">The index of the highest bit to write to(exclusive).</param>
        /// <param name="value">The value to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBitRange(ref ulong destination, byte lowBit, byte highBit, ulong value)
        {
            ulong mask = ((1ul << (highBit - lowBit)) - 1ul) << lowBit;

            // shift value into right spot
            // only keep bits in range of int width
            ulong shifted = (value << lowBit) & mask;

            // zero out destiantion bits and merge with existing value
            destination = (destination & ~mask) | shifted;
        }

        /// <summary>
        /// Read a signed integer out of a subset of 8 bytes.
        /// Assumes integers are encoded as two's complement.
        /// </summary>
        /// <param name="value">The 8 byte ulong to read from.</param>
        /// <param name="lowBit">The index of the lowest bit to read from (inclusive).</param>
        /// <param name="highBit">The index of the highest bit to read from (exclusive).</param>
        /// <returns>The read value shifted right by <paramref name="lowBit"/>s.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadSignedBitRange(ulong value, byte lowBit, byte highBit)
        {
            const byte maxShift = 64;

            // shift value far to the left to the highest bits
            var shifted = value << (maxShift - highBit);

            // do an unchecked cast to convert the number to a signed variant
            // this allows us to then do an arithmetic shift (which fills copies
            // of the highest bit when down shifted);
            long shiftBack = unchecked((long)shifted) >> (maxShift - (highBit - lowBit));

            return shiftBack;
        }

        /// <summary>
        /// Write a signed integer into a subset of 8 bytes.
        /// Encodes integers as two's complement.
        /// </summary>
        /// <param name="destination">The destination to merge the value into.</param>
        /// <param name="lowBit">The index of the lowest bit to write to(inclusive).</param>
        /// <param name="highBit">The index of the highest bit to write to(exclusive).</param>
        /// <param name="value">The value to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSignedBitRange(ref ulong destination, byte lowBit, byte highBit, long value)
        {
            ulong mask = ((1ul << (highBit - lowBit)) - 1ul) << lowBit;

            // shift value into right spot
            // only keep bits in range of int width
            // the mask wipes out higher order bits - which are set for negative values
            ulong shifted = (unchecked((ulong)value) << lowBit) & mask;

            // zero out destiantion bits and merge with existing value
            destination = (destination & ~mask) | shifted;
        }

        // intrinsically flawed since we can't represent -0 (negative 0) in C#
        ///// <summary>
        ///// Read a signed integer out of a subset of 8 bytes.
        ///// Assumes integers are encoded in the sign-magnitude format.
        ///// </summary>
        ///// <param name="value">The 8 byte ulong to read from.</param>
        ///// <param name="lowBit">The index of the lowest bit to read from (inclusive).</param>
        ///// <param name="highBit">The index of the highest bit to read from (exclusive).</param>
        ///// <returns>The read value shifted right by <paramref name="lowBit"/>s.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static long ReadSignedMagnitudeBitRange(ulong value, byte lowBit, byte highBit)
        //{
        //    // bit flip a mask one less than the width of the target number
        //    ulong highMask = (1ul << (highBit - lowBit - 1)) - 1ul;

        //    var shifted = (long)((value >> lowBit) & highMask);

        //    // if the sign bit is set make it negative
        //    return (value & (1ul << (highBit - 1))) != 0 ? -shifted : shifted;
        //}

        ///// <summary>
        ///// Write a signed integer into a subset of 8 bytes.
        ///// Encodes integers in the sign-magnitude format.
        ///// </summary>
        ///// <param name="destination">The destination to merge the value into.</param>
        ///// <param name="lowBit">The index of the lowest bit to write to(inclusive).</param>
        ///// <param name="highBit">The index of the highest bit to write to(exclusive).</param>
        ///// <param name="value">The value to write.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static void WriteSignedMagnitudeBitRange(ref ulong destination, byte lowBit, byte highBit, long value)
        //{
        //    var width = highBit - lowBit;

        //    ulong abs = unchecked((ulong)(value < 0 ? (~value) + 1 : value));

        //    // make a sign bit
        //    var negative = value < 0 ? 1ul << width : 0ul;

        //    // make a mask full of ones the width of the target number
        //    ulong mask = (1ul << width) - 1ul;

        //    // merge our value in with the sign bit
        //    // use the mask to wipe out higher order bits - which are only set for negative values
        //    ulong masked = (abs | negative) & mask;

        //    // shift value into right spot
        //    ulong shifted = masked << lowBit;

        //    // merge value with existing value
        //    destination = (destination & ~mask) | shifted;
        //}

        public static bool ReadBool16LittleEndian(ReadOnlySpan<byte> bytes)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(bytes) switch
            {
                0 => false,
                1 => true,
                ushort u => throw new InvalidOperationException("Invalid value for a 'boolean': " + u),
            };
        }

        /// <summary>
        /// Deals with a weird encoding where large values are encoded as consecutive
        /// two-byte little endian values.
        /// </summary>
        /// <param name="bytes">The span to read the first four bytes from.</param>
        /// <returns>The value as an <seealso cref="uint"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static uint ReadTwoUInt16LittleEndianAsOneUInt32(ReadOnlySpan<byte> bytes)
        {
            // bytes in file: b0 b1 b2 b3
            // decoding order (highest to lowest): b1 b0 b3 b2
            return (uint)(
              bytes[1] << 24 |
              bytes[0] << 16 |
              bytes[3] << 08 |
              bytes[2]);
        }

        /// <summary>
        /// Deals with a weird encoding where large values are encoded as consecutive
        /// two-byte little endian values.
        /// </summary>
        /// <param name="bytes">The span to read the first 8 bytes from.</param>
        /// <returns>The value as an <seealso cref="ulong"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static ulong ReadFourUInt16LittleEndianAsOneUInt64(ReadOnlySpan<byte> bytes)
        {
            // bytes in file: b0 b1 b2 b3 b4 b5 b6 b7
            // decoding order (highest to lowest): b1 b0 b3 b2 b5 b4 b7 b6
            return
                (ulong)bytes[1] << 56 |
                (ulong)bytes[0] << 48 |
                (ulong)bytes[3] << 40 |
                (ulong)bytes[2] << 32 |
                (ulong)bytes[5] << 24 |
                (ulong)bytes[4] << 16 |
                (ulong)bytes[7] << 08 |
                (ulong)bytes[6];
        }
    }
}
