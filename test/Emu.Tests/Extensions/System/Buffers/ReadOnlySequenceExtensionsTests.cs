// <copyright file="ReadOnlySequenceExtensionsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Extensions.System
{
    using global::System;
    using global::System.Buffers;
    using Xunit;

    public class ReadOnlySequenceExtensionsTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1024, 1024)]
        [InlineData(4096, 4096)]
        public void TestGetRelativeOffsetSingleSegment(long advance, long expectedPosition)
        {
            var segment = new byte[4096];
            var sequence = new ReadOnlySequence<byte>(segment);
            var reader = new SequenceReader<byte>(sequence);

            reader.Advance(advance);

            var actualPosition = sequence.GetSequenceOffset(reader.Position);

            Assert.Equal(expectedPosition, actualPosition);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(511, 511)]
        [InlineData(512, 512)]
        [InlineData(1024, 1024)]
        [InlineData(2048, 2048)]
        public void TestGetRelativeOffsetMultiSegment(long advance, long expectedPosition)
        {
            var segmentA = new byte[1024];
            var segmentB = new byte[1024];
            var segmentC = new byte[1024];

            var bufferA = new BufferSegment<byte>(segmentA);

            var sequence = new ReadOnlySequence<byte>(
                bufferA,
                512,
                bufferA.Append(segmentB).Append(segmentC),
                512);

            var reader = new SequenceReader<byte>(sequence);

            reader.Advance(advance);

            var actualPosition = sequence.GetSequenceOffset(reader.Position);

            Assert.Equal(expectedPosition, actualPosition);
        }

        internal class BufferSegment<T> : ReadOnlySequenceSegment<T>
        {
            public BufferSegment(ReadOnlyMemory<T> memory)
            {
                this.Memory = memory;
            }

            public BufferSegment<T> Append(ReadOnlyMemory<T> memory)
            {
                var segment = new BufferSegment<T>(memory)
                {
                    RunningIndex = this.RunningIndex + this.Memory.Length,
                };
                this.Next = segment;
                return segment;
            }
        }
    }
}
