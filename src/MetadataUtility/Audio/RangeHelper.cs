// <copyright file="RangeHelper.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio
{
    public static class RangeHelper
    {
        /// <summary>
        /// Reads a given range from a FLAC file stream.
        /// </summary>
        /// <param name="stream">The FLAC file stream.</param>
        /// <param name="range">The range to read.</param>
        /// <returns>A span containing the contents of the stream in the range.</returns>
        public static ReadOnlySpan<byte> ReadRange(Stream stream, Range range)
        {
            Span<byte> buffer = new byte[range.Length];

            if (stream.Seek(range.Start, SeekOrigin.Begin) != range.Start)
            {
                throw new IOException("ReadRange: could not seek to position");
            }

            var read = stream.Read(buffer);

            if (read != range.Length)
            {
                throw new InvalidOperationException("ReadRange: read != range.Length");
            }

            return buffer;
        }

        /// <summary>
        /// Reads a given range from a FLAC file stream asynchronously.
        /// </summary>
        /// <param name="stream">The FLAC file stream.</param>
        /// <param name="range">The range to read.</param>
        /// <returns>A byte array containing the contents of the stream in the range.</returns>
        public static async ValueTask<byte[]> ReadRangeAsync(Stream stream, Range range)
        {
            byte[] buffer = new byte[range.Length];

            if (stream.Seek(range.Start, SeekOrigin.Begin) != range.Start)
            {
                throw new IOException("ReadRange: could not seek to position");
            }

            var read = await stream.ReadAsync(buffer);

            if (read != range.Length)
            {
                throw new InvalidOperationException("ReadRange: read != range.Length");
            }

            return buffer;
        }

        public partial record Range(long Start, long End);

        public partial record Range
        {
            public long Length => this.End - this.Start;
        }
    }
}
