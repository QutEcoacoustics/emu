// <copyright file="RangeExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace System
{
    using System.Text.RegularExpressions;
    using LanguageExt;
    using static LanguageExt.Prelude;

    public static class RangeExtensions
    {
        public static int Length(this Range range, int? collectionLength = null)
        {
            return range
                .GetOffsetAndLength(collectionLength ?? int.MaxValue)
                .Length;
        }

        public static string FormatInterval(this Range range, Func<int, string> valueConverter)
        {
            return $"[{FormatIndex(range.Start)}, {FormatIndex(range.End)})";

            string FormatIndex(Index i)
            {
                return (i.IsFromEnd ? "-" : string.Empty) + valueConverter(i.Value);
            }
        }

        public static Option<Range> AsRange(this Group group)
        {
            if (group is null || !group.Success)
            {
                return None;
            }

            return new Range(group.Index, group.Index + group.Length);
        }

        public static Range MinMax(this IEnumerable<Range> ranges)
        {
            var min = ranges.Select(r => r.Start.Value).Min();
            var max = ranges.Select(r => r.End.Value).Max();
            return new Range(min, max);
        }
    }
}
