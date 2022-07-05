// <copyright file="RangeExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Extensions.System
{
    public static class RangeExtensions
    {
        public static int Length(this Range range)
        {
            return range.GetOffsetAndLength(int.MaxValue).Length;
        }

        public static string FormatInterval(this Range range, Func<int, string> valueConverter)
        {
            return $"[{FormatIndex(range.Start)}, {FormatIndex(range.End)})";

            string FormatIndex(Index i)
            {
                return (i.IsFromEnd ? "-" : string.Empty) + valueConverter(i.Value);
            }
        }
    }
}
