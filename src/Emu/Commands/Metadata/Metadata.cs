// <copyright file="Metadata.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Metadata
{
    using System.Collections.Generic;
    using System.CommandLine.Invocation;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Cli.ObjectFormatters;
    using Emu.Extensions.Microsoft.Extensions;
    using Emu.Metadata;
    using Emu.Metadata.SupportFiles;
    using Emu.Models;
    using Emu.Utilities;
    using Microsoft.Extensions.Logging;
    using static Emu.Cli.SpectreUtils;
    using Level = Microsoft.Extensions.Logging.LogLevel;

    public class Metadata : EmuCommandHandler<Recording>
    {
        private readonly ILogger<Metadata> logger;
        private readonly IFileSystem fileSystem;
        private readonly FileMatcher fileMatcher;
        private readonly PrettyFormatter pretty;
        private readonly CompactFormatter compact;
        private readonly IEnumerable<IMetadataOperation> allExtractors;

        public Metadata(
            ILogger<Metadata> logger,
            IFileSystem fileSystem,
            FileMatcher fileMatcher,
            OutputRecordWriter writer,
            MetadataRegister register,
            PrettyFormatter pretty,
            CompactFormatter compact)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
            this.fileMatcher = fileMatcher;
            this.pretty = pretty;
            this.compact = compact;
            this.Writer = writer;

            // the extension inferer is useful in the rename and repair scenarios
            // but not as useful in the metadata command where we want accurate output of data
            this.allExtractors = register.All.Where(r => r.GetType() != typeof(ExtensionInferer));
        }

        public string[] Targets { get; set; }

        public bool NoChecksum { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext invocationContext)
        {
            // Filter out HashCalculator if no checksum option is
            var filteredExtractors = this.NoChecksum ? this.allExtractors.Where(x => x is not HashCalculator) : this.allExtractors;

            var paths = this.fileMatcher.ExpandMatches(this.fileSystem.Directory.GetCurrentDirectory(), this.Targets);

            Dictionary<string, List<TargetInformation>> targetDirectories = new Dictionary<string, List<TargetInformation>>();

            // Group targets together according to their directories
            // This is done so that only one search for support files is done per directory
            this.logger.LogDebug("Grouping files into directories");
            foreach (var path in paths)
            {
                using var context = this.CreateContainer(path);

                string directory = context.FileSystem.Path.GetDirectoryName(context.Path);

                if (targetDirectories.ContainsKey(directory))
                {
                    targetDirectories[directory].Add(context);
                }
                else
                {
                    targetDirectories[directory] = new List<TargetInformation> { context };
                }
            }

            this.WriteHeader();

            // Extract recording information from each target
            foreach ((string directory, List<TargetInformation> targets) in targetDirectories)
            {
                using (this.logger.Measure("Searching for support files", level: Level.Trace))
                {
                    SupportFile.FindSupportFiles(directory, targets, this.fileSystem);
                }

                foreach (TargetInformation target in targets)
                {
                    this.logger.LogDebug("Processing target {path}", target.Path);

                    Recording recording = new Recording
                    {
                        Path = target.Path,
                    };

                    foreach (var extractor in filteredExtractors)
                    {
                        using (this.logger.Measure($"Running extractor {extractor.GetType().Name}", Level.Trace))
                        {
                            if (await extractor.CanProcessAsync(target))
                            {
                                recording = await extractor.ProcessFileAsync(target, recording);
                            }
                        }
                    }

                    this.Write(recording);
                }
            }

            this.WriteFooter();

            return ExitCodes.Success;
        }

        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
            Justification = "All the types are known to us.")]
        public override string FormatCompact(Recording record)
        {
            StringBuilder builder = new();

            this.compact.Print(builder, record);

            return builder.ToString();
        }

        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
            Justification = "All the types are known to us.")]
        public override object FormatRecord(Recording record)
        {
            StringBuilder builder = new();

            builder.Append(MarkupFileSection(record.Path));
            this.pretty.Print(builder, record, new() { Except = (k) => k == nameof(Recording.Path) });

            return builder;
        }

        private TargetInformation CreateContainer((string Base, string File) target)
        {
            return new TargetInformation(this.fileSystem, target.Base, target.File);
        }
    }
}
