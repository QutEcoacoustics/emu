// <copyright file="CsvSerializer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Serialization
{
    using System.Collections.Generic;
    using System.IO;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using MetadataUtility.Models;
    using NodaTime;

    /// <summary>
    /// Controls outputting of <see cref="Recording"/> data to other formats.
    /// </summary>
    public class CsvSerializer : ISerializer
    {
        private readonly Configuration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvSerializer"/> class.
        /// </summary>
        public CsvSerializer()
        {
            this.configuration = new Configuration()
            {
                ReferenceHeaderPrefix = (type, name) => $"{name}.",
            };

            this.configuration.TypeConverterCache.AddConverter<OffsetDateTime>(
                NodatimeConverters.OffsetDateTimeConverter);
            this.configuration.TypeConverterCache.AddConverter<Duration>(
                NodatimeConverters.DurationConverter);

            //this.configuration.RegisterClassMap<RecordingClassMap>();
        }

        /// <inheritdoc/>
        public string Serialize<T>(IEnumerable<T> recordings)
        {
            using (var stringWriter = new StringWriter())
            {
                this.Serialize(stringWriter, recordings);

                return stringWriter.ToString();
            }
        }

        /// <inheritdoc/>
        public void Serialize<T>(TextWriter writer, IEnumerable<T> recording)
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
