// <copyright file="DateFormatting.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Dates
{
    using NodaTime;
    using NodaTime.Text;

    public static class DateFormatting
    {
        public static readonly OffsetDateTimePattern CompactOffsetDatePattern = OffsetDateTimePattern.CreateWithInvariantCulture("uuuuMMdd'T'HHmmss;FFFFFFo<Z+HHmm>");
        public static readonly LocalDateTimePattern CompactDatePattern = LocalDateTimePattern.CreateWithInvariantCulture("uuuuMMdd'T'HHmmss;FFFFFF");
        public static readonly LocalDateTimePattern DatePatternISO8601 = LocalDateTimePattern.CreateWithInvariantCulture("S");
        public static readonly DurationPattern DurationISO8601HoursTotal = DurationPattern.CreateWithInvariantCulture("-HH:mm:ss.FFFFFF");
        public static readonly OffsetPattern OffsetPattern = OffsetPattern.CreateWithInvariantCulture("+HH:mm");
        public static readonly LocalTimePattern LocalTimePattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm");
        public static readonly LocalDatePattern LocalDatePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");
        public static readonly OffsetDateTimePattern OffsetDateTimePattern = OffsetDateTimePattern.Rfc3339;

        public static string FormatFileName(OffsetDateTime date)
        {
            return CompactOffsetDatePattern.Format(date);
        }

        public static string FormatFileName(LocalDateTime date)
        {
            return CompactDatePattern.Format(date);
        }
    }
}
