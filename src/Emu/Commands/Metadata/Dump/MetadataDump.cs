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
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Cli.ObjectFormatters;
    using Emu.Metadata;
    using Emu.Models.Notices;
    using Emu.Utilities;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using static Emu.Cli.SpectreUtils;

    public class MetadataDump : EmuCommandHandler<Dictionary<string, MetadataExtractionResult>>
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

        public string[] Blocks { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            if (this.Format == EmuCommand.OutputFormat.CSV)
            {
                throw new NotSupportedException();
            }

            var paths = this.fileMatcher.ExpandMatches(this.fileSystem.Directory.GetCurrentDirectory(), this.Targets);
            var selectedBlocks = this.GetSelectedBlocks();

            this.WriteHeader();

            // Extract recording information from each path
            foreach (var path in paths)
            {
                using var target = new TargetInformation(this.fileSystem, path.Base, path.File);
                var result = new Dictionary<string, MetadataExtractionResult>()
                {
                    { PathKey, new MetadataExtractionResult(target.Path, Seq.empty<Notice>()) },
                };

                foreach (var extractor in this.extractors)
                {
                    this.logger.LogDebug("Running {Extractor} on {Target}", extractor.Name, target.Path);

                    if (selectedBlocks.Count > 0 && !selectedBlocks.Contains(extractor.Name))
                    {
                        this.logger.LogTrace("Skipping extractor {Extractor} because it is not in selected block filter", extractor.Name);
                        continue;
                    }

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
        public override string FormatCompact(Dictionary<string, MetadataExtractionResult> record)
        {
            StringBuilder builder = new();

            foreach (var kvp in record)
            {
                if (kvp.Key == PathKey)
                {
                    builder.Append($"Path={kvp.Value.Data};");
                    continue;
                }

                this.compact.Print(builder, kvp.Value.Data, new()
                {
                    KeyPrefix = kvp.Key + ".",
                });

                if (kvp.Value.Notices.Any())
                {
                    builder.Append(';');
                    this.compact.Print(builder, kvp.Value.Notices, new()
                    {
                        KeyPrefix = kvp.Key + ".Notices.",
                    });
                }
            }

            return builder.ToString();
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All types printed are known to us")]
        public override object FormatRecord(Dictionary<string, MetadataExtractionResult> record)
        {
            StringBuilder builder = new();

            builder.Append(MarkupFileSection((string)record[PathKey].Data));

            bool any = false;
            foreach (var kvp in record.Filter(kvp => kvp.Key != PathKey))
            {
                builder.AppendFormat("Block [darkgoldenrod]{0}[/]:\n", kvp.Key);

                this.pretty.Print(builder, kvp.Value.Data, new() { Depth = 1 });

                if (kvp.Value.Notices.Any())
                {
                    builder.AppendLine("Notices:");
                    this.pretty.Print(builder, kvp.Value.Notices, new() { Depth = 1 });
                }

                any = true;
            }

            if (!any)
            {
                builder.AppendLine(MarkupWarning("No metadata blocks found"));
            }

            return builder;
        }

        private System.Collections.Generic.HashSet<string> GetSelectedBlocks()
        {
            if (this.Blocks is null || this.Blocks.Length == 0)
            {
                return new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["guan"] = "GUANO",
                ["guano"] = "GUANO",
                ["wamd"] = "WAMD",
            };

            var available = this.extractors
                .Select(x => x.Name)
                .ToDictionary(x => x, x => x, StringComparer.OrdinalIgnoreCase);

            var selected = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var block in this.Blocks)
            {
                if (string.IsNullOrWhiteSpace(block))
                {
                    continue;
                }

                var key = block.Trim();
                if (aliases.TryGetValue(key, out var canonicalAlias))
                {
                    key = canonicalAlias;
                }

                if (available.TryGetValue(key, out var extractorName))
                {
                    selected.Add(extractorName);
                }
                else
                {
                    this.logger.LogWarning("Requested metadata block filter '{block}' does not match any known metadata dump blocks", block);
                }
            }

            return selected;
        }
    }
}
