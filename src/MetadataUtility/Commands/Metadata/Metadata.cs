// <copyright file="Rename.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Commands.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using LanguageExt;
    using LanguageExt.Common;
    using Microsoft.Extensions.Logging;
    using MetadataUtility.Extensions.System;
    using MetadataUtility.Filenames;
    using MetadataUtility.Models;
    using MetadataUtility.Utilities;
    using NodaTime;

    public class Metadata : EmuCommandHandler
    {
        private readonly ILogger<Metadata> logger;
        private readonly IFileSystem fileSystem;
        private readonly FileMatcher fileMatcher;
        private readonly OutputRecordWriter writer;

        public Metadata(ILogger<Metadata> logger, IFileSystem fileSystem, FileMatcher fileMatcher, OutputRecordWriter writer)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
            this.fileMatcher = fileMatcher;
            this.writer = writer;
        }

        public string[] Targets { get; set; }

        // public bool Save {get; set;}

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            var files = this.fileMatcher.ExpandMatches(this.fileSystem.Directory.GetCurrentDirectory(), this.Targets);

            foreach ((string, string) file in files)
            {
                Recording recording = new Recording();

                recording.SourcePath = file.Item2;

                this.writer.Write(recording);
            }

            return 0;
        }
    }
}
