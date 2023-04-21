// <copyright file="Location.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using NodaTime;

    /// <summary>
    /// Defines a GPS location in the WGS84 Ellipsoid.
    /// </summary>
    public record Location : IFormattable
    {
        // this regex does not support the CRS when provided as a URI
        private static readonly Regex Iso6709ParseRegex = new(@"^(?<lat>[+-]\d\d(\.\d+)?)(?<lon>[+-]\d\d\d(\.\d+)?)(?<alt>[+-]\d+(\.\d+)?)?(CRS(?<crs>[^\/]+))?\/?$");

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        public Location()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class from string values found in an ISO6709:H string.
        /// </summary>
        /// <param name="latitude">The latitude string to parse.</param>
        /// <param name="longitude">The longitude string to parse.</param>
        /// <param name="altitude">The optional altitude to try and parse.</param>
        /// <param name="crs">The coordinate reference system this location used.</param>
        public Location(string latitude, string longitude, string altitude, string crs)
        {
            if (!TryParseLatitude(latitude, out var lat, out var precisionLat))
            {
                throw new FormatException("Cannot parse the given latitude");
            }

            if (!TryParseLongitude(longitude, out var lon, out var precisionLong))
            {
                throw new FormatException("Cannot parse the given longitude");
            }

            this.Latitude = lat;
            this.LatitudePrecision = precisionLat;
            this.Longitude = lon;
            this.LongitudePrecision = precisionLong;

            if (double.TryParse(
                altitude,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var alt))
            {
                this.Altitude = alt;
                this.AltitudePrecision = NumberOfDecimals(altitude, CultureInfo.InvariantCulture.NumberFormat);
            }

            this.CoordinateReferenceSystem = !string.IsNullOrEmpty(crs) ? crs : null;
        }

        /// <summary>
        /// Gets longitude in decimal degrees.
        /// </summary>
        public double? Latitude { get; init; }

        /// <summary>
        /// Gets the number of decimal places encountered during parsing.
        /// </summary>
        public int? LatitudePrecision { get; init; }

        /// <summary>
        /// Gets longitude in decimal degrees.
        /// </summary>
        public double? Longitude { get; init; }

        /// <summary>
        /// Gets the number of decimal places encountered during parsing.
        /// </summary>
        public int? LongitudePrecision { get; init; }

        /// <summary>
        /// Gets altitude in meters above the WGS84 reference ellipsoid in meters.
        /// </summary>
        public double? Altitude { get; init; }

        /// <summary>
        /// Gets the number of decimal places encountered during parsing.
        /// </summary>
        public int? AltitudePrecision { get; init; }

        /// <summary>
        /// Gets horizontal accuracy in meters above the WGS84 reference ellipsoid in meters.
        /// </summary>
        public double? HorizontalAccuracy { get; init; }

        /// <summary>
        /// Gets vertical accuracy in meters above the WGS84 reference ellipsoid in meters.
        /// </summary>
        public double? VerticalAccuracy { get; init; }

        /// <summary>
        /// Gets the speed in meters per second.
        /// </summary>
        public double? Speed { get; init; }

        /// <summary>
        /// Gets the heading in degrees, relative to true north.
        /// </summary>
        public double? Course { get; init; }

        /// <summary>
        /// Gets when this GPS sample was recorded.
        /// </summary>
        public Instant? SampleDateTime { get; init; }

        /// <summary>
        /// Gets the Coordinate Reference System (e.g. WGS84) that this location is using.
        /// </summary>
        public string CoordinateReferenceSystem { get; init; }

        /// <summary>
        /// Attempts to parse a ISO6709:H Latitude or a number with
        /// a cardinal direction from a string.
        /// </summary>
        /// <param name="latitudeText">The altitude to parse.</param>
        /// <param name="latitude">The longitude if parsing was successful.</param>
        /// <param name="precision">The precision of decimals encountered while parsing.</param>
        /// <returns><value>True</value> if parsing was successful.</returns>
        public static bool TryParseLatitude(string latitudeText, out double latitude, out int? precision)
        {
            return InternalParse(latitudeText, 'S', 'N', -90.0, 90.0, out latitude, out precision);
        }

        /// <summary>
        /// Attempts to parse a ISO6709:H Longitude or a number with
        /// a cardinal direction from a string.
        /// </summary>
        /// <param name="longitudeText">The altitude to parse.</param>
        /// <param name="longitude">The longitude if parsing was successful.</param>
        /// <param name="precision">The precision of decimals encountered while parsing.</param>
        /// <returns><value>True</value> if parsing was successful.</returns>
        public static bool TryParseLongitude(string longitudeText, out double longitude, out int? precision)
        {
            return InternalParse(longitudeText, 'W', 'E', -180.0, 180.0, out longitude, out precision);
        }

        public static bool TryParseAltitude(string altitudeText, out double? altitude, out int? precision)
        {
            // this one is a little different. altitude is often missing so that component
            // being null is not an error (unlike lat and long). So we only fail for
            // malformed numbers...

            altitude = null;
            precision = null;

            if (string.IsNullOrWhiteSpace(altitudeText))
            {
                return true;
            }

            if (!double.TryParse(altitudeText, out var alt))
            {
                return false;
            }

            altitude = alt;

            // count number of decimal places
            precision = NumberOfDecimals(altitudeText, CultureInfo.InvariantCulture.NumberFormat);

            return true;
        }

        public static bool TryParse(string input, out Location location)
        {
            location = default;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            // up to four parts: lat, long, altitude, CRS
            var match = Iso6709ParseRegex.Match(input);
            if (!match.Success)
            {
                return false;
            }

            if (!TryParseLatitude(match.Groups["lat"].Value, out var lat, out var precisionLat))
            {
                return false;
            }

            if (!TryParseLongitude(match.Groups["lon"].Value, out var lon, out var precisionLon))
            {
                return false;
            }

            if (!TryParseAltitude(match.Groups["alt"].Value, out var alt, out var precisionAlt))
            {
                return false;
            }

            var crs = match.Groups["crs"]?.Value;

            location = new Location()
            {
                Altitude = alt,
                AltitudePrecision = precisionAlt,
                CoordinateReferenceSystem = string.IsNullOrEmpty(crs) ? null : crs,
                Latitude = lat,
                LatitudePrecision = precisionLat,
                Longitude = lon,
                LongitudePrecision = precisionLon,
            };

            return true;
        }

        private static bool InternalParse(string text, char negative, char positive, double min, double max, out double value, out int? precision)
        {
            if (string.IsNullOrEmpty(text))
            {
                value = default;
                precision = null;
                return false;
            }

            // currently only supports parsing decimal degrees

            if (text[0] == positive)
            {
                // format: N123.45
                text = text[1..];
            }
            else if (text[0] == negative)
            {
                // format: S123.345
                text = '-' + text[1..];
            }
            else if (text[^2..] == " " + positive)
            {
                // format: 123.45 N
                text = text[0..^2];
            }
            else if (text[^2..] == " " + negative)
            {
                // format: 123.45 S
                text = '-' + text[0..^2];
            }

            var parsed = double.TryParse(
                text,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out value);

            if (value < min || value > max)
            {
                precision = null;
                return false;
            }

            // count number of decimal places
            precision = NumberOfDecimals(text, CultureInfo.InvariantCulture.NumberFormat);

            return parsed;
        }

        public string ToString(string format, IFormatProvider provider)
        {
            // https://en.wikipedia.org/wiki/ISO_6709
            var lat = FormatCoordinate(this.Latitude, CreateFormat("00", this.LatitudePrecision));
            var lon = FormatCoordinate(this.Longitude, CreateFormat("000", this.LongitudePrecision));
            var alt = FormatCoordinate(this.Altitude, CreateFormat("0", this.AltitudePrecision));
            return format switch
            {
                "D" => throw new NotImplementedException(),
                "F" => throw new NotImplementedException(),
                "H" => $"{lat}{lon}{alt}{FormatCrs()}/",

                // non-standard filename safe variant (omits solidus at end)
                "h" => $"{lat}{lon}{alt}{FormatCrs()}",

                // sames as "H"
                "" or null => $"{lat}{lon}{alt}{FormatCrs()}/",
                _ => throw new ArgumentException($"Location format string `{format}` unsupported"),
            };

            string FormatCoordinate(double? coordinate, string numberFormat)
            {
                return coordinate switch
                {
                    null => string.Empty,
                    >= 0 => $"+{coordinate.Value.ToString(numberFormat, provider)}",
                    < 0 => coordinate.Value.ToString(numberFormat, provider),
                    _ => throw new InvalidOperationException($"Don't know how to format `{coordinate}`"),
                };
            }

            string CreateFormat(string numerator, int? precision) =>
                numerator
                + CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator
                + (precision.HasValue ? new string('0', precision!.Value) : new string('#', 6));

            string FormatCrs() => this.CoordinateReferenceSystem switch
            {
                null when this.Altitude.HasValue => "CRSWGS_84",
                null => string.Empty,
                string s => "CRS" + s,
            };
        }

        private static int NumberOfDecimals(string text, NumberFormatInfo info)
        {
            var separatorIndex = text.IndexOf(info.NumberDecimalSeparator);

            if (separatorIndex < 0)
            {
                return 0;
            }

            return text.Length - separatorIndex - 1;
        }
    }
}
