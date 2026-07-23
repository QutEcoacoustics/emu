// <copyright file="FinTimestampConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using LanguageExt;
    using LanguageExt.Common;
    using Newtonsoft.Json;
    using NodaTime;

    public class FinTimestampConverter : JsonConverter<Fin<Either<LocalDateTime, OffsetDateTime>>>
    {
        public override bool CanWrite => true;

        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, Fin<Either<LocalDateTime, OffsetDateTime>> value, JsonSerializer serializer)
        {
            if (value.IsSucc)
            {
                value.ThrowIfFail().Match(
                    Left: local =>
                    {
                        serializer.Serialize(writer, local);
                        return 0;
                    },
                    Right: offset =>
                    {
                        serializer.Serialize(writer, offset);
                        return 0;
                    });

                return;
            }

            serializer.Serialize(writer, (Error)value);
        }

        public override Fin<Either<LocalDateTime, OffsetDateTime>> ReadJson(JsonReader reader, Type objectType, Fin<Either<LocalDateTime, OffsetDateTime>> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new JsonReaderException($"{nameof(FinTimestampConverter)} only supports writing JSON.");
        }
    }
}
