// <copyright file="BinaryHelpersTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Utilities
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Xunit;
    using static Emu.Utilities.BinaryHelpers;

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
            var actual = Read36BitUnsignedBigEndianIgnoringFirstNibble(input);

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
            var actual = Read20BitUnsignedBigEndianIgnoringLastNibble(input);

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
            var actual = Read3BitUnsignedBigEndianIgnoringFirstFourAndLastBit(input);

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
            var actual = Read5BitUnsignedBigEndianIgnoringFirstSevenAndLastFourBits(input);

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
            var actual = Read7BitUnsignedBigEndianIgnoringFirstBit(input);

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

            Write36BitUnsignedBigEndianIgnoringFirstNibble(actual, input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WritingAnOutOfBound36BitNumberThrows()
        {
            var actual = new byte[5];

            var error = Assert.Throws<ArgumentException>(
                "value",
                () => Write36BitUnsignedBigEndianIgnoringFirstNibble(actual, 68_719_476_736u));

            Assert.Contains("is outside the representable range", error.Message);
        }

        [Fact]
        public void WritingToSmallBufferThrows()
        {
            var actual = new byte[4];

            var error = Assert.Throws<ArgumentException>(
                "bytes",
                () => Write36BitUnsignedBigEndianIgnoringFirstNibble(actual, 68_719_476_735u));

            Assert.Contains("span must at least be 5 long", error.Message);
        }

        [Fact]
        public void ReadingFromSmallBufferThrows()
        {
            var actual = new byte[4];

            var error = Assert.Throws<ArgumentException>(
                "bytes",
                () => Read36BitUnsignedBigEndianIgnoringFirstNibble(actual));

            Assert.Contains("span must at least be 5 long", error.Message);
        }

        [Theory]
        [InlineData(new byte[] { 0x00, 0x00 }, false)]
        [InlineData(new byte[] { 0x01, 0x00 }, true)]
        [InlineData(new byte[] { 0x02, 0x00 }, null)]
        [InlineData(new byte[] { 0xFF, 0xFF }, null)]
        [InlineData(new byte[] { 0x00, 0x01 }, null)]
        public void ReadBool16LittleEndianTest(byte[] testCase, bool? expected)
        {
            if (expected.HasValue)
            {
                var actual = ReadBool16LittleEndian(testCase.AsSpan());
                actual.Should().Be(expected.Value);
            }
            else
            {
                Assert.Throws<InvalidOperationException>(
                    () => ReadBool16LittleEndian(testCase.AsSpan()));
            }
        }

        [Theory]
        [InlineData(1, 0, 1, 1)]
        [InlineData(0, 1, 2, 0)]
        [InlineData(1, 7, 12, 0x80)]
        [InlineData(129, 0, 8, 129)]
        [InlineData(4, 29, 32, 0x80_00_00_00)]
        [InlineData(2, 30, 32, 0x80_00_00_00)]
        [InlineData(1, 31, 32, 0x80_00_00_00)]
        [InlineData(0x81, 24, 32, 0x81_00_00_00)]
        [InlineData(0x2_00_01, 7, 25, 0x01_00_00_80)]
        public void ReadingAndWritingBitRangesWorksForUInt(uint expected, byte start, byte end, uint solo)
        {
            const uint TestPattern = 0b1000_0001__0000_0000__0000_0000__1000_0001;

            var actualRead = ReadBitRange(TestPattern, start, end);
            actualRead.Should().Be(expected);

            uint dest = 0;
            WriteBitRange(ref dest, start, end, actualRead);
            dest.Should().Be(solo);

            // there are no misaligned bits
            (dest | TestPattern).Should().Be(TestPattern);

            // it should clear out existing bits when writing
            dest = 0xFF_FF_FF_FF;
            WriteBitRange(ref dest, start, end, actualRead);
            ReadBitRange(dest, start, end).Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 0, 1, 1)]
        [InlineData(0, 1, 2, 0)]
        [InlineData(1, 7, 12, 0x80)]
        [InlineData(129, 0, 8, 129)]
        [InlineData(4, 29, 32, 0x80_00_00_00)]
        [InlineData(2, 30, 32, 0x80_00_00_00)]
        [InlineData(1, 31, 32, 0x80_00_00_00)]
        [InlineData(0x81, 24, 32, 0x81_00_00_00)]
        [InlineData(0x2_00_01, 7, 25, 0x01_00_00_80)]
        [InlineData(15, 60, 64, 0xF0_00_00_00_00_00_00_00)]
        [InlineData(15, 32, 36, 0x00_00_00_0F_00_00_00_00)]
        [InlineData(0xF0_00_00_0F, 32, 64, 0xF0_00_00_0F_00_00_00_00)]
        [InlineData(248, 28, 36, 0x0F_80_00_00_00)]
        public void ReadingAndWritingBitRangesWorksULong(ulong expected, byte start, byte end, ulong solo)
        {
            const ulong TestPattern = 0b1111_0000__0000_0000__0000_0000__0000_1111__1000_0001__0000_0000__0000_0000__1000_0001;

            var actualRead = ReadBitRange(TestPattern, start, end);
            actualRead.Should().Be(expected);

            ulong dest = 0;
            WriteBitRange(ref dest, start, end, actualRead);
            dest.Should().Be(solo);

            // there are no misaligned bits
            (dest | TestPattern).Should().Be(TestPattern);

            // it should clear out existing bits when writing
            dest = 0xFF_FF_FF_FF_FF_FF_FF_FF;
            WriteBitRange(ref dest, start, end, actualRead);
            ReadBitRange(dest, start, end).Should().Be(expected);
        }

        [Theory]
        [InlineData(-1, 0, 1, 1)]
        [InlineData(0, 1, 2, 0)]
        [InlineData(1, 7, 12, 0x80)]
        [InlineData(-127, 0, 8, 129)]
        [InlineData(-4, 29, 32, 0x80_00_00_00)]
        [InlineData(-2, 30, 32, 0x80_00_00_00)]
        [InlineData(-1, 31, 32, 0x80_00_00_00)]
        [InlineData(-1, 60, 64, 0xF0_00_00_00_00_00_00_00)]
        [InlineData(-1, 32, 36, 0x00_00_00_0F_00_00_00_00)]
        [InlineData(-268_435_441, 32, 64, 0xF0_00_00_0F_00_00_00_00)]
        [InlineData(-8, 28, 36, 0x0F_80_00_00_00)]
        public void ReadingAndWritingSignedBitRangesWorksULong(long expected, byte start, byte end, ulong solo)
        {
            const ulong TestPattern = 0b1111_0000__0000_0000__0000_0000__0000_1111__1000_0001__0000_0000__0000_0000__1000_0001;

            var actualRead = ReadSignedBitRange(TestPattern, start, end);
            actualRead.Should().Be(expected);

            ulong dest = 0;
            WriteSignedBitRange(ref dest, start, end, actualRead);
            dest.Should().Be(solo);

            // there are no misaligned bits
            (dest | TestPattern).Should().Be(TestPattern);

            // it should clear out existing bits when writing
            dest = 0xFF_FF_FF_FF_FF_FF_FF_FF;
            WriteSignedBitRange(ref dest, start, end, actualRead);
            ReadSignedBitRange(dest, start, end).Should().Be(expected);
        }
    }
}
