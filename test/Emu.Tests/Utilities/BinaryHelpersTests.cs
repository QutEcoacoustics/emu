// <copyright file="BinaryHelpersTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Utilities
{
    using System;
    using Emu.Utilities;
    using Xunit;

    public class BinaryHelpersTests
    {
        // Verify first 4 bits are correctly ignored in the five byte sequence
        [Theory]
        [InlineData(new byte[] { 0b1111_1111, 0xFF, 0xFF, 0xFF, 0xFF }, 68_719_476_735u)]
        [InlineData(new byte[] { 0b0000_1111, 0xFF, 0xFF, 0xFF, 0xFF }, 68_719_476_735u)]
        [InlineData(new byte[] { 0b0101_1111, 0xFF, 0xFF, 0xFF, 0xFF }, 68_719_476_735u)]
        [InlineData(new byte[] { 0b1010_1111, 0xFF, 0xFF, 0xFF, 0xFF }, 68_719_476_735u)]
        [InlineData(new byte[] { 0b1111_0000, 0x00, 0x00, 0x00, 0x00 }, 0)]
        [InlineData(new byte[] { 0b0000_0000, 0x00, 0x00, 0x00, 0x00 }, 0)]
        [InlineData(new byte[] { 0b0101_0000, 0x00, 0x00, 0x00, 0x00 }, 0)]
        [InlineData(new byte[] { 0b1010_0000, 0x00, 0x00, 0x00, 0x00 }, 0)]
        [InlineData(new byte[] { 0b1010_1010, 0xAA, 0xAA, 0xAA, 0xAA }, 45_812_984_490u)]
        [InlineData(new byte[] { 0b1111_1010, 0xAA, 0xAA, 0xAA, 0xAA }, 45_812_984_490u)]
        public void CanRead36BitIntegers(byte[] input, ulong expected)
        {
            var actual = BinaryHelpers.Read36BitUnsignedBigEndianIgnoringFirstNibble(input);

            Assert.Equal(expected, actual);
        }

        // Verify last 4 bits are correctly ignored in the three byte sequence
        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0b1111_1111 }, 1_048_575u)]
        [InlineData(new byte[] { 0xFF, 0xFF, 0b1111_0000 }, 1_048_575u)]
        [InlineData(new byte[] { 0xFF, 0xFF, 0b1111_0101 }, 1_048_575u)]
        [InlineData(new byte[] { 0xFF, 0xFF, 0b1111_1010 }, 1_048_575u)]
        [InlineData(new byte[] { 0x00, 0x00, 0b0000_1111 }, 0)]
        [InlineData(new byte[] { 0x00, 0x00, 0b0000_0000 }, 0)]
        [InlineData(new byte[] { 0x00, 0x00, 0b0000_0101 }, 0)]
        [InlineData(new byte[] { 0x00, 0x00, 0b0000_1010 }, 0)]
        [InlineData(new byte[] { 0xAA, 0xAA, 0b1010_1010 }, 699_050u)]
        [InlineData(new byte[] { 0xAA, 0xAA, 0b1010_1111 }, 699_050u)]
        public void CanRead20BitIntegers(byte[] input, ulong expected)
        {
            var actual = BinaryHelpers.Read20BitUnsignedBigEndianIgnoringLastNibble(input);

            Assert.Equal(expected, actual);
        }

        // Verify the first 4 and the last bit are correctly ignored in the byte
        [Theory]
        [InlineData(new byte[] { 0b1111_1111 }, 7u)]
        [InlineData(new byte[] { 0b0000_1110 }, 7u)]
        [InlineData(new byte[] { 0b1010_1111 }, 7u)]
        [InlineData(new byte[] { 0b1111_0001 }, 0)]
        [InlineData(new byte[] { 0b0000_0000 }, 0)]
        [InlineData(new byte[] { 0b1010_0001 }, 0)]
        [InlineData(new byte[] { 0b1111_1011 }, 5u)]
        [InlineData(new byte[] { 0b0000_1010 }, 5u)]
        [InlineData(new byte[] { 0b1010_1011 }, 5u)]
        public void CanRead3BitIntegers(byte[] input, ulong expected)
        {
            var actual = BinaryHelpers.Read3BitUnsignedBigEndianIgnoringFirstFourAndLastBit(input);

            Assert.Equal(expected, actual);
        }

        // Verify first 7 and last 4 bits are correctly ignored in the two byte sequence
        [Theory]
        [InlineData(new byte[] { 0b1111_1111, 0b1111_1111 }, 31u)]
        [InlineData(new byte[] { 0b0000_0001, 0b1111_0000 }, 31u)]
        [InlineData(new byte[] { 0b1010_1011, 0b1111_0101 }, 31u)]
        [InlineData(new byte[] { 0b1111_1110, 0b0000_1111 }, 0)]
        [InlineData(new byte[] { 0b0000_0000, 0b0000_0000 }, 0)]
        [InlineData(new byte[] { 0b1010_1010, 0b0000_0101 }, 0)]
        [InlineData(new byte[] { 0b1111_1111, 0b0101_1111 }, 21u)]
        [InlineData(new byte[] { 0b0000_0001, 0b0101_0000 }, 21u)]
        [InlineData(new byte[] { 0b1010_1011, 0b0101_0101 }, 21u)]
        public void CanRead5BitIntegers(byte[] input, ulong expected)
        {
            var actual = BinaryHelpers.Read5BitUnsignedBigEndianIgnoringFirstSevenAndLastFourBits(input);

            Assert.Equal(expected, actual);
        }

        // Verify first bit is correctly ignored in the two byte sequence
        [Theory]
        [InlineData(new byte[] { 0b1111_1111 }, 127u)]
        [InlineData(new byte[] { 0b0111_1111 }, 127u)]
        [InlineData(new byte[] { 0b1000_0000 }, 0)]
        [InlineData(new byte[] { 0b0000_0000 }, 0)]
        [InlineData(new byte[] { 0b1101_0101 }, 85u)]
        [InlineData(new byte[] { 0b0101_0101 }, 85u)]
        public void CanRead7BitIntegers(byte[] input, ulong expected)
        {
            var actual = BinaryHelpers.Read7BitUnsignedBigEndianIgnoringFirstBit(input);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new byte[] { 0b1111_1111, 0xFF, 0xFF, 0xFF, 0xFF }, 68_719_476_735u, 0b1111_1111)]
        [InlineData(new byte[] { 0b0000_1111, 0xFF, 0xFF, 0xFF, 0xFF }, 68_719_476_735u, 0b0000_1111)]
        [InlineData(new byte[] { 0b0101_1111, 0xFF, 0xFF, 0xFF, 0xFF }, 68_719_476_735u, 0b0101_1111)]
        [InlineData(new byte[] { 0b1010_1111, 0xFF, 0xFF, 0xFF, 0xFF }, 68_719_476_735u, 0b1010_1111)]
        [InlineData(new byte[] { 0b1111_0000, 0x00, 0x00, 0x00, 0x00 }, 0, 0b1111_1111)]
        [InlineData(new byte[] { 0b0000_0000, 0x00, 0x00, 0x00, 0x00 }, 0, 0b0000_1111)]
        [InlineData(new byte[] { 0b0101_0000, 0x00, 0x00, 0x00, 0x00 }, 0, 0b0101_1111)]
        [InlineData(new byte[] { 0b1010_0000, 0x00, 0x00, 0x00, 0x00 }, 0, 0b1010_1111)]
        [InlineData(new byte[] { 0b1010_1010, 0xAA, 0xAA, 0xAA, 0xAA }, 45_812_984_490u, 0b1010_1010)]
        [InlineData(new byte[] { 0b1111_1010, 0xAA, 0xAA, 0xAA, 0xAA }, 45_812_984_490u, 0b1111_1010)]
        public void CanWrite36BitIntegers(byte[] expected, ulong input, byte firstByte)
        {
            var actual = new byte[5];
            actual[0] = firstByte;

            BinaryHelpers.Write36BitUnsignedBigEndianIgnoringFirstNibble(actual, input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WritingAnOutOfBound36BitNumberThrows()
        {
            var actual = new byte[5];

            var error = Assert.Throws<ArgumentException>(
                "value",
                () => BinaryHelpers.Write36BitUnsignedBigEndianIgnoringFirstNibble(actual, 68_719_476_736u));

            Assert.Contains("is outside the representable range", error.Message);
        }

        [Fact]
        public void WritingToSmallBufferThrows()
        {
            var actual = new byte[4];

            var error = Assert.Throws<ArgumentException>(
                "bytes",
                () => BinaryHelpers.Write36BitUnsignedBigEndianIgnoringFirstNibble(actual, 68_719_476_735u));

            Assert.Contains("span must at least be 5 long", error.Message);
        }

        [Fact]
        public void ReadingFromSmallBufferThrows()
        {
            var actual = new byte[4];

            var error = Assert.Throws<ArgumentException>(
                "bytes",
                () => BinaryHelpers.Read36BitUnsignedBigEndianIgnoringFirstNibble(actual));

            Assert.Contains("span must at least be 5 long", error.Message);
        }
    }
}
