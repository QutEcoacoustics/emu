// <copyright file="OutputWriter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using MetadataUtility.Models;
    using MetadataUtility.Serialization;

    /// <summary>
    /// Writes record results to an output.
    /// </summary>
    public class OutputWriter : IDisposable
    {
        private readonly ISerializer serializer;
        private readonly TextWriter sink;
        private IDisposable serializerContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputWriter"/> class.
        /// </summary>
        /// <param name="serializer">The serializer to use.</param>
        /// <param name="sink">The output to write to.</param>
        public OutputWriter(ISerializer serializer, TextWriter sink)
        {
            this.serializer = serializer;
            this.sink = sink;
        }

        /// <summary>
        /// Serializes a single record to the output sink and
        /// if needed also writes a header.
        /// </summary>
        /// <param name="recording">The recording to serialize.</param>
        public void Write(Recording recording)
        {
            if (this.serializerContext == null)
            {
                // TODO: possible race condition
                this.serializerContext = this.serializer.WriteHeader<Recording>(this.sink);
            }

            this.serializerContext = this.serializer.WriteRecord(this.serializerContext, this.sink, recording);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // don't write a footer if a header was never written
            if (this.serializerContext != null)
            {
                var context = this.serializer.WriteFooter<Recording>(this.serializerContext, this.sink);
                context.Dispose();
            }

            this.sink?.Dispose();
        }
    }
}
