// <copyright file="DurationConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Serialization.Converters
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// A CsvHelper converter for Nodatime <see cref="Duration"/> values.
    /// </summary>
    public class DurationConverter : DefaultTypeConverter
    {
        private static readonly DurationPattern DurationPattern = DurationPattern.CreateWithInvariantCulture("-S.FFFFFFFFF");

        /// <inheritdoc />
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var duration = (Duration)value;

            return DurationPattern.Format(duration);
        }

        /// <inheritdoc />
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text == null)
            {
                return base.ConvertFromString(text, row, memberMapData);
            }

            return DurationPattern.Parse(text).Value;
        }
    }
}
