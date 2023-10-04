// <copyright file="Rename.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Rename
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Filenames;
    using Emu.Metadata;
    using Emu.Metadata.SupportFiles;
    using Emu.Models;
    using Emu.Utilities;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using static Emu.Cli.SpectreUtils;
    using static Emu.Utilities.DryRun;
    using static LanguageExt.Prelude;
    using Error = LanguageExt.Common.Error;

    /// <summary>
    /// Renames files.
    /// </summary>
    public class Rename : EmuCommandHandler<RenameResult>
    {
        private static readonly Error NoDateError = Error.New("no timestamp found in filename, cannot give give a new offset");
        private static readonly Error NoOffsetError = Error.New("no offset timestamp found in filename, cannot give give a new offset. Try using --offset to give an initial offset to a local date.");

        private static readonly Regex LocalDateTokenRegex = new($"{nameof(Recording.LocalStartDate).AsToken()}");

        private readonly ILogger<Rename> logger;
        private readonly DryRunFactory dryRunFactory;
        private readonly IFileSystem fileSystem;
        private readonly FilenameExtractor filenameExtractor;
        private readonly FileMatcher fileMatcher;
        private readonly SupportFileFinder fileFinder;
        private readonly FilenameParser parser;
        private readonly MetadataRegister extractorRegister;
        private readonly FilenameGenerator generator;
        private IEnumerable<IMetadataOperation> allExtractors;

        public Rename(
            ILogger<Rename> logger,
            DryRunFactory dryRunFactory,
            IFileSystem fileSystem,
            FileMatcher fileMatcher,
            SupportFileFinder fileFinder,
            OutputRecordWriter writer,
            FilenameParser parser,
            MetadataRegister extractorRegister,
            FilenameGenerator generator)
        {
            this.logger = logger;
            this.dryRunFactory = dryRunFactory;
            this.fileSystem = fileSystem;
            this.fileMatcher = fileMatcher;
            this.fileFinder = fileFinder;
            this.parser = parser;
            this.extractorRegister = extractorRegister;
            this.generator = generator;
            this.Writer = writer;
            this.fileSystem = fileSystem;
            this.fileSystem = fileSystem;

            this.filenameExtractor = this.extractorRegister.Get<FilenameExtractor>();
        }

        public string[] Targets { get; set; }

        public DirectoryInfo CopyTo { get; set; }

        public bool Flatten { get; set; }

        public bool DryRun { get; set; }

        public Offset? Offset { get; set; }

        public Offset? NewOffset { get; set; }

        public bool ScanMetadata { get; set; }

        public string Template { get; set; }

        // lazy generate a list of extractors
        // lazy because generation will do service resolution
        // don't include filename extractor because that operation is done by default
        private IEnumerable<IMetadataOperation> AllExtractors =>
            this.allExtractors ??= this.extractorRegister.All.Where(t => t is not FilenameExtractor or HashCalculator);

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            // validate options

            // resolve targets
            this.logger.LogDebug("Input targets: {targets}", this.Targets);

            var files = this.fileMatcher.ExpandMatches(this.fileSystem.Directory.GetCurrentDirectory(), this.Targets);

            this.WriteMessage("Looking for targets...");
            this.WriteHeader();

            using var dryRun = this.dryRunFactory(this.DryRun);

            var (renames, failed) = await this.ProcessFiles(files, dryRun);

            this.WriteFooter();

            int success = 0, unchanged = 0, fail = 0;
            if (renames.Any())
            {
                if (failed)
                {
                    fail = renames.Length;
                }
                else
                {
                    foreach (var result in renames)
                    {
                        if (result.Reason is not null)
                        {
                            fail++;
                        }
                        else if (result.OldName == result.NewName)
                        {
                            unchanged++;
                        }
                        else
                        {
                            success++;
                        }
                    }
                }
            }
            else
            {
                this.WriteMessage($"No files matched targets: {this.Targets.FormatInlineList()}");
            }

            this.WriteMessage($"{renames.Length} files, {success} renamed, {unchanged} unchanged, {fail} failed");

            return ExitCodes.Get(!failed);
        }

        public async ValueTask<(RenameResult[] Results, bool Failed)> ProcessFiles(IEnumerable<(string Base, string File)> files, DryRun dryRun)
        {
            // parse file names, apply rename transforms
            var renames = await this.ApplyOperations(files).ToArrayAsync();

            // check if any of the new names will have issues
            var conflicts = this.CheckNewPaths(renames);
            if (conflicts > 0)
            {
                this.logger.LogError("{conflicts} files would be overwritten by the rename operation, cancelling rename.", conflicts);
                foreach (var rename in renames)
                {
                    this.Write(rename);
                }

                return (renames, true);
            }

            foreach (var rename in renames)
            {
                await this.ApplyRename(rename, dryRun);
            }

            return (renames, false);
        }

        public override string FormatCompact(RenameResult record)
        {
            var r = record;

            if (r.NewName is null)
            {
                return $"{r.OldName}\t{r.NewName}\t{r.Reason}";
            }

            return $"{r.OldName}\t{r.NewName}\t";
        }

        public override object FormatRecord(RenameResult record)
        {
            var r = record;

            if (r.NewName is null)
            {
                return $"-     Error {MarkupPath(r.OldName)}\n    because {r.Reason}";
            }

            if (r.NewName == r.OldName)
            {
                return $"- No change {MarkupPath(r.OldName)}";
            }

            var partial = $"-   Renamed {MarkupPath(r.OldName)}\n         to {MarkupPath(r.NewName)}";
            if (r.Reason is not null)
            {
                partial += $"\n      Error {r.Reason}";
            }

            return partial;
        }

        private async IAsyncEnumerable<RenameResult> ApplyOperations(IEnumerable<(string Base, string File)> files)
        {
            foreach (var file in files)
            {
                this.logger.LogDebug("Processing file {path}", file.File);

                var parse = this.Parse(file);

                var moreMetadata = await this.ExtractMetadata(parse);

                var transformed = moreMetadata
                    .Bind(this.ForceOffsetDate)
                    .Bind(this.ApplyTemplate)
                    .Bind(this.ApplyOffset)
                    .Bind(this.ApplyNewOffset)
                    .Bind(this.ApplyFlatten)
                    .Bind(this.ApplyMove);

                yield return this.ToNewName(transformed, parse.Old);
            }
        }

        private RenameResult ToNewName(Fin<FilenameTransform> transform, string oldPath)
        {
            this.logger.LogDebug("Filename transform: is error? {error}", transform.IsFail ? (Error)transform : default);
            this.logger.LogTrace("Filename transform: {transform}", transform.IfFail(null));

            return transform.Case switch
            {
                Error error => new RenameResult(oldPath, default, error.ToString()),
                FilenameTransform t => Generate(t),
                _ => throw new NotImplementedException(),
            };

            RenameResult Generate(FilenameTransform t)
            {
                var newName = this.generator
                    .Reconstruct(t.TokenTemplate, t.Data)
                    .Map(n => this.fileSystem.Path.Combine(t.Data.Directory, n));

                return newName.Match(
                    Succ: name => new RenameResult(t.Old, name, default),
                    Fail: error => new RenameResult(oldPath, default, error.ToString()));
            }
        }

        private ValueTask ApplyRename(RenameResult rename, DryRun dryRun)
        {
            if (rename.NewName is null)
            {
                dryRun.WouldDo($"skip rename", () =>
                {
                    this.logger.LogDebug("Skipping rename of {old} because {reason}", rename.OldName, rename.Reason);
                });

                this.Write(rename);
                return default;
            }

            var directory = this.fileSystem.Path.GetDirectoryName(rename.NewName);
            if (!this.fileSystem.Directory.Exists(directory))
            {
                dryRun.WouldDo($"create directory: {directory}", CreateDirectory);
            }

            if (this.CopyTo is not null)
            {
                dryRun.WouldDo($"copy and rename:", Copy);
            }
            else
            {
                dryRun.WouldDo($"rename:", Rename);
            }

            this.Write(rename);

            return default;

            void CreateDirectory()
            {
                this.fileSystem.Directory.CreateDirectory(directory!);
            }

            void Rename()
            {
                this.fileSystem.File.Move(rename.OldName, rename.NewName, overwrite: false);
            }

            void Copy()
            {
                this.fileSystem.File.Copy(rename.OldName, rename.NewName, overwrite: false);
            }
        }

        private FilenameTransform Parse((string Base, string File) result)
        {
            var fragments = this.parser.Parse(result.File);
            var stem = this.fileSystem.Path.GetFileNameWithoutExtension(result.File);
            var size = this.fileSystem.FileInfo.New(result.File).Length;

            var recording = this.filenameExtractor.ApplyValues(
                new Recording
                {
                    Path = result.File,
                },
                fragments,
                stem,
                (ulong)size);

            return new FilenameTransform(result.Base, result.File, fragments, recording, fragments.TokenizedName);
        }

        private async ValueTask<Fin<FilenameTransform>> ExtractMetadata(FilenameTransform transform)
        {
            if (!this.ScanMetadata)
            {
                this.logger.LogDebug("Skipping extended metadata scan");
                return transform;
            }

            this.logger.LogDebug("Doing extended metadata scan");

            using var target = new TargetInformation(this.fileSystem, transform.Base, transform.Data.Path);

            this.fileFinder.FindSupportFiles(transform.Data.Directory, target.AsArray());

            Recording recording = transform.Data;
            foreach (var extractor in this.AllExtractors)
            {
                if (await extractor.CanProcessAsync(target))
                {
                    this.logger.LogTrace("Extracting metadata with {extractor}", extractor.GetType().Name);
                    recording = await extractor.ProcessFileAsync(target, recording);
                }
            }

            return transform with { Data = recording };
        }

        private Fin<FilenameTransform> ApplyFlatten(FilenameTransform transform)
        {
            if (!this.Flatten)
            {
                return transform;
            }

            return transform with { Data = FlattenDirectory(transform.Data) };

            Recording FlattenDirectory(Recording fragments)
            {
                return fragments with { Directory = transform.Base };
            }
        }

        private Fin<FilenameTransform> ApplyMove(FilenameTransform transform)
        {
            if (this.CopyTo == null)
            {
                return transform;
            }

            return transform with { Data = RelativeDirectory(transform.Data) };

            Recording RelativeDirectory(Recording fragments)
            {
                var basePath = transform.Base;
                var relativeFragment = this.fileSystem.Path.GetRelativePath(basePath, fragments.Directory);
                var newDirectory =
                    this.fileSystem.Path.GetFullPath(
                        this.fileSystem.Path.Combine(this.CopyTo.FullName, relativeFragment));
                return fragments with { Directory = newDirectory };
            }
        }

        private Fin<FilenameTransform> ForceOffsetDate(FilenameTransform transform)
        {
            var match = LocalDateTokenRegex.Match(transform.TokenTemplate);

            if (!match.Success)
            {
                return transform;
            }

            this.logger.LogDebug("Substituting local date for offset date in rename template");

            return transform with
            {
                TokenTemplate = match.Result($"$`{nameof(Recording.StartDate).AsToken()}$'"),
            };
        }

        private Fin<FilenameTransform> ApplyTemplate(FilenameTransform transform)
        {
            if (string.IsNullOrEmpty(this.Template))
            {
                return transform;
            }

            this.logger.LogDebug("Applying template {template}", this.Template);

            return transform with
            {
                TokenTemplate = this.Template,
            };
        }

        private Fin<FilenameTransform> ApplyOffset(FilenameTransform transform)
        {
            if (this.Offset is null)
            {
                return transform;
            }

            var offset = this.Offset.Value;

            return transform.Data switch
            {
                // this needs to come first to short-ciruit the switch
                { StartDate: not null } => transform,
                { LocalStartDate: not null } => transform with
                {
                    Data = transform.Data with
                    {
                        StartDate = transform.Data.LocalStartDate.Value.WithOffset(offset),
                    },
                },
                _ => NoDateError,
            };
        }

        private Fin<FilenameTransform> ApplyNewOffset(FilenameTransform transform)
        {
            if (this.NewOffset is null)
            {
                return transform;
            }

            var offset = this.NewOffset.Value;

            return transform.Data switch
            {
                { StartDate: not null } => transform with
                {
                    Data = transform.Data with
                    {
                        StartDate = transform.Data.StartDate.Value.WithOffset(offset),
                    },
                },
                { LocalStartDate: not null } => NoOffsetError,
                _ => NoDateError,
            };
        }

        /// <summary>
        /// Generates renamed paths for recordings.
        /// </summary>
        private int CheckNewPaths(RenameResult[] renames)
        {
            var hash = new System.Collections.Generic.HashSet<string>(renames.Length * 2);
            var conflicts = 0;
            for (var i = 0; i < renames.Length; i++)
            {
                var rename = renames[i];
                if (!hash.Add(rename.OldName))
                {
                    var error = $"conflicts with a path of the same name";
                    renames[i] = rename with { Reason = error };
                    conflicts++;
                }

                if (rename.NewName == rename.OldName)
                {
                    // name has turned out the same, not an error
                    continue;
                }

                if (rename.Reason is not null or "")
                {
                    // other errors will prevent reneame
                    continue;
                }

                if (!hash.Add(rename.NewName))
                {
                    var error = $"conflicts with another renamed path";
                    renames[i] = rename with { Reason = error };
                    conflicts++;
                    continue;
                }

                if (this.fileSystem.File.Exists(rename.NewName))
                {
                    var error = $"conflicts with a pre-existing file";
                    renames[i] = rename with { Reason = error };
                    conflicts++;
                }
            }

            return conflicts;
        }

        private record FilenameTransform(
            string Base,
            string Old,
            ParsedFilename Fragments,
            Recording Data,
            string TokenTemplate);
    }
}
