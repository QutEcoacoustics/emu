// <copyright file="Metadata.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Commands.Metadata
{
    using System.CommandLine.Invocation;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using MetadataUtility.Cli;
    using MetadataUtility.Metadata;
    using MetadataUtility.Metadata.SupportFiles;
    using MetadataUtility.Models;
    using MetadataUtility.Utilities;
    using Microsoft.Extensions.Logging;

    public class Metadata : EmuCommandHandler
    {
        private readonly ILogger<Metadata> logger;
        private readonly IFileSystem fileSystem;
        private readonly FileMatcher fileMatcher;
        private readonly OutputRecordWriter writer;
        private readonly MetadataRegister extractorRegister;

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
            this.extractorRegister = register;
        }

        public string[] Targets { get; set; }

        // public bool Save {get; set;}

        public override async Task<int> InvokeAsync(InvocationContext invocationContext)
        {
            var paths = this.fileMatcher.ExpandMatches(this.fileSystem.Directory.GetCurrentDirectory(), this.Targets);

            foreach (var path in paths)
            {
                using var context = this.CreateContainer(path);

                SupportFile.FindSupportFiles(context);

                Recording recording = new Recording
                {
                    SourcePath = context.Path,
                };

                foreach (var extractor in this.extractorRegister.All)
                {
                    if (await extractor.CanProcessAsync(context))
                    {
                        recording = await extractor.ProcessFileAsync(context, recording);
                    }
                }

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
