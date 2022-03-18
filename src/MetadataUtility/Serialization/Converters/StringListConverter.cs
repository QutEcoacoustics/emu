// <copyright file="StringListConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Serialization.Converters
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;

    public class StringListConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text != null && !text.Equals(string.Empty))
            {
                return text.Split(";");
            }

            return null;
        }
    }
}
