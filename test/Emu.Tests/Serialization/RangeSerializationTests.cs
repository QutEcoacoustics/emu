// <copyright file="RangeSerializationTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Serialization
{
    using System;
    using System.IO;
    using System.Text;
    using Emu.Serialization;
    using Emu.Tests.TestHelpers;
    using Xunit;

    public class RangeSerializationTests
    {
        public static readonly Range Range = 1..2;
        public static readonly Range RangeFromEnd = ^4..^2;

        [Fact]
        public void JsonRangeIsSerializedAsAnInterval()
        {
            var serializer = new JsonSerializer();
            var wrapper = new Wrapper(Range);

            var builder = WriteRecord(serializer, wrapper);

            Assert.Equal(
                @"[
  {
    ""Range"": ""[1, 2)""
  }
]".NormalizeLineEndings(),
                builder.ToString());
        }

        [Fact]
        public void JsonRangeFromEndIsSerializedAsAnInterval()
        {
            var serializer = new JsonSerializer();
            var wrapper = new Wrapper(RangeFromEnd);

            var builder = WriteRecord(serializer, wrapper);

            Assert.Equal(
                @"[
  {
    ""Range"": ""[-4, -2)""
  }
]".NormalizeLineEndings(),
                builder.ToString());
        }

        [Fact]
        public void JsonLinesRangeIsSerializedAsAnInterval()
        {
            var serializer = new JsonLinesSerializer();
            var wrapper = new Wrapper(Range);

            var builder = WriteRecord(serializer, wrapper);

            Assert.Equal(
                @"{""Range"":""[1, 2)""}
".NormalizeLineEndings(),
                builder.ToString());
        }

        [Fact]
        public void JsonLinesRangeFromEndIsSerializedAsAnInterval()
        {
            var serializer = new JsonLinesSerializer();
            var wrapper = new Wrapper(RangeFromEnd);

            var builder = WriteRecord(serializer, wrapper);

            Assert.Equal(
                @"{""Range"":""[-4, -2)""}
".NormalizeLineEndings(),
                builder.ToString());
        }

        [Fact]
        public void CsvRangeIsSerializedAsAnInterval()
        {
            var serializer = new CsvSerializer();
            var wrapper = new Wrapper(Range);

            var builder = WriteRecord(serializer, wrapper);

            // CSV always outputs \r\n
            Assert.Equal(
                @"Range
""[1, 2)""
".NormalizeLineEndings("\r\n"),
                builder.ToString());
        }

        [Fact]
        public void CsvRangeFromEndIsSerializedAsAnInterval()
        {
            var serializer = new CsvSerializer();
            var wrapper = new Wrapper(RangeFromEnd);

            var builder = WriteRecord(serializer, wrapper);

            // CSV always outputs \r\n
            Assert.Equal(
                @"Range
""[-4, -2)""
".NormalizeLineEndings("\r\n"),
                builder.ToString());
        }

        private static StringBuilder WriteRecord(IRecordFormatter serializer, Wrapper wrapper)
        {
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                var context = serializer.WriteHeader(null, writer, wrapper);
                context = serializer.WriteRecord(context, writer, wrapper);
                serializer.Dispose(context, writer);
            }

            return builder;
        }

        public class Wrapper
        {
            public Wrapper(Range range)
            {
                this.Range = range;
            }

            public Range Range { get; }
        }
    }
}
