// <copyright file="FilenameParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.FilenameParsing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MetadataUtility.Models;
    using NodaTime;

    /// <summary>
    /// Parses information from filenames.
    /// </summary>
    public class FilenameParser
    {
        /// <summary>
        /// Attempts to parse information from a filename.
        /// </summary>
        /// <param name="filename">The name of the file to process.</param>
        /// <returns>The parsed information.</returns>
        public ParsedFilename Parse(string filename)
        {
            return new ParsedFilename();
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
    }
}
