// <copyright file="Location.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataExtractor.Models
{
    /// <summary>
    /// Defines a GPS location in the WGS84 Ellipsoid.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Gets or sets latitude in decimal degrees.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets longitude in decimal degrees.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets altitude in meters above the WGS84 reference ellipsoid in meters.
        /// </summary>
        public double? Altitude { get; set; }

        /// <summary>
        /// Gets or sets horizontal accuracy in meters above the WGS84 reference ellipsoid in meters.
        /// </summary>
        public double? HorizontalAccuracy { get; set; }

        /// <summary>
        /// Gets or sets vertical accuracy in meters above the WGS84 reference ellipsoid in meters.
        /// </summary>
        public double? VerticalAccuracy { get; set; }

        /// <summary>
        /// Gets or sets the speed in meters per second.
        /// </summary>
        public double? Speed { get; set; }

        /// <summary>
        /// Gets or sets the heading in degrees, relative to true north.
        /// </summary>
        public double? Course { get; set; }
    }
}
