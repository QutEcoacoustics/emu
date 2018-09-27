// <copyright file="Serializer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataExtractor.Serialization
{
    using System.Collections.Generic;
    using System.IO;
    using CsvHelper;
    using CsvHelper.Configuration;
    using MetadataExtractor.Models;

    /// <summary>
    /// Controls outputting of <see cref="Recording"/> data to other formats.
    /// </summary>
    public static class Serializer
    {
        static Serializer()
        {
        }

        /// <summary>
        /// Convert the given <see cref="Recording"/> to a string.
        /// </summary>
        /// <param name="recording">The recording to convert.</param>
        /// <returns>A string representation of the recording.</returns>
        public static string Serialize(IEnumerable<Recording> recording)
        {
            using (var stringWriter = new StringWriter())
            {
                var serializer = GetCsvWriter(stringWriter);

                serializer.WriteRecords(recording);

                return stringWriter.ToString();
            }
        }

        private static CsvWriter GetCsvWriter(TextWriter writer)
        {
            Configuration configuration = new Configuration()
            {
               ReferenceHeaderPrefix = (type, name) => $"{name}.",
            };
            configuration.RegisterClassMap<RecordingClassMap>();
            return new CsvWriter(writer, configuration)
            {
            };
        }
    }
}
