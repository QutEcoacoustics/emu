// <copyright file="FinJsonConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System.Diagnostics;
    using System.Numerics;
    using LanguageExt;
    using LanguageExt.Common;
    using Newtonsoft.Json;

    /// <summary>
    /// mirrors implementation at https://github.com/louthy/language-ext/blob/d42c12a2c09bc04f9185587fd4f74bafa27d987f/LanguageExt.Core/Monads/Alternative%20Value%20Monads/Fin/Fin.cs#L401-L427.
    /// </summary>
    public class FinJsonConverter<T> : JsonConverter<Fin<T>>
    {
        public FinJsonConverter()
        {
        }

        public override bool CanWrite => true;

        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, Fin<T> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("State");
            writer.WriteValue(value.IsSucc);

            if (value.IsSucc)
            {
                writer.WritePropertyName("Succ");
                serializer.Serialize(writer, (T)value);
            }
            else
            {
                writer.WritePropertyName("Fail");
                serializer.Serialize(writer, (Error)value);
            }

            writer.WriteEndObject();
        }

        public override Fin<T> ReadJson(JsonReader reader, Type objectType, Fin<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                bool state = false;
                T value = default;
                Error error = default;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndObject)
                    {
                        break;
                    }

                    Debug.Assert(reader.TokenType == JsonToken.PropertyName, "must be a name in an object but got" + reader.TokenType);

                    var name = (string)reader.Value;
                    if (name == "State")
                    {
                        state = reader.ReadAsBoolean() ?? throw new JsonReaderException("unexpected value in State");
                    }
                    else if (name == "Succ")
                    {
                        reader.Read();
                        value = serializer.Deserialize<T>(reader);
                    }
                    else if (name == "Fail")
                    {
                        // dirty dirty hack but I am short on time
                        // open question: why isn't [DataContract] on Error working?
                        reader.Read();
                        var dict = serializer.Deserialize<Dictionary<string, object>>(reader);
                        error = Error.New(
                            (dict?.ContainsKey("Code") ?? false) ? (int)(long)dict["Code"] : -1,
                            dict?.GetValueOrDefault("Message")?.ToString() ?? "<unknown message>");
                    }
                    else
                    {
                        throw new JsonReaderException("Unexpected key in Fin object: " + name);
                    }
                }

                return state ? value : error;
            }
            else
            {
                throw new JsonReaderException("Expected an object, got a " + reader.TokenType);
            }
        }
    }
}
