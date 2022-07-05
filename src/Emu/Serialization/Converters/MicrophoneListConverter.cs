// <copyright file="MicrophoneListConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Serialization
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using Emu.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// A CsvHelper converter for a list of Microphones <see cref="Microphone"/> values.
    /// </summary>
    public class MicrophoneListConverter : DefaultTypeConverter
    {
        /// <inheritdoc />
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject((Microphone[])value);
        }

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text != null)
            {
                return JsonConvert.DeserializeObject<Microphone[]>(text);
            }

            return null;
        }
    }
}
