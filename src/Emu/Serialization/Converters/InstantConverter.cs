// <copyright file="InstantConverter.cs" company="QutEcoacoustics">
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
    /// A CsvHelper converter for Nodatime <see cref="Instant"/> values.
    /// </summary>
    public class InstantConverter : DefaultTypeConverter
    {
        /// <inheritdoc />
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var instant = (Instant)value;

            return InstantPattern.General.Format(instant);
        }

        /// <inheritdoc />
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return text == null
#pragma warning disable CS8604 // Possible null reference argument.
                ? base.ConvertFromString(text, row, memberMapData)
#pragma warning restore CS8604 // Possible null reference argument.
                : InstantPattern.General.Parse(text).Value;
        }
    }
}
