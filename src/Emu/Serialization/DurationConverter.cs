// <copyright file="DurationConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Serialization
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
                // ReSharper disable once ExpressionIsAlwaysNull - throws an error
#pragma warning disable CS8604 // Possible null reference argument.
                return base.ConvertFromString(text, row, memberMapData);
#pragma warning restore CS8604 // Possible null reference argument.
            }

            return DurationPattern.Parse(text).Value;
        }
    }
}
