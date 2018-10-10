// <copyright file="FilenameParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.FilenameParsing
{
    using System;
    using System.Collections.Generic;
    using System.Text;

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
        public object Parse(string filename)
        {
            return filename;
        }
    }
}
