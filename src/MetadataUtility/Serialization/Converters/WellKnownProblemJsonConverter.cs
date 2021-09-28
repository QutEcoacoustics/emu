// <copyright file="WellKnownProblemJsonConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.ComponentModel;
    using System.Globalization;
    using Newtonsoft.Json;

    public class WellKnownProblemJsonConverter : JsonConverter<WellKnownProblem>
    {
        public override void WriteJson(JsonWriter writer, WellKnownProblem value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Id.ToString());
        }

        public override WellKnownProblem ReadJson(JsonReader reader, Type objectType, WellKnownProblem existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;

            if (WellKnownProblems.TryLookup(s, out var problem))
            {
                return problem;
            }
            else
            {
                return null;
            }
        }

    }

    public class WellKnownProblemTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(WellKnownProblem))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
            {
                if (WellKnownProblems.TryLookup(s, out var problem))
                {
                    return problem;
                }
                else
                {
                    return null;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is WellKnownProblem p)
            {
                return p.Id;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
