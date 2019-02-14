// <copyright file="TimeParsing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Dates
{
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// Functions used for extracting durations from strings.
    /// </summary>
    public class DurationParsing
    {
        public static bool TryParseDuration(string input, out Duration duration)
        {
            var parseResult = DurationPattern.Roundtrip.Parse(input);
            if (parseResult.Success)
            {
                duration = parseResult.Value;
                return true;
            }

            duration = default;
            return false;
        }

        public static bool TryParseOffset(string input, out Offset duration)
        {
            var parseResult = OffsetPattern.GeneralInvariantWithZ.Parse(input);
            if (parseResult.Success)
            {
                duration = parseResult.Value;
                return true;
            }

            duration = default;
            return false;
        }
    }
}
