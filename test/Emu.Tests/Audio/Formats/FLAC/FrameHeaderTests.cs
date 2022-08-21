// <copyright file="FrameHeaderTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Formats.FLAC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Audio;
    using FluentAssertions;
    using LanguageExt.Common;
    using Xunit;

    public class FrameHeaderTests
    {
        [Fact]
        public void ThrowsIfGivenLessThan16Bytes()
        {
            Assert.Throws<ArgumentException>(() => FrameHeader.Parse(new byte[15], 0, 0, out var _));
        }

        [Theory]
        [ClassData(typeof(ValidFrames))]
        public void ParseWorks(byte[] frameHeader, FrameHeader expected)
        {
            // use intentionally weird numbers here to test edge cases
            var actual = FrameHeader.Parse(PadBytes(frameHeader), 12345, 31, out var actualConsumed);

            actual.ThrowIfFail().Should().BeEquivalentTo(expected);

            actualConsumed.Should().Be(frameHeader.Length);
        }

        [Theory]
        [ClassData(typeof(InvalidFrames))]
        public void ParseFailsWithBadData(byte[] frameHeader, string expectedError)
        {
            // use intentionally weird numbers here to test edge cases
            var actual = FrameHeader.Parse(PadBytes(frameHeader), 12345, 31, out var _);

            actual.IsFail.Should().BeTrue();

            ((Error)actual).Message.Should().Be(expectedError);
        }

        private static byte[] PadBytes(byte[] input)
        {
            var dest = new byte[16];
            Array.Copy(input, dest, input.Length);
            return dest;
        }

        private class ValidFrames : TheoryData<byte[], FrameHeader>
        {
            public ValidFrames()
            {
                // block size is 8-bit, read further down
                this.Add(
                    new byte[] { 0b11111111, 0b11111000, 0b01101001, 0b00011000, 0b00000000, 0b00000000, 0b10111111 },
                    new FrameHeader(FrameBlockingStrategy.Fixed, 1, 44_100, FrameChannelAssignment.LeftRight, 16, 0, null, 0xbf));

                // block size is  8-bit, read further down
                this.Add(
                    new byte[] { 0b11111111, 0b11111000, 0b01101001, 0b10011000, 0b00000000, 0b00001111, 0b10011001 },
                    new FrameHeader(FrameBlockingStrategy.Fixed, 16, 44_100, FrameChannelAssignment.RightPlusSideStereo, 16, 0, null, 0x99));

                // block size is  8-bit, read further down
                this.Add(
                    new byte[] { 0b11111111, 0b11111000, 0b01101000, 0b00000010, 0b00000000, 0b00010111, 0b11101001 },
                    new FrameHeader(FrameBlockingStrategy.Fixed, 24, 32_000, FrameChannelAssignment.Mono, 8, 0, null, 0xe9));

                // block size is  16-bit, read further down
                this.Add(
                    new byte[] { 0b11111111, 0b11111000, 0b01111000, 0b00000010, 0b00000000, 0b10011100, 0b01000000, 0b01000101 },
                    new FrameHeader(FrameBlockingStrategy.Fixed, 40_001, 32_000, FrameChannelAssignment.Mono, 8, 0, null, 0x45));

                // sample rate comes from header
                this.Add(
                    new byte[] { 0b11111111, 0b11111000, 0b11100000, 0b00000010, 0b00000000, 0b01101110 },
                    new FrameHeader(FrameBlockingStrategy.Fixed, 16384, 12345, FrameChannelAssignment.Mono, 8, 0, null, 0x6e));

                // bit depth comes from header
                this.Add(
                    new byte[] { 0b11111111, 0b11111000, 0b11101011, 0b00000000, 0b00000000, 0b11101001 },
                    new FrameHeader(FrameBlockingStrategy.Fixed, 16384, 96_000, FrameChannelAssignment.Mono, 31, 0, null, 0xe9));

                // block size is  16-bit, read further down, sample rate is 16-bit, read further down
                this.Add(
                    new byte[] { 0b11111111, 0b11111000, 0b01111110, 0b10011000, 0b00000000, 0b10100010, 0b10000001, 0b00110000, 0b00111001, 0x64 },
                    new FrameHeader(FrameBlockingStrategy.Fixed, 41_602, 123_450, FrameChannelAssignment.RightPlusSideStereo, 16, 0, null, 0x64));

                // from one of our sample files, first frame (it's set to get sample rate from stream info)
                this.Add(
                    new byte[] { 0b11111111, 0b11111000, 0b00110000, 0b00001000, 0b00000000, 0b11000011 },
                    new FrameHeader(FrameBlockingStrategy.Fixed, 1152, 12345, FrameChannelAssignment.Mono, 16, 0, null, 0xc3));

                // from one of our sample files, nearly the last frame
                this.Add(
                    new byte[] { 0b11111111, 0b11111000, 0b00110000, 0b00001000, 0b11110000, 0b10100001, 0b10100111, 0b10001100, 0b11001110 },
                    new FrameHeader(FrameBlockingStrategy.Fixed, 1152, 12345, FrameChannelAssignment.Mono, 16, 137_676, null, 0xce));

                // failing case from a test file
                this.Add(
                    new byte[] { 0b11111111, 0b11111000, 0b11001001, 0b10101000, 0b00100000, 0b01101101 },
                    new FrameHeader(FrameBlockingStrategy.Fixed, 4096, 44100, FrameChannelAssignment.MidPlusSideStereo, 16, 32, null, 0x6d));
            }
        }

        private class InvalidFrames : TheoryData<byte[], string>
        {
            public InvalidFrames()
            {
                this.Add(
                    new byte[] { 0b11111011, 0b11111000, 0b01101001, 0b00011000, 0b00000000, 0b00000000, 0b10111111 },
                    "Invalid Frame Header: Sync code not found at start of span");

                // one-byte lag
                this.Add(
                    new byte[] { 0b00000000,  0b11111111, 0b11111000, 0b01101001, 0b00011000, 0b00000000, 0b00000000, 0b10111111 },
                    "Invalid Frame Header: Sync code not found at start of span");
            }
        }
    }
}
