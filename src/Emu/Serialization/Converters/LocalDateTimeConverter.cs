// <copyright file="LocalDateTimeConverter.cs" company="QutEcoacoustics">
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
    /// A CsvHelper converter for Nodatime <see cref="LocalDateTime"/> values.
    /// </summary>
    public class LocalDateTimeConverter : DefaultTypeConverter
    {
        /// <inheritdoc />
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var date = (LocalDateTime)value;

            if (date.Calendar != CalendarSystem.Iso)
            {
                throw new ArgumentException(
                    $"Values of type {nameof(LocalDateTime)} must (currently) use the ISO calendar in order to be serialized.");
            }

            return LocalDateTimePattern.ExtendedIso.Format(date);
        }

        /// <inheritdoc />
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return text == null ? base.ConvertFromString(text, row, memberMapData) : LocalDateTimePattern.ExtendedIso.Parse(text).Value;
        }
    }
}
