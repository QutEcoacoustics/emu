// <copyright file="WellKnownProblemJsonConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
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
}
