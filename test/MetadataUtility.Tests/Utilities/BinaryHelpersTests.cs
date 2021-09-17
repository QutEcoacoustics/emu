// <copyright file="BinaryHelpersTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Utilities
{
    using System;
    using MetadataUtility.Utilities;
    using Xunit;

    public class BinaryHelpersTests
    {
        /*
         * The point of these tests is to see if the first 4-bits are correctly ignored in the five byte sequence
         */

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
            var actual = BinaryHelpers.Read36BitUnsignedBigEndianIgnoringFirstOctet(input);

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

            BinaryHelpers.Write36BitUnsignedBigEndianIgnoringFirstOctet(actual, input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WritingAnOutOfBound36BitNumberThrows()
        {
            var actual = new byte[5];

            var error = Assert.Throws<ArgumentException>(
                "value",
                () => BinaryHelpers.Write36BitUnsignedBigEndianIgnoringFirstOctet(actual, 68_719_476_736u));

            Assert.Contains("is outside the representable range", error.Message);
        }

        [Fact]
        public void WritingToSmallBufferThrows()
        {
            var actual = new byte[4];

            var error = Assert.Throws<ArgumentException>(
                "bytes",
                () => BinaryHelpers.Write36BitUnsignedBigEndianIgnoringFirstOctet(actual, 68_719_476_735u));

            Assert.Contains("span must at least be 5 long", error.Message);
        }

        [Fact]
        public void ReadingFromSmallBufferThrows()
        {
            var actual = new byte[4];

            var error = Assert.Throws<ArgumentException>(
                "bytes",
                () => BinaryHelpers.Read36BitUnsignedBigEndianIgnoringFirstOctet(actual));

            Assert.Contains("span must at least be 5 long", error.Message);
        }
    }
}
