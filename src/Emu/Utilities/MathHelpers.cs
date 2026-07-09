// <copyright file="MathHelpers.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Utilities
{
    using System;

    public static class MathHelpers
    {
        /// <summary>
        /// Calculates the Shannon entropy of a byte sequence in bits.
        /// Maximum entropy for 256 possible byte values is 8.0 bits (perfectly uniform distribution).
        /// Structured data (e.g. audio, headers) will have significantly lower entropy.
        /// </summary>
        /// <param name="data">The byte data to analyze.</param>
        /// <returns>The Shannon entropy in bits, ranging from 0.0 to 8.0.</returns>
        public static double CalculateEntropy(ReadOnlySpan<byte> data)
        {
            if (data.Length == 0)
            {
                return 0.0;
            }

            Span<int> counts = stackalloc int[256];
            counts.Clear();

            for (int i = 0; i < data.Length; i++)
            {
                counts[data[i]]++;
            }

            double entropy = 0.0;
            double length = data.Length;

            for (int i = 0; i < 256; i++)
            {
                if (counts[i] == 0)
                {
                    continue;
                }

                double p = counts[i] / length;
                entropy -= p * Math.Log2(p);
            }

            return entropy;
        }
    }
}
