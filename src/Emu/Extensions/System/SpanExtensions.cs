// <copyright file="SpanExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace System;

using System.Buffers.Binary;
using System.Runtime.InteropServices;

public static class SpanExtensions
{
    public static string ToHexString(this byte[] bytes)
    {
        return bytes.AsSpan().ToHexString();
    }

    public static string ToHexString(this Span<byte> bytes)
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
}
