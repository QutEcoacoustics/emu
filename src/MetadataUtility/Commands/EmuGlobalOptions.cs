// <copyright file="EmuGlobalOptions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using static MetadataUtility.EmuCommand;

    public class EmuGlobalOptions
    {
        public bool Verbose { get; set; }

        public bool VeryVerbose { get; set; }

        public LogLevel LogLevel { get; set; }

        public OutputFormat Format { get; set; }

        public string Output { get; set; }
    }
}
