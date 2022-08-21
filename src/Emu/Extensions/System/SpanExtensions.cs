// <copyright file="SpanExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace System;

using System.Buffers.Binary;
using System.Runtime.InteropServices;

public static class SpanExtensions
{
    /// <summary>
    /// Converts a byte array to a hexadecimal string representation (lower case).
    /// </summary>
    /// <param name="bytes">The bytes to encode.</param>
    /// <returns>The encoded string.</returns>
    public static string ToHexString(this byte[] bytes)
    {
        return ToHexString((ReadOnlySpan<byte>)bytes);
    }

    /// <summary>
    /// Converts a byte span to a hexadecimal string representation (lower case).
    /// </summary>
    /// <param name="bytes">The bytes to encode.</param>
    /// <returns>The encoded string.</returns>
    public static string ToHexString(this ReadOnlySpan<byte> bytes)
    {
        Span<char> buffer = stackalloc char[bytes.Length * 2];

        for (int i = 0; i < bytes.Length; i++)
        {
            buffer[i * 2] = ToHex((byte)((bytes[i] >> 4) & 0xF));
            buffer[(i * 2) + 1] = ToHex((byte)(bytes[i] & 0xF));
        }

        return new string(buffer);

        char ToHex(byte b)
        {
            return (char)(b < 10 ? '0' + b : 'a' + (b - 10));
        }
    }

    public static byte ReadByte(this ReadOnlySpan<byte> bytes, ref int offset)
    {
        var value = bytes[offset];
        offset++;
        return value;
    }

    public static ushort ReadUInt16BigEndian(this ReadOnlySpan<byte> bytes, ref int offset)
    {
        var value = BinaryPrimitives.ReadUInt16BigEndian(bytes[offset..]);
        offset += sizeof(ushort);
        return value;
    }
}
