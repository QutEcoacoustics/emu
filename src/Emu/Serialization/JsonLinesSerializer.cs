// <copyright file="JsonLinesSerializer.cs" company="QutEcoacoustics">
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
    public class JsonLinesSerializer : ISerializer, IRecordFormatter
    {
        private readonly JsonSerializerSettings settings;
        private readonly Newtonsoft.Json.JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonLinesSerializer"/> class.
        /// </summary>
        public JsonLinesSerializer()
        {
            // for the love of god make sure the same value converters are registered
            // for both this and the standard JsonSerializer
            this.settings = JsonSerializer.Settings;
            this.settings.Formatting = Formatting.None;

            this.serializer = Newtonsoft.Json.JsonSerializer.Create(this.settings);
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
            foreach (var @object in objects)
            {
                this.serializer.Serialize(writer, @object);
                writer.WriteLine();
            }
        }

        /// <inheritdoc />
        public IDisposable WriteHeader<T>(IDisposable context, T record)
        {
            var json = new JsonTextWriter(this.Writer);

            // noop

            return json;
        }

        /// <inheritdoc />
        public IDisposable WriteRecord<T>(IDisposable context, T record)
        {
            var json = (JsonTextWriter)context;
            this.serializer.Serialize(json, record);
            this.Writer.WriteLine();

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
            // json lines does not have a footer
            return context;
        }

        /// <inheritdoc/>
        public void Dispose(IDisposable context)
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
