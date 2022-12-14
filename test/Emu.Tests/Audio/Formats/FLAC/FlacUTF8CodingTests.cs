// <copyright file="FlacUTF8CodingTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Formats.FLAC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Audio.Formats.FLAC;
    using FluentAssertions;
    using LanguageExt.Common;
    using Xunit;

    public class FlacUTF8CodingTests
    {
        // Test cases lifted from https://github.com/eaburns/flac/blob/9a6fb92396d1ba6412b82819435dca0b46f959fb/decode_test.go#L12
        [Theory]
        [InlineData(new byte[] { 0x7F }, 0x7F)]
        [InlineData(new byte[] { 0xC2, 0xA2 }, 0xA2)]
        [InlineData(new byte[] { 0xC2, 0x80 }, 0x080)]
        [InlineData(new byte[] { 0xDF, 0xBF }, 0x7FF)]
        [InlineData(new byte[] { 0xE2, 0x82, 0xAC }, 0x20AC)]
        [InlineData(new byte[] { 0xE0, 0xA0, 0x80 }, 0x800)]
        [InlineData(new byte[] { 0xEF, 0xBF, 0xBF }, 0xFFFF)]
        [InlineData(new byte[] { 0xF0, 0x90, 0x80, 0x80 }, 0x10000)]
        [InlineData(new byte[] { 0xF7, 0xBF, 0xBF, 0xBF }, 0x1FFFFF)]
        [InlineData(new byte[] { 0xF0, 0xA4, 0xAD, 0xA2 }, 0x24B62)]
        [InlineData(new byte[] { 0xF8, 0x88, 0x80, 0x80, 0x80 }, 0x200000)]
        [InlineData(new byte[] { 0xFB, 0xBF, 0xBF, 0xBF, 0xBF }, 0x3FFFFFF)]
        [InlineData(new byte[] { 0xFC, 0x84, 0x80, 0x80, 0x80, 0x80 }, 0x4000000)]
        [InlineData(new byte[] { 0xFD, 0xBF, 0xBF, 0xBF, 0xBF, 0xBF }, 0x7FFFFFFF)]
        public void TestDecode(byte[] bytes, ulong expected)
        {
            var result = FlacUTF8Coding.Utf8Decode(bytes, out var consumed);

            result.ThrowIfFail().Should().Be(expected);

            consumed.Should().Be(bytes.Length);
        }

        [Fact]
        public void TestDecodeNotEnoughBytes()
        {
            Assert.Throws<ArgumentException>(
                () => FlacUTF8Coding.Utf8Decode(""u8.ToArray(), out var _));
        }

        [Fact]
        public void TestUnsupportedCoding()
        {
            // the last allowable coding bit in the leading byte is in position 6
            // 1111 110x
            // thus 1111 1110 is invalid
            var subject = new byte[] { 0b1111_1110 };
            var actual = FlacUTF8Coding.Utf8Decode(subject, out var consumed);

            Assert.True(actual.IsFail);
            ((Error)actual).Should().Be(FlacUTF8Coding.UnsupportedUTF8Coding);
            consumed.Should().Be(1);
        }

        [Fact]
        public void TestNotEnoughBytes()
        {
            // given a format
            // 1110 xxxx   10xx xxxx   10xx xxxx
            // then if we omit the last byte there's not enough bytes
            var subject = new byte[] { 0b1110_1111, 0b1011_1111 };
            var actual = FlacUTF8Coding.Utf8Decode(subject, out var consumed);

            Assert.True(actual.IsFail);
            ((Error)actual).Should().Be(FlacUTF8Coding.NotEnoughBytes);
            consumed.Should().Be(1);
        }

        [Fact]
        public void TestBadEncoding()
        {
            // given a format
            // 1110 xxxx   10xx xxxx   10xx xxxx
            // if one of the bytes does not correctly have the continutation bit, then failure
            // 1110 xxxx   11xx xxxx   10xx xxxx
            var subject = new byte[] { 0b1110_1111, 0b1111_1111, 0b1011_1111 };
            var actual = FlacUTF8Coding.Utf8Decode(subject, out var consumed);

            Assert.True(actual.IsFail);
            ((Error)actual).Should().Be(FlacUTF8Coding.BadEncoding);
            consumed.Should().Be(2);
        }
    }
}
