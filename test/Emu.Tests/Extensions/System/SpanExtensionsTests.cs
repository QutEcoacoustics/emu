// <copyright file="SpanExtensionsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Extensions.System
{
    using FluentAssertions;
    using global::System;
    using Xunit;

    public class SpanExtensionsTests
    {
        // test byte[] to hex string
        [Theory]
        [InlineData(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, "000102030405060708090a0b0c0d0e0f")]
        [InlineData(new byte[] { 0xFF, 0xEE, 0xDD, 0xCC, 0xBB, 0xAA, 0x99, 0x88, 0x77, 0x66, 0x55, 0x44, 0x33, 0x22, 0x11, 0x00 }, "ffeeddccbbaa99887766554433221100")]
        public void TestByteArrayToHexString(byte[] bytes, string expected)
        {
            var actual = bytes.ToHexString();
            Assert.Equal(expected, actual);

            var reversed = actual.FromHexString();
            reversed.Should().BeEquivalentTo(bytes);
        }
    }
}
