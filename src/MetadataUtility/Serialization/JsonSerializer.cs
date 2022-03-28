// <copyright file="JsonSerializer.cs" company="QutEcoacoustics">
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
    public class JsonSerializer : ISerializer, IRecordFormatter
    {
        private readonly JsonSerializerSettings settings;
        private readonly Newtonsoft.Json.JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer"/> class.
        /// </summary>
        public JsonSerializer()
        {
            this.settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                    new WellKnownProblemJsonConverter(),
                    new JsonRangeConverter(),
                    new RationalNullJsonConverter(),
                    new RationalJsonConverter(),
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
            this.serializer.Serialize(writer, objects);
        }

        /// <inheritdoc />
        public IDisposable WriteHeader<T>(IDisposable context, TextWriter writer, T record)
        {
            var json = new JsonTextWriter(writer);

            json.WriteStartArray();

            //json.WriteWhitespace(Environment.NewLine);

            return json;
        }

        /// <inheritdoc />
        public IDisposable WriteRecord<T>(IDisposable context, TextWriter writer, T record)
        {
            var json = (JsonTextWriter)context;
            this.serializer.Serialize(json, record);

            return json;
        }

        /// <inheritdoc/>
        public IDisposable WriteFooter<T>(IDisposable context, TextWriter writer, T record)
        {
            // noop
            return context;
        }

        /// <inheritdoc/>
        public void Dispose(IDisposable context, TextWriter writer)
        {
            var json = context as JsonTextWriter;

            if (json == null)
            {
                throw new InvalidOperationException("JSON logger disposed before footer written");
            }

            json.WriteWhitespace(Environment.NewLine);
            json.WriteEndArray();

            return;
        }

        /// <inheritdoc />
        public IEnumerable<T> Deserialize<T>(TextReader reader)
        {
            using var jsonReader = new JsonTextReader(reader);
            return this.serializer.Deserialize<IEnumerable<T>>(jsonReader);
        }
    }
}
