// <copyright file="ReadOnlySequenceExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace System.Buffers
{
    using global::System;

    public static class ReadOnlySequenceExtensions
    {
        /// <summary>
        /// Returns an offset into the sequence as a whole - as if the sequence were just one span.
        /// </summary>
        /// <remarks>
        /// Differs from <see cref="ReadOnlySequence{T}.GetOffset(SequencePosition)"/> in that GetOffset
        /// returns the offset of position relative to the buffer in which it was originally from.
        /// This is not so useful if you're treating the ReadOnlySequence as if it were one continuous span.
        /// </remarks>
        /// <typeparam name="T">The type of item.</typeparam>
        /// <param name="sequence">The sequence to examine.</param>
        /// <param name="position">The position to get an offset for.</param>
        /// <returns>The position's offset into the sequence.</returns>
        public static long GetSequenceOffset<T>(this ReadOnlySequence<T> sequence, SequencePosition position)
        {
            if (sequence.IsSingleSegment)
            {
                return sequence.GetOffset(position);
            }

            var start = sequence.GetPosition(0);

            return sequence.GetOffset(position) - sequence.GetOffset(start);
        }
    }
}
