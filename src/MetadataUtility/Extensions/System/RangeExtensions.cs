// <copyright file="RangeExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Extensions.System
{
    public static class RangeExtensions
    {
        public static int Length(this Range range)
        {
            return range.GetOffsetAndLength(int.MaxValue).Length;
        }
    }
}
