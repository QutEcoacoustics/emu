// <copyright file="Location.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using NodaTime;

    /// <summary>
    /// Defines a GPS location in the WGS84 Ellipsoid.
    /// </summary>
    public record Location
    {
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
            if (!TryParseLatitude(latitude, out var lat))
            {
                throw new FormatException("Cannot parse the given latitude");
            }

            if (!TryParseLongitude(longitude, out var lon))
            {
                throw new FormatException("Cannot parse the given longitude");
            }

            this.Latitude = lat;
            this.Longitude = lon;

            if (double.TryParse(
                altitude,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var alt))
            {
                this.Altitude = alt;
            }

            this.CoordinateReferenceSystem = crs;
        }

        /// <summary>
        /// Gets longitude in decimal degrees.
        /// </summary>
        public double? Latitude { get; init; }

        /// <summary>
        /// Gets longitude in decimal degrees.
        /// </summary>
        public double? Longitude { get; init; }

        /// <summary>
        /// Gets altitude in meters above the WGS84 reference ellipsoid in meters.
        /// </summary>
        public double? Altitude { get; init; }

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
        /// Attempts to parse a ISO6709:H Latitude value from a string.
        /// </summary>
        /// <param name="latitudeText">The altitude to parse.</param>
        /// <param name="latitude">The longitude if parsing was successful.</param>
        /// <returns><value>True</value> if parsing was successful.</returns>
        public static bool TryParseLatitude(string latitudeText, out double latitude)
        {
            return InternalParse(latitudeText, 'S', 'N', -90.0, 90.0, out latitude);
        }

        /// <summary>
        /// Attempts to parse a ISO6709:H Latitude value from a string.
        /// </summary>
        /// <param name="longitudeText">The altitude to parse.</param>
        /// <param name="longitude">The longitude if parsing was successful.</param>
        /// <returns><value>True</value> if parsing was successful.</returns>
        public static bool TryParseLongitude(string longitudeText, out double longitude)
        {
            return InternalParse(longitudeText, 'E', 'W', -180.0, 180.0, out longitude);
        }

        private static bool InternalParse(string latitudeText, char negative, char positive, double min, double max, out double value)
        {
            if (string.IsNullOrEmpty(latitudeText))
            {
                value = default;
                return false;
            }

            // currently only supports parsing decimal degrees

            if (latitudeText[0] == positive)
            {
                latitudeText = latitudeText.Substring(1);
            }
            else if (latitudeText[0] == negative)
            {
                latitudeText = '-' + latitudeText.Substring(1);
            }

            var parsed = double.TryParse(
                latitudeText,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out value);

            if (value < min || value > max)
            {
                return false;
            }

            return parsed;
        }
    }
}
