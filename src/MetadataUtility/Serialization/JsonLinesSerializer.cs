// <copyright file="JsonLinesSerializer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using MetadataUtility.Serialization.Converters;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using NodaTime;
    using NodaTime.Serialization.JsonNet;

    /// <inheritdoc cref="ISerializer"/>
    public class JsonLinesSerializer : ISerializer, IRecordFormatter
    {
        private readonly JsonSerializerSettings settings;
        private readonly Newtonsoft.Json.JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonLinesSerializer"/> class.
        /// </summary>
        public JsonLinesSerializer()
        {
            this.settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                    new WellKnownProblemJsonConverter(),
                    new JsonRangeConverter(),
                },
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            this.serializer = Newtonsoft.Json.JsonSerializer.Create(this.settings);
        }

        /// <inheritdoc />
        public string Serialize<T>(IEnumerable<T> objects)
        {
            using var stringWriter = new StringWriter();
            this.Serialize(stringWriter, objects);

            return stringWriter.ToString();
        }

        /// <inheritdoc />
        public void Serialize<T>(TextWriter writer, IEnumerable<T> objects)
        {
            foreach (var @object in objects)
            {
                this.serializer.Serialize(writer, @object);
                writer.WriteLine();
            }
        }

        /// <inheritdoc />
        public IDisposable WriteHeader<T>(IDisposable context, TextWriter writer, T record)
        {
            var json = new JsonTextWriter(writer);

            // noop

            return json;
        }

        /// <inheritdoc />
        public IDisposable WriteRecord<T>(IDisposable context, TextWriter writer, T record)
        {
            var json = (JsonTextWriter)context;
            this.serializer.Serialize(json, record);
            writer.WriteLine();

            return json;
        }

        /// <inheritdoc/>
        public IDisposable WriteFooter<T>(IDisposable context, TextWriter writer, T record)
        {
            // json lines does not have a footer
            return context;
        }

        /// <inheritdoc/>
        public void Dispose(IDisposable context, TextWriter writer)
        {
            // json lines does not have a footer
            return;
        }

        /// <inheritdoc />
        public IEnumerable<T> Deserialize<T>(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) is not null)
            {
                using var jsonReader = new JsonTextReader(new StringReader(line));
                yield return this.serializer.Deserialize<T>(jsonReader);
            }
        }
    }
}
