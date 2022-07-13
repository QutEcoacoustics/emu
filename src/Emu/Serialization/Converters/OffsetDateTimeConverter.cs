// <copyright file="OffsetDateTimeConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Serialization
{
    using System;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// A CsvHelper converter for Nodatime <see cref="OffsetDateTime"/> values.
    /// </summary>
    public class OffsetDateTimeConverter : DefaultTypeConverter
    {
        /// <inheritdoc />
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var date = (OffsetDateTime)value;

            if (date.Calendar != CalendarSystem.Iso)
            {
                throw new ArgumentException(
                    $"Values of type {nameof(OffsetDateTime)} must (currently) use the ISO calendar in order to be serialized.");
            }

            return OffsetDateTimePattern.Rfc3339.Format(date);
        }

        /// <inheritdoc />
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return text == null
#pragma warning disable CS8604 // Possible null reference argument.
                ? base.ConvertFromString(text, row, memberMapData)
#pragma warning restore CS8604 // Possible null reference argument.
                : OffsetDateTimePattern.Rfc3339.Parse(text).Value;
        }
    }
}
