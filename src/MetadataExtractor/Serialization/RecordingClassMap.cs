// <copyright file="RecordingClassMap.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataExtractor.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using CsvHelper.Configuration;
    using MetadataExtractor.Models;

    /// <inheritdoc />
    internal class RecordingClassMap : ClassMap<Recording>
    {
        public RecordingClassMap()
        {
            this.AutoMap();
        }
    }
}
