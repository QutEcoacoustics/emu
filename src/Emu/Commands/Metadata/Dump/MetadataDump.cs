// <copyright file="MetadataDump.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Metadata.Dump
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine.Invocation;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Cli.ObjectFormatters;
    using Emu.Metadata;
    using Emu.Metadata.FrontierLabs;
    using Emu.Metadata.SupportFiles;
    using Emu.Metadata.WildlifeAcoustics;
    using Emu.Models;
    using Emu.Utilities;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    using static Emu.Cli.SpectreUtils;

    public class MetadataDump : EmuCommandHandler<Dictionary<string, object>>
    {
        private const string PathKey = "Path";

        private readonly ILogger<MetadataDump> logger;
        private readonly IFileSystem fileSystem;
        private readonly FileMatcher fileMatcher;
        private readonly PrettyFormatter pretty;
        private readonly CompactFormatter compact;
        private readonly IEnumerable<IRawMetadataOperation> extractors;

        public MetadataDump(
            ILogger<MetadataDump> logger,
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
            this.extractors = register.AllRaw;
        }

        public string[] Targets { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            if (this.Format == EmuCommand.OutputFormat.CSV)
            {
                throw new NotSupportedException();
            }

            var paths = this.fileMatcher.ExpandMatches(this.fileSystem.Directory.GetCurrentDirectory(), this.Targets);

            this.WriteHeader();

            // Extract recording information from each path
            foreach (var path in paths)
            {
                using var target = new TargetInformation(this.fileSystem, path.Base, path.File);
                var result = new Dictionary<string, object>()
                {
                    { PathKey, target.Path },
                };

                foreach (var extractor in this.extractors)
                {
                    this.logger.LogDebug("Running {Extractor} on {Target}", extractor.Name, target.Path);

                    if (await extractor.CanProcessAsync(target))
                    {
                        var name = extractor.Name;
                        var item = await extractor.ProcessFileAsync(target);
                        result[name] = item;
                    }
                }

                this.Write(result);
            }

            this.WriteFooter();

            return ExitCodes.Success;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All types printed are known to us")]
        public override string FormatCompact(Dictionary<string, object> record)
        {
            StringBuilder builder = new();

            this.compact.Print(builder, record);

            return builder.ToString();
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All types printed are known to us")]
        public override object FormatRecord(Dictionary<string, object> record)
        {
            StringBuilder builder = new();

            builder.Append(MarkupFileSection((string)record[PathKey]));

            bool any = false;
            foreach (var kvp in record.Filter(kvp => kvp.Key != PathKey))
            {
                builder.AppendFormat("Block [darkgoldenrod]{0}[/]:\n", kvp.Key);

                this.pretty.Print(builder, kvp.Value, new() { Depth = 1 });

                any = true;
            }

            if (!any)
            {
                builder.AppendLine(MarkupWarning("No metadata blocks found"));
            }

            return builder;
        }
    }
}
