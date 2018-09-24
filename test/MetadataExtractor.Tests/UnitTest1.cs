using System;
using Xunit;

namespace MetadataExtractor.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var expected = 35;
            var actual = MetadataExtractor.Program.Test(7, 5);
            Assert.Equal(expected, actual);
        }
    }
}
