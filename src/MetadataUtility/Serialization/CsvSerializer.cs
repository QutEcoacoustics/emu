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

    /// <inheritdoc cref="CsvHelper.ISerializer"/>
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
            this.configuration.TypeConverterCache.AddConverter<LocalDateTime>(
                NodatimeConverters.LocalDateTimeConverter);
            this.configuration.TypeConverterCache.AddConverter<Offset>(
                NodatimeConverters.OffsetConverter);
            this.configuration.TypeConverterCache.AddConverter<Duration>(
                NodatimeConverters.DurationConverter);

            //this.configuration.RegisterClassMap<RecordingClassMap>();
        }

        /// <inheritdoc/>
        public string Serialize<T>(IEnumerable<T> objects)
        {
            using (var stringWriter = new StringWriter())
            {
                this.Serialize(stringWriter, objects);

                return stringWriter.ToString();
            }
        }

        /// <inheritdoc/>
        public void Serialize<T>(TextWriter writer, IEnumerable<T> objects)
        {
            var serializer = new CsvWriter(writer, this.configuration);

            serializer.WriteRecords(objects);
        }

        /// <inheritdoc />
        public IEnumerable<T> Deserialize<T>(TextReader reader)
        {
            var deserializer = new CsvReader(reader, this.configuration);

            // adds support for writing to immutable records
            deserializer.Configuration.IncludePrivateMembers = true;

            return deserializer.GetRecords<T>();
        }
    }
}
