// <copyright file="EmuExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace NodaTime
{
    using System;

    public static class EmuExtensions
    {
        public static Duration MakeDuration(int hours = 0, int minutes = 0, int seconds = 0)
        {
            var negative = hours < 0 || minutes < 0 || seconds < 0;
            var sum =
                Math.Abs(seconds) +
                Math.Abs(minutes * 60) +
                Math.Abs(hours * 60 * 60);

            return Duration.FromSeconds(sum * (negative ? -1 : 1));
        }

        public static bool IsWholeHourOffset(this Offset offset)
        {
            return offset.Seconds % NodaConstants.SecondsPerHour == 0;
        }
    }
}
