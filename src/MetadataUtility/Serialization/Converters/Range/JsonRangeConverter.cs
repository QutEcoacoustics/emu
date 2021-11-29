// <copyright file="JsonRangeConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Serialization.Converters
{
    using MetadataUtility.Extensions.System;
    using Newtonsoft.Json;

    public class JsonRangeConverter : JsonConverter<Range>
    {
        public override Range ReadJson(JsonReader reader, Type objectType, Range existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, Range value, JsonSerializer serializer)
        {
            writer.WriteValue(value.FormatInterval((i) => i.ToString(serializer.Culture)));
        }
    }
}
