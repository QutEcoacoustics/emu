// <copyright file="FlacUTF8Coding.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Formats.FLAC
{
    using System;
    using LanguageExt;
    using LanguageExt.Common;

    /// <summary>
    /// FLAC use a "UTF-8" "style" coding to encode frame numbers.
    /// It's not really UTF-8 so we can't use .NET APIs though.
    /// This is lifted from https://github.com/eaburns/flac/blob/9a6fb92396d1ba6412b82819435dca0b46f959fb/utf8.go#L12.
    /// The original source: https://github.com/xiph/flac/blob/10d7ce268b758b7cf62c3663338f180370a9a415/src/libFLAC/bitreader.c#L1093.
    /// </summary>
    public static class FlacUTF8Coding
    {
        public static readonly Error UnsupportedUTF8Coding = Error.New("Unsupported \"UTF-8\" encoding");
        public static readonly Error NotEnoughBytes = Error.New("Not enough bytes provided to read \"UTF-8\" coded number");
        public static readonly Error BadEncoding = Error.New("Bad continuation bits in \"UTF-8\" coded number");

        public static Fin<ulong> Utf8Decode(ReadOnlySpan<byte> bytes, out int consumed)
        {
            if (bytes.Length < 1)
            {
                throw new ArgumentException("Needs at least 1 bytes to read a number");
            }

            consumed = 0;

            var b0 = bytes.ReadByte(ref consumed);

            int left;
            ulong value;

            // how utf8 coding works: https://en.wikipedia.org/wiki/UTF-8#Encoding
            switch (b0)
            {
                // 0xxx xxxx
                case byte when (b0 & 0x80) == 0:
                    value = (ulong)(b0 & 0x7F);
                    return value;

                // 110x xxxx   10xx xxxx
                case byte when (b0 & 0xE0) == 0xC0:
                    left = 1;

                    value = (ulong)(b0 & 0x1F);
                    break;

                // 1110 xxxx   10xx xxxx   10xx xxxx
                case byte when (b0 & 0xF0) == 0xE0:
                    left = 2;

                    value = (ulong)(b0 & 0xF);
                    break;

                // 1111 0xxx   10xx xxxx   10xx xxxx   10xx xxxx
                case byte when (b0 & 0xF8) == 0xF0:
                    left = 3;

                    value = (ulong)(b0 & 0x7);
                    break;

                // 1111 10xx   10xx xxxx   10xx xxxx   10xx xxxx   10xx xxxx
                case byte when (b0 & 0xFC) == 0xF8:
                    left = 4;

                    value = (ulong)(b0 & 0x3);
                    break;

                // 1111 110x   10xx xxxx   10xx xxxx   10xx xxxx   10xx xxxx   10xx xxxx
                case byte when (b0 & 0xFE) == 0xFC:
                    left = 5;

                    value = (ulong)(b0 & 0x1);
                    break;
                default:
                    return UnsupportedUTF8Coding;
            }

            // first byte already read, just checking remainder
            if ((bytes.Length - 1) < left)
            {
                return NotEnoughBytes;
            }

            for (var n = 0; n < left; n++)
            {
                var b = bytes.ReadByte(ref consumed);

                if ((b & 0xC0) != 0x80)
                {
                    return BadEncoding;
                }

                value = (value << 6) | b & 0x3FUL;
            }

            return value;
        }
    }
}
