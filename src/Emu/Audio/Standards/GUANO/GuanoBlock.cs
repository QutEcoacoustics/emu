// <copyright file="GuanoBlock.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Standards.GUANO
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using Emu.Models;
    using LanguageExt;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Text;
    using Error = LanguageExt.Common.Error;

    /// <summary>
    /// See https://github.com/riggsd/guano-spec/blob/master/guano_specification.md.
    /// https://www.wildlifeacoustics.com/SCHEMA/GUANO.html.
    /// </summary>
    public record GuanoBlock
    {
        private static readonly OffsetDateTimePattern OffsetDatePattern = OffsetDateTimePattern.Rfc3339;
        private static readonly LocalDateTimePattern LocalDatePattern = LocalDateTimePattern.ExtendedIso;

        /// <summary>
        /// Gets all parsed GUANO metadata entries.
        /// </summary>
        [Browsable(false)]
        public IReadOnlyDictionary<GuanoKey, string> Entries { get; init; } = new Dictionary<GuanoKey, string>();

        /// <summary>
        /// Gets the value of <c>GUANO|Version</c>.
        /// This is required and should be the first key in a GUANO block.
        /// </summary>
        public string GuanoVersion { get; init; }

        /// <summary>
        /// Gets the value of <c>Make</c>.
        /// Manufacturer of the recording hardware.
        /// </summary>
        public string Make => this.GetValue("Make");

        /// <summary>
        /// Gets the value of <c>Model</c>.
        /// Model name or number of the recording hardware.
        /// </summary>
        public string Model => this.GetValue("Model");

        /// <summary>
        /// Gets the value of <c>Firmware Version</c>.
        /// Device firmware version in manufacturer-specific format.
        /// </summary>
        public string FirmwareVersion => this.GetValue("Firmware Version");

        /// <summary>
        /// Gets the value of <c>Hardware Version</c>.
        /// Device hardware revision or options in manufacturer-specific format.
        /// </summary>
        public string HardwareVersion => this.GetValue("Hardware Version");

        /// <summary>
        /// Gets the value of <c>Serial</c>.
        /// Serial number or unique hardware identifier.
        /// </summary>
        public string Serial => this.GetValue("Serial");

        /// <summary>
        /// Gets the value of <c>Original Filename</c>.
        /// Original filename used by the recording hardware.
        /// </summary>
        public string OriginalFilename => this.GetValue("Original Filename");

        /// <summary>
        /// Gets the parsed value of <c>Timestamp</c>.
        /// Successful values are either local datetime or offset datetime.
        /// </summary>
        public Fin<Either<LocalDateTime, OffsetDateTime>> Timestamp => ParseTimestamp(this.GetValue("Timestamp"));

        /// <summary>
        /// Gets the value of <c>Length</c> in seconds.
        /// </summary>
        public double? Length => ParseDouble(this.GetValue("Length"));

        /// <summary>
        /// Gets the value of <c>Samplerate</c> in Hz.
        /// </summary>
        public uint? Samplerate => ParseUInt(this.GetValue("Samplerate"));

        /// <summary>
        /// Gets the value of <c>TE</c>.
        /// Time-expansion factor where 1 means direct recording.
        /// </summary>
        public uint? TE => ParseUInt(this.GetValue("TE"));

        /// <summary>
        /// Gets the value of <c>Filter HP</c> in kHz.
        /// </summary>
        public double? FilterHP => ParseDouble(this.GetValue("Filter HP"));

        /// <summary>
        /// Gets the value of <c>Filter LP</c> in kHz.
        /// </summary>
        public double? FilterLP => ParseDouble(this.GetValue("Filter LP"));

        /// <summary>
        /// Gets the value of <c>Temperature Int</c> in degrees Celsius.
        /// </summary>
        public double? TemperatureInt => ParseDouble(this.GetValue("Temperature Int"));

        /// <summary>
        /// Gets the value of <c>Temperature Ext</c> in degrees Celsius.
        /// </summary>
        public double? TemperatureExt => ParseDouble(this.GetValue("Temperature Ext"));

        /// <summary>
        /// Gets the value of <c>Humidity</c> as percentage in the range 0.0-100.0.
        /// </summary>
        public double? Humidity => ParseDouble(this.GetValue("Humidity"));

        /// <summary>
        /// Gets the value of <c>Note</c>.
        /// Freeform multiline textual note.
        /// </summary>
        public string Note => this.GetValue("Note");

        /// <summary>
        /// Gets the values of <c>Species Auto ID</c>.
        /// Species or guild classifications from automated classification.
        /// </summary>
        public IReadOnlyList<string> SpeciesAutoID => ParseList(this.GetValue("Species Auto ID"));

        /// <summary>
        /// Gets the values of <c>Species Manual ID</c>.
        /// Species or guild classifications from human classification.
        /// </summary>
        public IReadOnlyList<string> SpeciesManualID => ParseList(this.GetValue("Species Manual ID"));

        /// <summary>
        /// Gets the values of <c>Tags</c>.
        /// Comma-separated arbitrary labels.
        /// </summary>
        public IReadOnlyList<string> Tags => ParseList(this.GetValue("Tags"));

        /// <summary>
        /// Gets the value of <c>Loc Position</c> as a WGS84 latitude/longitude tuple string.
        /// </summary>
        public string LocPosition => this.GetValue("Loc Position");

        /// <summary>
        /// Gets the value of <c>Loc Elevation</c> in meters above mean sea level.
        /// </summary>
        public string LocElevation => this.GetValue("Loc Elevation");

        /// <summary>
        /// Gets the value of <c>Loc Accuracy</c> in meters.
        /// Estimated Position Error (EPE) style accuracy value.
        /// </summary>
        public double? LocAccuracy => ParseDouble(this.GetValue("Loc Accuracy"));

        public Location Location
        {
            get
            {
                if (TryParseLocation(this.LocPosition, this.LocElevation, this.LocAccuracy, out var location))
                {
                    return location;
                }

                return null;
            }
        }

        [JsonIgnore]
        public IEnumerable<KeyValuePair<GuanoKey, string>> VendorEntries =>
            this.Entries.Where(x => x.Key.IsVendorNamespace);

        public string PrimaryVendorNamespace => this.VendorEntries.FirstOrDefault().Key.Namespaces.FirstOrDefault();

        public string GetValue(string field)
        {
            return this.GetValue(GuanoKey.Parse(field));
        }

        public string GetValue(GuanoKey key)
        {
            return this.Entries.TryGetValue(key, out var value) ? value : null;
        }

        private static Fin<Either<LocalDateTime, OffsetDateTime>> ParseTimestamp(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Error.New("GUANO `Timestamp` was empty.");
            }

            var offsetParsed = OffsetDatePattern.Parse(value);
            if (offsetParsed.Success)
            {
                return Fin<Either<LocalDateTime, OffsetDateTime>>.Succ((Either<LocalDateTime, OffsetDateTime>)offsetParsed.Value);
            }

            var localParsed = LocalDatePattern.Parse(value);
            if (localParsed.Success)
            {
                return Fin<Either<LocalDateTime, OffsetDateTime>>.Succ((Either<LocalDateTime, OffsetDateTime>)localParsed.Value);
            }

            return Error.New($"Invalid GUANO `Timestamp`: {value}");
        }

        private static double? ParseDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private static uint? ParseUInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private static IReadOnlyList<string> ParseList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool TryParseLocation(string locationText, string elevationText, double? accuracy, out Location location)
        {
            location = null;
            if (string.IsNullOrWhiteSpace(locationText))
            {
                return false;
            }

            var split = locationText
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (split.Length < 2)
            {
                return false;
            }

            if (!Location.TryParseLatitude(split[0], out var lat, out var latPrecision))
            {
                return false;
            }

            if (!Location.TryParseLongitude(split[1], out var lon, out var lonPrecision))
            {
                return false;
            }

            _ = Location.TryParseAltitude(elevationText, out var alt, out var altPrecision);

            location = new Location
            {
                Latitude = lat,
                LatitudePrecision = latPrecision,
                Longitude = lon,
                LongitudePrecision = lonPrecision,
                Altitude = alt,
                AltitudePrecision = altPrecision,
                HorizontalAccuracy = accuracy,
                CoordinateReferenceSystem = "WGS84",
            };

            return true;
        }
    }
}
