// <copyright file="Formatting.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Dates
{
    using NodaTime;
    using NodaTime.Text;

    public static class DateFormatting
    {
        public static readonly OffsetDateTimePattern OffsetDatePattern = OffsetDateTimePattern.CreateWithInvariantCulture("uuuuMMdd'T'HHmmss;FFFFFFo<Z+HHmm>");
        public static readonly LocalDateTimePattern DatePattern = LocalDateTimePattern.CreateWithInvariantCulture("uuuuMMdd'T'HHmmss;FFFFFF");

        public static string FormatFileName(OffsetDateTime date)
        {
            return OffsetDatePattern.Format(date);
        }

        public static string FormatFileName(LocalDateTime date)
        {
            return DatePattern.Format(date);
        }
    }
}
