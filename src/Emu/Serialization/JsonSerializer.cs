// <copyright file="JsonSerializer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Emu.Serialization.Converters;
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
            this.settings = Settings;

            this.serializer = Newtonsoft.Json.JsonSerializer.Create(this.settings);
        }

        public static JsonSerializerSettings Settings
        {
            get
            {
                return new JsonSerializerSettings()
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
            }
        }

        public TextWriter Writer { get; set; }

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
        public IDisposable WriteHeader<T>(IDisposable context, T record)
        {
            var json = new JsonTextWriter(this.Writer);

            json.WriteStartArray();

            //json.WriteWhitespace(Environment.NewLine);

            return json;
        }

        /// <inheritdoc />
        public IDisposable WriteRecord<T>(IDisposable context, T record)
        {
            var json = (JsonTextWriter)context;
            this.serializer.Serialize(json, record);

            return json;
        }

        /// <inheritdoc />
        public virtual IDisposable WriteMessage<T>(IDisposable context, T message)
        {
            // noop

            return context;
        }

        /// <inheritdoc/>
        public IDisposable WriteFooter<T>(IDisposable context, T record)
        {
            // noop
            return context;
        }

        /// <inheritdoc/>
        public void Dispose(IDisposable context)
        {
            if (context is not JsonTextWriter json)
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
