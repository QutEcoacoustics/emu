// <copyright file="CsvSerializer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using CsvHelper;
    using CsvHelper.Configuration;
    using MetadataUtility.Serialization.Converters;
    using NodaTime;
    using Rationals;

    /// <inheritdoc cref="ISerializer"/>
    public class CsvSerializer : ISerializer, IRecordFormatter
    {
        private readonly CsvConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvSerializer"/> class.
        /// </summary>
        public CsvSerializer()
        {
            this.configuration = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                ReferenceHeaderPrefix = (args) => $"{args.MemberName}.",
            };

            //this.configuration.RegisterClassMap<RecordingClassMap>();
        }

        /// <inheritdoc/>
        public string Serialize<T>(IEnumerable<T> objects)
        {
            using var stringWriter = new StringWriter();
            this.Serialize(stringWriter, objects);

            return stringWriter.ToString();
        }

        /// <inheritdoc/>
        public void Serialize<T>(TextWriter writer, IEnumerable<T> objects)
        {
            var serializer = new CsvWriter(writer, this.configuration);
            ApplyConverters(serializer.Context);

            serializer.WriteRecords(objects);
        }

        /// <inheritdoc/>
        public IDisposable WriteHeader<T>(IDisposable context, TextWriter writer, T record)
        {
            var csv = new CsvWriter(writer, this.configuration);
            ApplyConverters(csv.Context);

            csv.WriteHeader<T>();
            csv.NextRecord();

            return csv;
        }

        /// <inheritdoc/>
        public IDisposable WriteRecord<T>(IDisposable context, TextWriter writer, T record)
        {
            var csv = (CsvWriter)context;

            csv.WriteRecord(record);
            csv.NextRecord();
            csv.Flush();

            return csv;
        }

        /// <inheritdoc/>
        public IDisposable WriteFooter<T>(IDisposable context, TextWriter writer, T record)
        {
            // csv does not have a footer
            return context;
        }

        /// <inheritdoc/>
        public void Dispose(IDisposable context, TextWriter writer)
        {
            // csv does not have a footer
            return;
        }

        /// <inheritdoc />
        public IEnumerable<T> Deserialize<T>(TextReader reader)
        {
            var deserializer = new CsvReader(reader, this.configuration);
            ApplyConverters(deserializer.Context);

            // adds support for writing to immutable records
            deserializer.Context.Configuration.IncludePrivateMembers = true;

            return deserializer.GetRecords<T>();
        }

        private static void ApplyConverters(CsvContext context)
        {
            context.TypeConverterCache.AddConverter<OffsetDateTime>(
                NodatimeConverters.OffsetDateTimeConverter);
            context.TypeConverterCache.AddConverter<LocalDateTime>(
                NodatimeConverters.LocalDateTimeConverter);
            context.TypeConverterCache.AddConverter<Offset>(
                NodatimeConverters.OffsetConverter);
            context.TypeConverterCache.AddConverter<Duration>(
                NodatimeConverters.DurationConverter);
            context.TypeConverterCache.AddConverter<Range>(new CsvRangeConverter());
            context.TypeConverterCache.AddConverter<Rational>(new RationalsConverter());
            context.TypeConverterCache.AddConverter<string[]>(new StringListConverter());
        }
    }
}
