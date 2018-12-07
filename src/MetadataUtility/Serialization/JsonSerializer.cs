// <copyright file="JsonSerializer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Serialization.JsonNet;

    /// <inheritdoc cref="CsvHelper.ISerializer"/>
    public class JsonSerializer : ISerializer
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
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            this.serializer = Newtonsoft.Json.JsonSerializer.Create(this.settings);
        }

        /// <inheritdoc />
        public string Serialize<T>(IEnumerable<T> objects)
        {
            using (var stringWriter = new StringWriter())
            {
                this.Serialize(stringWriter, objects);

                return stringWriter.ToString();
            }
        }

        /// <inheritdoc />
        public void Serialize<T>(TextWriter writer, IEnumerable<T> objects)
        {
            this.serializer.Serialize(writer, objects);
        }

        /// <inheritdoc />
        public IEnumerable<T> Deserialize<T>(TextReader reader)
        {
            using (var jsonReader = new JsonTextReader(reader))
            {
                return this.serializer.Deserialize<IEnumerable<T>>(jsonReader);
            }
        }
    }
}
