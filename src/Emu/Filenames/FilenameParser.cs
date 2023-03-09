// <copyright file="FilenameParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Filenames
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Emu.Dates;
    using Emu.Fixes.FrontierLabs;
    using Emu.Models;
    using LanguageExt;
    using MoreLinq;
    using NodaTime;
    using NodaTime.Text;
    using static LanguageExt.Prelude;

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
                Prefix + Date + IsoSeparator + TimeFractional + End,
                LocalDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss.FFFFFF")),

            // high precision variant WA SM4 - they use an underscore to separate the time
            // and fractional second components
            // valid: FNQ-RBS_20190102_044802_010.wav
            new DateVariant<LocalDateTime>(
                Prefix + Date + WildlifeAcousticsSeparator + @"(?<Time>\d{6}_\d{1,3})" + End,
                LocalDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss_FFF")),

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
                Prefix + @"(?<Date>\d{6})" + "(?<Separator>_)" + @"(?<Time>\d{4})" + End,
                LocalDateTimePattern.CreateWithInvariantCulture("yyMMddTHHmm")),

            // valid: 671629352.181204100002.wav
            new DateVariant<LocalDateTime>(
                Prefix + @"(?<Date>\d{6})" + Time + End,
                LocalDateTimePattern.CreateWithInvariantCulture("yyMMddTHHmmss")),

            // valid: prefix_2359-01012015.mp3, a_2359-01012015.a, a_2359-01012015.dnsb48364JSFDSD
            new DateVariant<LocalDateTime>(
                Prefix + @"(?<Time>\d{4})" + "(?<Separator>-)" + @"(?<Date>\d{8})" + End,
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
                Prefix + Date + IsoSeparator + TimeFractional + Offset + End,
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

        /// <summary>
        /// A collection of well known location variants.
        /// </summary>
        public static readonly Regex[] LocationVariants = new[]
        {
            // The FL format
            // valid: ...[27.2819 90.1361]..., ...[-27.2819 -90.1361]...
            @".*(?<Location>\[(?<Latitude>-?\d{1,2}(?:\.\d+)) (?<Longitude>-?\d{1,3}(?:\.\d+))\]).*",

            // A char-safe variant of the FL labs format
            // valid: ..._27.2819 90.1361_..., ..._-27.2819 -90.1361_...
            @".*(?<Location>_(?<Latitude>-?\d{1,2}(?:\.\d+)) (?<Longitude>-?\d{1,3}(?:\.\d+))_).*",

            // A ISO6709:H format (decimal degrees only)
            // We indicate the trailing slash (the solidus in the spec) is optional because it
            // cannot legally exist in windows filenames
            // valid: +40.20361-075.00417CRSWGS_84, -40.20361-075.00417, N40.20361E075.00417
            // valid: S40.20361W075.00417, +40.1213-075.0015+2.79CRSWGS_84, +40.20361-075.00417CRSWGS_84
            $@".*(?<Location>{Latitude}{Longitude}(?<Altitude>[-+][\.\d]+)?(?:CRS(?<Crs>[\w_]+))?\/?).*",
        }.Select(x => new Regex(x, RegexOptions.Compiled)).ToArray();

        private const string Prefix = @"^(?<Prefix>.*)";
        private const string Suffix = @"(?<Suffix>.*)";
        private const string Extension = @"(?<Extension>\.([a-zA-Z0-9]+))$";
        private const string Separator = @"(?<Separator>T|-|_|\$)";
        private const string WildlifeAcousticsSeparator = @"(?<Separator>_|\$)";
        private const string IsoSeparator = "(?<Separator>T)";
        private const string InvariantDateTimeSeparator = "T";
        private const string End = NoOffset + Suffix + Extension;
        private const string Date = @"(?<Date>\d{8})";
        private const string Time = @"(?<Time>\d{6})";
        private const string TimeFractional = @"(?<Time>\d{6}\.\d{1,6})";
        private const string Offset = @"(?<Offset>[-+][\d:]{2,5}|Z)";
        private const string NoOffset = @"(?![-+\d:]{1,6}|Z)";
        private const string Latitude = @"(?<Latitude>[-+NS]\d{2}(?:\.\d+)?)";
        private const string Longitude = @"(?<Longitude>[-+NS]\d{3}(?:\.\d+)?)";
        private readonly IFileSystem fileSystem;
        private readonly FilenameGenerator filenameGenerator;
        private readonly IEnumerable<DateVariant<LocalDateTime>> localDateVariants;
        private readonly IEnumerable<DateVariant<OffsetDateTime>> offsetDateVariant;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilenameParser"/> class.
        /// </summary>
        public FilenameParser(IFileSystem fileSystem, FilenameGenerator filenameGenerator)
            : this(fileSystem, filenameGenerator, PossibleLocalVariants, PossibleOffsetVariants)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilenameParser"/> class.
        /// </summary>
        /// <param name="fileSystem">The filesystem to use.</param>
        /// <param name="filenameGenerator">The filename generator to use.</param>
        /// <param name="localDateVariants">The possible local date strings to parse.</param>
        /// <param name="offsetDateVariant">The possible date with timezone strings to parse.</param>
        public FilenameParser(IFileSystem fileSystem, FilenameGenerator filenameGenerator, IEnumerable<DateVariant<LocalDateTime>> localDateVariants, IEnumerable<DateVariant<OffsetDateTime>> offsetDateVariant)
        {
            this.fileSystem = fileSystem;
            this.filenameGenerator = filenameGenerator;
            this.localDateVariants = localDateVariants;
            this.offsetDateVariant = offsetDateVariant;
            if (((localDateVariants?.Count() ?? 0) + (offsetDateVariant?.Count() ?? 0)) == 0)
            {
                throw new ArgumentException("No date variants were given to filename parser");
            }
        }

        /// <summary>
        /// Attempts to parse information from a filename.
        /// </summary>
        /// <param name="path">The path of the file to process.</param>
        /// <returns>The parsed information.</returns>
        public ParsedFilename Parse(string path)
        {
            var directory = this.fileSystem.Path.GetDirectoryName(path);
            var filename = this.fileSystem.Path.GetFileName(path);

            // FL008 sees a space where a leading 0 should be for the day segment of a datestamp.
            // Detect this and match it!
            if (SpaceInDatestamp.Matcher.Match(filename) is { Success: true } m)
            {
                filename = m.Result(SpaceInDatestamp.ReplaceString);
            }

            var extension = this.ParseExtension(filename, out var extensionRange);
            var location = ParseLocation(filename, out var locationRange);

            foreach (var dateVariant in this.offsetDateVariant)
            {
                if (TryParse(filename, dateVariant, out var value, out var valueLocation))
                {
                    return new ParsedFilename()
                    {
                        Extension = extension,
                        LocalStartDate = value.LocalDateTime,
                        StartDate = value,
                        Location = location is null ? null : location with
                        {
                            SampleDateTime = value.ToInstant(),
                        },
                        Directory = directory,
                        TokenizedName = this.ContructTokenizedName(filename, valueLocation, None, locationRange, extensionRange),
                    };
                }
            }

            foreach (var dateVariant in this.localDateVariants)
            {
                if (TryParse(filename, dateVariant, out var value, out var valueLocation))
                {
                    return new ParsedFilename()
                    {
                        Extension = extension,
                        LocalStartDate = value,
                        Location = location,
                        Directory = directory,
                        TokenizedName = this.ContructTokenizedName(filename, None, valueLocation, locationRange, extensionRange),
                    };
                }
            }

            // finally, if no date can be found return minimal information
            return new ParsedFilename()
            {
                Extension = extension,
                LocalStartDate = null,
                Location = location,
                StartDate = null,
                TokenizedName = this.ContructTokenizedName(filename, None, None, locationRange, extensionRange),
                Directory = directory,
            };
        }

        private static bool TryParse<T>(string filename, DateVariant<T> dateVariant, out T value, out Range location)
        {
            var match = dateVariant.Regex.Match(filename);
            if (match.Success)
            {
                var time = match.Groups[nameof(Time)];
                var offset = match.Groups[nameof(Offset)];
                var date = match.Groups[nameof(Date)];

                var timePart = time.Value + offset.Value;
                var parseString = date.Value + (timePart == string.Empty ? timePart : InvariantDateTimeSeparator + timePart);

                var parseResult = dateVariant.ParseFormat.Parse(parseString);

                if (parseResult.Success)
                {
                    value = parseResult.Value;
                    location = Seq(time, offset, date).Select(g => g.AsRange()).Somes().MinMax();
                    return true;
                }
            }

            value = default;
            location = default;
            return false;
        }

        private static Location ParseLocation(string target, out Option<Range> range)
        {
            foreach (var locationVariant in LocationVariants)
            {
                var match = locationVariant.Match(target);
                if (match.Success)
                {
                    var location = match.Groups["Location"];
                    var latitude = match.Groups[nameof(Latitude)];
                    var longitude = match.Groups[nameof(Longitude)];
                    var altitude = match.Groups["Altitude"];
                    var crs = match.Groups["CRS"];

                    range = location.AsRange();
                    return new Location(latitude.Value, longitude.Value, altitude.Value, crs.Value);
                }
            }

            range = None;
            return null;
        }

        private string ParseExtension(string filename, out Option<Range> range)
        {
            var extension = this.fileSystem.Path.GetExtension(filename);
            range = new Range(filename.Length - extension.Length, filename.Length);

            return extension;
        }

        private string ContructTokenizedName(
            string filename,
            Option<Range> datestamp,
            Option<Range> localDatestamp,
            Option<Range> location,
            Option<Range> extension)
        {
            Option<(Range Range, string Token)> Bind(Option<Range> range, string name) => range.Case switch
            {
                Range r => (r, name.AsToken()),
                _ => None,
            };

            var all = Seq(
                Bind(datestamp, nameof(Recording.StartDate)),
                Bind(localDatestamp, nameof(Recording.LocalStartDate)),
                Bind(location, nameof(Recording.Location)));

            var result = Lst<string>.Empty;
            Index previousIndex = 0;

            // filter out nones
            // sort by index
            // collect fragements of the strings and tokens
            foreach (var (range, token) in all.Somes().OrderBy(x => x.Range.Start.Value))
            {
                var slice = previousIndex..range.Start;
                AppendLiteral(slice);

                result = result.Add(token);

                previousIndex = range.End;
            }

            // add on any trailing segment
            string extensionToken = null;
            if (Bind(extension, nameof(Recording.Extension)).Case is (Range extRange, string extToken))
            {
                var slice = previousIndex..extRange.Start;
                AppendLiteral(slice);
                extensionToken = extToken;
                result += extToken;
            }
            else
            {
                AppendLiteral(previousIndex..);
            }

            return result.Aggregate(new StringBuilder(filename.Length * 2), Join).ToString();

            StringBuilder Join(StringBuilder builder, string current)
            {
                if (builder.Length > 0 && !current.StartsWith(".") && current != extensionToken)
                {
                    builder.Append(FilenameGenerator.Delimitter);
                }

                builder.Append(current);
                return builder;
            }

            void AppendLiteral(Range slice)
            {
                if (slice.Length(filename.Length) > 0)
                {
                    var cleaned = this.filenameGenerator.CleanSegment(filename[slice]);
                    if (cleaned.Length > 0)
                    {
                        result += cleaned;
                    }
                }
            }
        }

        /// <summary>
        /// Represents a regex match and nodatime parsing combination for date stamps
        /// embedded in other strings.
        /// </summary>
        /// <typeparam name="T">The date type to deserialize.</typeparam>
        public class DateVariant<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DateVariant{T}"/> class.
            /// </summary>
            /// <param name="regex">The regex to use.</param>
            /// <param name="parseFormat">The date parser to use.</param>
            public DateVariant(string regex, IPattern<T> parseFormat /*,string[] helpHints = null*/)
            {
                this.Regex = new Regex(regex, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

                this.ParseFormat = parseFormat;
            }

            /// <summary>
            /// Gets the regex that will be used to match a string pattern.
            /// Must have named groups "Date", "Time", and optionally "Offset".
            /// </summary>
            public Regex Regex { get; }

            /// <summary>
            /// Gets the NodaTime <see cref="IPattern{T}"/> used to parse a date from the
            /// string extracted by the <see cref="Regex"/>.
            /// </summary>
            public IPattern<T> ParseFormat { get; }
        }
    }
}
