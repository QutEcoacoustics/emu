// <copyright file="Guano.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Standards.GUANO
{
    using System.Collections.Generic;
    using System.Globalization;
    using Emu.Models;
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// See https://github.com/riggsd/guano-spec/blob/master/guano_specification.md.
    /// https://www.wildlifeacoustics.com/SCHEMA/GUANO.html.
    /// </summary>
    public record GuanoBlock
    {
        private static readonly OffsetDateTimePattern OffsetDatePattern = OffsetDateTimePattern.Rfc3339;
        private static readonly LocalDateTimePattern LocalDatePattern = LocalDateTimePattern.ExtendedIso;

        /// <summary>
        /// Gets the parsed value of GUANO|Version.
        /// </summary>
        public string Version { get; init; }

        /// <summary>
        /// Gets all parsed GUANO metadata entries.
        /// </summary>
        public IReadOnlyList<GuanoEntry> Entries { get; init; } = new List<GuanoEntry>();

        public string Make => this.GetValue("Make");

        public string Model => this.GetValue("Model");

        public string FirmwareVersion => this.GetValue("Firmware Version");

        public string Serial => this.GetValue("Serial");

        public string Timestamp => this.GetValue("Timestamp");

        public OffsetDateTime? TimestampOffsetDateTime => ParseOffsetDateTime(this.Timestamp);

        public LocalDateTime? TimestampLocalDateTime => ParseLocalDateTime(this.Timestamp);

        public double? LengthSeconds => ParseDouble(this.GetValue("Length"));

        public uint? SampleRateHertz => ParseUInt(this.GetValue("Samplerate"));

        public double? TemperatureIntCelsius => ParseDouble(this.GetValue("Temperature Int"));

        public double? TemperatureExtCelsius => ParseDouble(this.GetValue("Temperature Ext"));

        public string LocPosition => this.GetValue("Loc Position");

        public string LocElevation => this.GetValue("Loc Elevation");

        public Location Location
        {
            get
            {
                if (TryParseLocation(this.LocPosition, this.LocElevation, out var location))
                {
                    return location;
                }

                return null;
            }
        }

        public IEnumerable<GuanoEntry> VendorEntries => this.Entries.Where(x => x.Namespaces.Count > 0 && x.Namespaces[0] != "GUANO");

        public string PrimaryVendorNamespace => this.VendorEntries.FirstOrDefault()?.Namespaces[0];

        public string GetValue(string field)
        {
            return this.GetValue(Array.Empty<string>(), field);
        }

        public string GetValue(IReadOnlyList<string> namespaces, string field)
        {
            foreach (var entry in this.Entries)
            {
                if (!string.Equals(entry.Field, field, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!SequenceEqual(entry.Namespaces, namespaces))
                {
                    continue;
                }

                return entry.Value;
            }

            return null;
        }

        public static string ToKey(IReadOnlyList<string> namespaces, string field)
        {
            if (namespaces is null || namespaces.Count == 0)
            {
                return field;
            }

            return string.Join("|", namespaces) + "|" + field;
        }

        private static bool SequenceEqual(IReadOnlyList<string> left, IReadOnlyList<string> right)
        {
            left ??= Array.Empty<string>();
            right ??= Array.Empty<string>();

            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                if (!string.Equals(left[i], right[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static OffsetDateTime? ParseOffsetDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var parsed = OffsetDatePattern.Parse(value);
            return parsed.Success ? parsed.Value : null;
        }

        private static LocalDateTime? ParseLocalDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var offset = ParseOffsetDateTime(value);
            if (offset is not null)
            {
                return offset.Value.LocalDateTime;
            }

            var parsed = LocalDatePattern.Parse(value);
            return parsed.Success ? parsed.Value : null;
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

        private static bool TryParseLocation(string locationText, string elevationText, out Location location)
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
                CoordinateReferenceSystem = "WGS84",
            };

            return true;
        }
    }
}
