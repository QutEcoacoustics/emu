// <copyright file="Metadata.cs" company="QutEcoacoustics">
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
    using MetadataUtility.Cli;
    using MetadataUtility.Metadata;

    public class Metadata : EmuCommandHandler
    {
        private readonly ILogger<Metadata> logger;
        private readonly IFileSystem fileSystem;
        private readonly FileMatcher fileMatcher;
        private readonly OutputRecordWriter writer;
        private readonly MetadataRegister register;

        public Metadata(
            ILogger<Metadata> logger,
            IFileSystem fileSystem,
            FileMatcher fileMatcher,
            OutputRecordWriter writer,
            MetadataRegister register)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
            this.fileMatcher = fileMatcher;
            this.writer = writer;
            this.register = register;
        }

        public string[] Targets { get; set; }

        // public bool Save {get; set;}

        public override async Task<int> InvokeAsync(InvocationContext _)
        {
            var paths = this.fileMatcher.ExpandMatches(this.fileSystem.Directory.GetCurrentDirectory(), this.Targets);

            var contexts = paths.Select(this.CreateContainer);

            foreach (var context in contexts)
            {
                Recording recording = new Recording
                {
                    SourcePath = context.Path,
                };

                foreach (var operation in this.register.All)
                {
                    if (await operation.CanProcessAsync(context))
                    {
                        recording = await operation.ProcessFileAsync(context, recording);
                    }
                }

                context.Dispose();

                this.writer.Write(recording);
            }

            return ExitCodes.Success;
        }

        private TargetInformation CreateContainer((string Base, string File) target)
        {
            return new TargetInformation(this.fileSystem)
            {
                Base = target.Base,
                Path = target.File,
            };
        }
    }
}
