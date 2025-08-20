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
            new(
                Prefix + Date + IsoSeparator + TimeFractional + End,
                LocalDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss.FFFFFF")),

            // high precision variant WA SM4 - they use an underscore to separate the time
            // and fractional second components
            // valid: FNQ-RBS_20190102_044802_010.wav
            new(
                Prefix + Date + WildlifeAcousticsSeparator + @"(?<Time>\d{6}_\d{1,3})" + End,
                LocalDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss_FFF")),

            // valid: Prefix_YYYYMMDD_hhmmss.wav,
            // valid: prefix_20140101_235959.mp3, a_00000000_000000.a, a_99999999_999999.dnsb48364JSFDSD
            // valid: SERF_20130314_000021_000.wav, a_20130314_000021_a.a, a_99999999_999999_a.dnsb48364JSFDSD
            new(
                Prefix + Date + Separator + Time + End,
                LocalDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss")),

            // valid: 20070415051314.wav.trimmed.wav
            new(
                Prefix + Date + Time + End,
                LocalDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss")),

            // valid: short_time_180801_1630_test.wav
            new(
                Prefix + @"(?<Date>\d{6})" + "(?<Separator>_)" + @"(?<Time>\d{4})" + End,
                LocalDateTimePattern.CreateWithInvariantCulture("yyMMddTHHmm")),

            // valid: 671629352.181204100002.wav
            new(
                Prefix + @"(?<Date>\d{6})" + Time + End,
                LocalDateTimePattern.CreateWithInvariantCulture("yyMMddTHHmmss")),

            // valid: prefix_2359-01012015.mp3, a_2359-01012015.a, a_2359-01012015.dnsb48364JSFDSD
            new(
                Prefix + @"(?<Time>\d{4})" + "(?<Separator>-)" + @"(?<Date>\d{8})" + End,
                LocalDateTimePattern.CreateWithInvariantCulture("ddMMuuuuTHHmm")),
        };

        /// <summary>
        /// A collection of well known date with offset variants.
        /// </summary>
        public static readonly DateVariant<OffsetDateTime>[] PossibleOffsetVariants =
        {
            // high precision start and end variant used for localization
            // valid: S20240815T091156.982648+1000_E20240815T091251.967555+1000_-12.34567+78.98102.wav
            new(
                "^(?<DatePrefix>S)" + Date + IsoSeparator + TimeFractional + Offset + "_(?<DateEndPrefix>E)" + DateEnd + IsoSeparator + TimeFractionalEnd + OffsetEnd + End,
                OffsetDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss.FFFFFFo<I>")),

            // high precision variant
            // valid: 20091219T070006.789123+1130_00600.wav
            new(
                Prefix + Date + IsoSeparator + TimeFractional + Offset + End,
                OffsetDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmss.FFFFFFo<I>")),

            // valid:Prefix_YYYYMMDD_hhmmssZ.wav
            // valid:prefix_20140101_235959Z.mp3
            // valid: prefix_20140101-235959+10.mp3, a_00000000-000000+00.a, a_99999999-999999+9999.dnsb48364JSFDSD
            // valid: prefix_20140101_235959+10.mp3, a_00000000_000000+00.a, a_99999999_999999+9999.dnsb48364JSFDSD
            // ISO8601-ish (supports a file compatible variant of ISO8601)
            // valid: prefix_20140101T235959+10.mp3, a_00000000T000000+00.a, a_99999999T999999+9999.dnsb48364JSFDSD
            new(
                Prefix + Date + Separator + Time + Offset + End,
                OffsetDateTimePattern.CreateWithInvariantCulture("uuuuMMddTHHmmsso<I>")),

            // an audio moth style date 5AFCD4F4.WAV
            new("^(?<Date>[0-9A-F]{8})" + Extension, new AudioMothDateParser()),
        };

        /// <summary>
        /// A collection of well known location variants.
        /// </summary>
        public static readonly Regex[] LocationVariants = new[]
        {
            // The FL format
            // valid: ...[27.2819 90.1361]..., ...[-27.2819 -90.1361]...
            @".*(?<Location>\[(?<Latitude>-?\d{1,2}(?:\.\d+)) (?<Longitude>-?\d{1,3}(?:\.\d+))\]).*",

            // another variant of the FL format without the space in between. It also requires a sign.
            // ...[27.2819+90.1361]...
            @".*(?<Location>\[(?<Latitude>[-+]\d{1,2}(?:\.\d+))(?<Longitude>[-+]\d{1,3}(?:\.\d+))\]).*",

            // A char-safe variant of the FL labs format
            // valid: ..._27.2819 90.1361_..., ..._-27.2819 -90.1361_...
            @".*(?<Location>_(?<Latitude>-?\d{1,2}(?:\.\d+)) (?<Longitude>-?\d{1,3}(?:\.\d+))_).*",

            // A ISO6709:H format (decimal degrees only)
            // We indicate the trailing slash (the solidus in the spec) is optional because it
            // cannot legally exist in windows filenames
            // valid: +40.20361-075.00417CRSWGS_84, -40.20361-075.00417, N40.20361E075.00417
            // valid: S40.20361W075.00417, +40.1213-075.0015+2.79CRSWGS_84, +40.20361-075.00417CRSWGS_84
            // valid: 20190626T160000+1000_REC_-27.3888+152.8808.flac
            // valid: S20240815T091156.982648+1000_E20240815T091251.967555+1000_-12.34567+78.98102.wav
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
        private const string DateEnd = @"(?<DateEnd>\d{8})";
        private const string TimeEnd = @"(?<TimeEnd>\d{6})";
        private const string TimeFractionalEnd = @"(?<TimeEnd>\d{6}\.\d{1,6})";
        private const string OffsetEnd = @"(?<OffsetEnd>[-+][\d:]{2,5}|Z)";
        private const string NoOffset = @"(?![-+\d:]{1,6}|Z)";
        private const string Latitude = @"(?<Latitude>[-+NS]\d{1,2}(?:\.\d+)?)";
        private const string Longitude = @"(?<Longitude>[-+NS]\d{1,3}(?:\.\d+)?)";
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
                if (TryParse(filename, dateVariant, out var startDate, out var endDate))
                {
                    return new ParsedFilename()
                    {
                        Extension = extension,
                        LocalStartDate = startDate.Date.Value.LocalDateTime,
                        StartDate = startDate.Date.Value,
                        EndDate = endDate.Map(e => e.Date.Value).ToNullable(),
                        Location = location is null ? null : location with
                        {
                            SampleDateTime = startDate.Date.Value.ToInstant(),
                        },
                        Directory = directory,
                        NameTokens = ConstructTokenizedName(
                            filename,
                            startDate.Location,
                            None,
                            endDate.Map(e => e.Location),
                            locationRange,
                            extensionRange,
                            startDate.Prefix,
                            endDate.Bind(e => e.Prefix)),
                    };
                }
            }

            foreach (var dateVariant in this.localDateVariants)
            {
                if (TryParse(filename, dateVariant, out var value, out var endDate))
                {
                    if (endDate.IsSome)
                    {
                        throw new NotSupportedException("End date is not supported for local date variants");
                    }

                    return new ParsedFilename()
                    {
                        Extension = extension,
                        LocalStartDate = value.Date.Value,
                        Location = location,
                        Directory = directory,
                        NameTokens = ConstructTokenizedName(filename, None, value.Location, None, locationRange, extensionRange, value.Prefix, None),
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
                NameTokens = ConstructTokenizedName(filename, None, None, None, locationRange, extensionRange, None, None),
                Directory = directory,
            };
        }

        private static bool TryParse<T>(string filename, DateVariant<T> dateVariant, out DateMatch<T> startDate, out Option<DateMatch<T>> endDate)
        {
            var match = dateVariant.Regex.Match(filename);
            if (match.Success)
            {
                var startDateMatch = ParseDate(match, nameof(Date), nameof(Time), nameof(Offset), "DatePrefix", dateVariant.ParseFormat);
                if (startDateMatch.Date.Success)
                {
                    startDate = startDateMatch;

                    // We only try and parse an end date if we already found a start date
                    var parseResultEnd = ParseDate<T>(match, nameof(DateEnd), nameof(TimeEnd), nameof(OffsetEnd), "DateEndPrefix", dateVariant.ParseFormat);
                    endDate = parseResultEnd.Date.Success ? parseResultEnd : None;

                    return true;
                }
            }

            startDate = default;
            endDate = None;
            return false;
        }

        private static DateMatch<T> ParseDate<T>(Match match, string dateGroupName, string timeGroupName, string offsetGroupName, string prefixGroupName, IPattern<T> pattern)
        {
            var date = match.Groups[dateGroupName];
            var time = match.Groups[timeGroupName];
            var offset = match.Groups[offsetGroupName];
            var prefix = match.Groups[prefixGroupName];

            // we need a t least a date value to parse the time
            if (!date.Success)
            {
                return new(ParseResult<T>.ForException(() => new FormatException("Missing date or time")), default, None);
            }

            // Build parsing string
            // We support date only for AudioMoth timestamps (this is just a group naming quirk)
            var parseString = date.Value;

            // no point adding a separator if we don't have a time
            // this step normalizes the separator
            // and offsets can only be appending to times
            if (time.Success)
            {
                parseString += InvariantDateTimeSeparator + time.Value + offset.Value;
            }

            var location = Seq(date, time, offset, prefix).Select(g => g.AsRange()).Somes().MinMax();
            return new(pattern.Parse(parseString), location, prefix.Success ? prefix.Value : None);
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

        private static Lst<FilenameToken> ConstructTokenizedName(
            string filename,
            Option<Range> datestamp,
            Option<Range> localDatestamp,
            Option<Range> endDatestamp,
            Option<Range> location,
            Option<Range> extension,
            Option<string> datestampPrefix,
            Option<string> endDatestampPrefix)
        {
            Option<(Range Range, string Token, string Prefix)> Bind(Option<Range> range, string name, Option<string> prefix) => range.Case switch
            {
                Range r => (r, name, prefix.IfNone(string.Empty)),
                _ => None,
            };

            var all = Seq(
                Bind(datestamp, nameof(Recording.StartDate), datestampPrefix),
                Bind(endDatestamp, nameof(Recording.EndDate), endDatestampPrefix),
                Bind(localDatestamp, nameof(Recording.LocalStartDate), None),
                Bind(location, nameof(Recording.Location), None));

            var result = Lst<FilenameToken>.Empty;
            Index previousIndex = 0;

            // filter out nones
            // sort by index
            // collect fragments of the strings and tokens
            foreach (var (range, token, prefix) in all.Somes().OrderBy(x => x.Range.Start.Value))
            {
                var slice = previousIndex..range.Start;
                AppendLiteral(slice);

                result = result.Add(new FilenameToken.Value(token, prefix));

                previousIndex = range.End;
            }

            // add on any trailing segment

            if (Bind(extension, nameof(Recording.Extension), None).Case is (Range extRange, string extToken, string extPrefix))
            {
                var slice = previousIndex..extRange.Start;
                AppendLiteral(slice);
                result = result.Add(new FilenameToken.Value(extToken, extPrefix, Compact: true));
            }
            else
            {
                AppendLiteral(previousIndex..);
            }

            return result;

            void AppendLiteral(Range slice)
            {
                if (slice.Length(filename.Length) > 0)
                {
                    result += new FilenameToken.Literal(filename[slice]);
                }
            }
        }

        private string ParseExtension(string filename, out Option<Range> range)
        {
            var extension = this.fileSystem.Path.GetExtension(filename);
            range = new Range(filename.Length - extension.Length, filename.Length);

            return extension;
        }

        private record struct DateMatch<T>(ParseResult<T> Date, Range Location, Option<string> Prefix);

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
            /// <param name="prefix">A prefix to add to the date string if it is reconstructed.</param>
            public DateVariant(string regex, IPattern<T> parseFormat)
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
