// <copyright file="FilenameParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Filenames
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using MetadataUtility.Dates;
    using MetadataUtility.Models;
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// Parses information from filenames.
    /// </summary>
    public partial class FilenameParser
    {
        /// <summary>
        /// A collection of well known local date variants.
        /// </summary>
        public static readonly DateVariant<LocalDateTime>[] PossibleLocalVariants =
        {
            // high precision variant
            // valid: 20091219T070006.789123_00600.wav
            new DateVariant<LocalDateTime>(
                Prefix + Date + InvariantDateTimeSeparator + TimeFractional + End,
                LocalDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss.FFFFFF")),

            // valid: Prefix_YYYYMMDD_hhmmss.wav,
            // valid: prefix_20140101_235959.mp3, a_00000000_000000.a, a_99999999_999999.dnsb48364JSFDSD
            // valid: SERF_20130314_000021_000.wav, a_20130314_000021_a.a, a_99999999_999999_a.dnsb48364JSFDSD
            new DateVariant<LocalDateTime>(
                Prefix + Date + Separator + Time + End,
                LocalDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss")),

            // valid: 20070415051314.wav.trimmed.wav
            new DateVariant<LocalDateTime>(
                Prefix + Date + Time + End,
                LocalDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss")),

            // valid: short_time_180801_1630_test.wav
            new DateVariant<LocalDateTime>(
                Prefix + @"(?<Date>\d{6})" + "_" + @"(?<Time>\d{4})" + End,
                LocalDateTimePattern.CreateWithInvariantCulture("yyMMddTHHmm")),

            // valid: prefix_2359-01012015.mp3, a_2359-01012015.a, a_2359-01012015.dnsb48364JSFDSD
            new DateVariant<LocalDateTime>(
                Prefix + @"(?<Time>\d{4})" + "-" + @"(?<Date>\d{8})" + End,
                LocalDateTimePattern.CreateWithInvariantCulture("ddMMuuuuTHHmm")),
        };

        /// <summary>
        /// A collection of well known date with offset variants.
        /// </summary>
        public static readonly DateVariant<OffsetDateTime>[] PossibleOffsetVariants =
        {
            // high precision variant
            // valid: 20091219T070006.789123+1130_00600.wav
            new DateVariant<OffsetDateTime>(
                Prefix + Date + InvariantDateTimeSeparator + TimeFractional + Offset + End,
                OffsetDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss.FFFFFFo<I>")),

            // valid:Prefix_YYYYMMDD_hhmmssZ.wav
            // valid:prefix_20140101_235959Z.mp3
            // valid: prefix_20140101-235959+10.mp3, a_00000000-000000+00.a, a_99999999-999999+9999.dnsb48364JSFDSD
            // valid: prefix_20140101_235959+10.mp3, a_00000000_000000+00.a, a_99999999_999999+9999.dnsb48364JSFDSD
            // ISO8601-ish (supports a file compatible variant of ISO8601)
            // valid: prefix_20140101T235959+10.mp3, a_00000000T000000+00.a, a_99999999T999999+9999.dnsb48364JSFDSD
            new DateVariant<OffsetDateTime>(
                Prefix + Date + Separator + Time + Offset + End,
                OffsetDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmsso<I>")),

            // an audio moth style date 5AFCD4F4.WAV
            new DateVariant<OffsetDateTime>(
                "^(?<Date>[0-9A-F]{8})" + Extension,
                new AudioMothDateParser()),
        };

        private const string Prefix = @"^(?<Prefix>.*)";
        private const string Suffix = @"(?<Suffix>.*)";
        private const string Extension = @"\.([a-zA-Z0-9]+)$";
        private const string Separator = @"(?<Separator>T|-|_|\$)";
        private const string InvariantDateTimeSeparator = "T";
        private const string End = NoOffset + Suffix + Extension;
        private const string Date = @"(?<Date>\d{8})";
        private const string Time = @"(?<Time>\d{6})";
        private const string TimeFractional = @"(?<Time>\d{6}\.\d{1,6})";
        private const string Offset = @"(?<Offset>[-+][\d:]{2,5}|Z)";
        private const string NoOffset = @"(?![-+\d:]{1,6}|Z)";

        private readonly IEnumerable<DateVariant<LocalDateTime>> localDateVariants;
        private readonly IEnumerable<DateVariant<OffsetDateTime>> offsetDateVariant;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilenameParser"/> class.
        /// </summary>
        /// <param name="localDateVariants">The possible local date strings to parse.</param>
        /// <param name="offsetDateVariant">The possible date with timezone strings to parse.</param>
        public FilenameParser(IEnumerable<DateVariant<LocalDateTime>> localDateVariants, IEnumerable<DateVariant<OffsetDateTime>> offsetDateVariant)
        {
            this.localDateVariants = localDateVariants;
            this.offsetDateVariant = offsetDateVariant;
        }

        /// <summary>
        /// Gets a default <see cref="FilenameParser"/> with prebuilt formats already supplied.
        /// </summary>
        public static FilenameParser Default { get; } = new FilenameParser(PossibleLocalVariants, PossibleOffsetVariants);

        /// <summary>
        /// Attempts to parse information from a filename.
        /// </summary>
        /// <param name="filename">The name of the file to process.</param>
        /// <returns>The parsed information.</returns>
        public ParsedFilename Parse(string filename)
        {
            foreach (var dateVariant in this.offsetDateVariant)
            {
                if (this.TryParse(filename, dateVariant, out var value, out var parsedFilename1))
                {
                    parsedFilename1.LocalDateTime = value.LocalDateTime;
                    parsedFilename1.OffsetDateTime = value;
                    return parsedFilename1;
                }
            }

            foreach (var dateVariant in this.localDateVariants)
            {
                if (this.TryParse(filename, dateVariant, out var value, out var parsedFilename1))
                {
                    parsedFilename1.LocalDateTime = value;
                    return parsedFilename1;
                }
            }

            // finally, if no date can be found find at least
            return new ParsedFilename()
            {
                Extension = Path.GetExtension(filename),
                LocalDateTime = null,
                Location = null /* TODO */,
                OffsetDateTime = null,
                Prefix = Path.GetFileNameWithoutExtension(filename),
                SensorType = null /* TODO */,
                SensorTypeEstimate = 1.0,
                Suffix = string.Empty,
            };
        }

        private bool TryParse<T>(string filename, DateVariant<T> dateVariant, out T value, out ParsedFilename result)
        {
            var match = dateVariant.Regex.Match(filename);
            if (match.Success)
            {
                var timePart = match.Groups[nameof(Time)].Value + match.Groups[nameof(Offset)].Value;
                var parseString = match.Groups[nameof(Date)]
                                  + (timePart == string.Empty ? timePart : InvariantDateTimeSeparator + timePart);

                var parseResult = dateVariant.ParseFormat.Parse(parseString);

                if (parseResult.Success)
                {
                    result = new ParsedFilename()
                    {
                        Extension = "." + match.Groups[nameof(Extension)],
                        Location = null /* TODO */,
                        Prefix = match.Groups[nameof(Prefix)].Value,
                        SensorType = null /* TODO */,
                        SensorTypeEstimate = double.NaN,
                        Suffix = match.Groups[nameof(Suffix)].Value,
                    };
                    value = parseResult.Value;
                    return true;
                }
            }

            value = default;
            result = null;
            return false;
        }

        public class ParsedFilename
        {
            public OffsetDateTime? OffsetDateTime { get; set; }

            public LocalDateTime? LocalDateTime { get; set; }

            public Location Location { get; set; }

            public string Prefix { get; set; }

            public string Suffix { get; set; }

            public string Extension { get; set; }

            public string SensorType { get; set; }

            public double SensorTypeEstimate { get; set; }
        }

        public class DateVariant<T>
        {
            public DateVariant(string regex, IPattern<T> parseFormat, string[] helpHints = null)
            {
                this.Regex = new Regex(regex, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

                this.ParseFormat = parseFormat;
                this.HelpHints = helpHints;
            }

            public Regex Regex { get; }

            public IPattern<T> ParseFormat { get; }

            public string[] HelpHints { get; }
        }
    }
}
