// <copyright file="OutputWriter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using MetadataUtility.Serialization;

    /// <summary>
    /// Writes record results to an output.
    /// </summary>
    public class OutputWriter
    {
        private readonly ISerializer serializer;
        private readonly TextWriter sink;

        public OutputWriter(ISerializer serializer, TextWriter sink)
        {
            this.serializer = serializer;
            this.sink = sink;
        }

        public async Task<bool> Write(Recording recording)
        {
            this.serializer.Serialize(this.sink, )
        }
    }
}
