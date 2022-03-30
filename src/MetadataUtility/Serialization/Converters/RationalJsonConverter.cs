// <copyright file="RationalJsonConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.Numerics;
    using Newtonsoft.Json;
    using Rationals;

    public class RationalJsonConverter : JsonConverter<Rational>
    {
        public override bool CanWrite => true;

        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, Rational value, JsonSerializer serializer)
        {
            writer.WriteValue((decimal)value);
        }

        public override Rational ReadJson(JsonReader reader, Type objectType, Rational existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string text = (string)reader.Value;

            if (text != null)
            {
                if (Rational.TryParseDecimal(text, out Rational result))
                {
                    return result;
                }

                string[] args = text.Split("/");

                if (args.Length == 1)
                {
                    return new Rational(BigInteger.Parse(args[0]));
                }
                else
                {
                    return new Rational(BigInteger.Parse(args[0]), BigInteger.Parse(args[1]));
                }
            }

            return default;
        }
    }
}