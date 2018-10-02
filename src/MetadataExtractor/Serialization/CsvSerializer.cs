// <copyright file="CsvSerializer.cs" company="QutEcoacoustics">
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
    public class CsvSerializer : ISerializer
    {
        private readonly Configuration configuration;

        /// <inheritdoc />
        public CsvSerializer()
        {
            this.configuration = new Configuration()
            {
                ReferenceHeaderPrefix = (type, name) => $"{name}.",
            };
            this.configuration.RegisterClassMap<RecordingClassMap>();
            
            this.configuration.TypeConverterCache.AddConverter<>();
        }

        /// <inheritdoc/>
        public string Serialize(IEnumerable<Recording> recordings)
        {
            using (var stringWriter = new StringWriter())
            {
                this.Serialize(stringWriter, recordings);

                return stringWriter.ToString();
            }
        }

        /// <inheritdoc/>
        public void Serialize(TextWriter writer, IEnumerable<Recording> recording)
        {
            var serializer = this.GetCsvWriter(writer);

            serializer.WriteRecords(recording);
        }

        private CsvWriter GetCsvWriter(TextWriter writer)
        {
            return new CsvWriter(writer, this.configuration);
        }
    }
}
