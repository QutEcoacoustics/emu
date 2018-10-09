// <copyright file="UnitTest1.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataExtractor.Tests
{
    using System;
    using Xunit;

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var expected = 35;
            MetadataExtractor.EmuEntry.Main(new[] { "test" });
            Assert.Equal(expected, 35 * 20);
        }
    }
}
