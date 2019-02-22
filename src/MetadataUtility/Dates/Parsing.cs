// <copyright file="Parsing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Dates
{
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// Functions used for extracting durations from strings.
    /// </summary>
    public class Parsing
    {
        private static readonly OffsetPattern NoSignPattern;
        private static readonly OffsetPattern NoTimeSeparatorPattern;

        static Parsing()
        {
            NoSignPattern = OffsetPattern.CreateWithInvariantCulture("HH:mm");
            NoTimeSeparatorPattern = OffsetPattern.CreateWithInvariantCulture("-HHmm");
        }

        /// <summary>
        /// Attempts to parse a duration.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <param name="duration">The duration that was parsed if parsing was successful.</param>
        /// <returns>Whether or not parsing was successful.</returns>
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

        /// <summary>
        /// Attempts to parse a UTC offset.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <param name="offset">The offset that was parsed if parsing was successful.</param>
        /// <returns>Whether or not parsing was successful.</returns>
        public static bool TryParseOffset(string input, out Offset offset)
        {
            var parseResult = OffsetPattern.GeneralInvariantWithZ.Parse(input);
            if (parseResult.Success)
            {
                offset = parseResult.Value;
                return true;
            }

            parseResult = NoSignPattern.Parse(input);
            if (parseResult.Success)
            {
                offset = parseResult.Value;
                return true;
            }

            parseResult = NoTimeSeparatorPattern.Parse(input);
            if (parseResult.Success)
            {
                offset = parseResult.Value;
                return true;
            }

            offset = default;
            return false;
        }
    }
}
