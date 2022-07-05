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
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Extensions.System;
    using Emu.Filenames;
    using Emu.Utilities;
    using LanguageExt;
    using LanguageExt.Common;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    /// <summary>
    /// Renames files.
    /// </summary>
    public class Rename : EmuCommandHandler
    {
        private static readonly Error NoDateError = Error.New("no timestamp found in filename, cannot give give a new offset");
        private static readonly Error NoOffsetError = Error.New("no offset timestamp found in filename, cannot give give a new offset. Try using --offset to give an initial offset to a local date.");
        private readonly ILogger<Rename> logger;
        private readonly ILogger<DryRun> dryRunLogger;
        private readonly IFileSystem fileSystem;
        private readonly FileMatcher fileMatcher;
        private readonly FilenameParser parser;

        public Rename(ILogger<Rename> logger, ILogger<DryRun> dryRunLogger, IFileSystem fileSystem, FileMatcher fileMatcher, OutputRecordWriter writer, FilenameParser parser)
        {
            this.logger = logger;
            this.dryRunLogger = dryRunLogger;
            this.fileSystem = fileSystem;
            this.fileMatcher = fileMatcher;
            this.parser = parser;
            this.Writer = writer;
            this.fileSystem = fileSystem;
            this.fileSystem = fileSystem;
        }

        public string[] Targets { get; set; }

        public DirectoryInfo CopyTo { get; set; }

        public bool Flatten { get; set; }

        public bool DryRun { get; set; }

        public Offset? Offset { get; set; }

        public Offset? NewOffset { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            // validate options

            // resolve targets
            this.logger.LogDebug("Input targets: {targets}", this.Targets);

            var files = this.fileMatcher.ExpandMatches(this.fileSystem.Directory.GetCurrentDirectory(), this.Targets);

            this.WriteHeader<RenameResult>();
            this.Write("Looking for targets...");

            using var dryRun = new DryRun(this.DryRun, this.dryRunLogger);

            var (renames, failed) = await this.ProcessFiles(files, dryRun);
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
                this.WriteFooter($"No files matched targets: {this.Targets.FormatInlineList()}");
            }

            this.WriteFooter($"{renames.Length} files, {success} renamed, {unchanged} unchanged, {fail} failed");

            return ExitCodes.Get(!failed);
        }

        public async ValueTask<(RenameResult[] Results, bool Failed)> ProcessFiles(IEnumerable<(string Base, string File)> files, DryRun dryRun)
        {
            // parse file names, apply rename transforms
            var renames = files
                .ToSeq()
                .Map(this.Parse)
                .Map(this.ApplyOffset)
                .Map(this.ApplyNewOffset)
                .Map(this.ApplyFlatten)
                .Map(this.ApplyMove)
                .Map(this.ToNewName)
                .ToArray();

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

        protected override object FormatCompact<T>(T record)
        {
            return record switch
            {
                RenameResult r => Format(r),
                _ => record,
            };

            string Format(RenameResult r)
            {
                if (r.NewName is null)
                {
                    return $"{r.OldName}\t{r.NewName}\t{r.Reason}";
                }

                return $"{r.OldName}\t{r.NewName}\t";
            }
        }

        protected override object FormatDefault<T>(T record)
        {
            return record switch
            {
                RenameResult r => Format(r),
                _ => record,
            };

            string Format(RenameResult r)
            {
                if (r.NewName is null)
                {
                    return $"-     Error {r.OldName}\n    because {r.Reason}";
                }

                if (r.NewName == r.OldName)
                {
                    return $"- No change {r.OldName}";
                }

                var partial = $"-   Renamed {r.OldName}\n         to {r.NewName}";
                if (r.Reason is not null)
                {
                    partial += $"\n      Error {r.Reason}";
                }

                return partial;
            }
        }

        private RenameResult ToNewName(FilenameTransform transform)
        {
            var old = transform.Old;

            this.logger.LogDebug("Filename transform: is error? {error}, {transform}", transform.New.IsFail, transform.New);

            return transform.New.Case switch
            {
                Error error => new RenameResult(old, default, error.ToString()),
                ParsedFilename newFragments => new RenameResult(old, newFragments.Reconstruct(this.fileSystem), default),
                _ => throw new NotImplementedException(),
            };
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
            return new(result.Base, result.File, fragments, fragments);
        }

        private FilenameTransform ApplyFlatten(FilenameTransform transform)
        {
            if (!this.Flatten)
            {
                return transform;
            }

            if (transform.New.IsFail)
            {
                return transform;
            }

            return transform with { New = FlattenDirectory((ParsedFilename)transform.New) };

            ParsedFilename FlattenDirectory(ParsedFilename fragments)
            {
                return fragments with { Directory = transform.Base };
            }
        }

        private FilenameTransform ApplyMove(FilenameTransform transform)
        {
            if (this.CopyTo == null)
            {
                return transform;
            }

            if (transform.New.IsFail)
            {
                return transform;
            }

            return transform with { New = RelativeDirectory((ParsedFilename)transform.New) };

            ParsedFilename RelativeDirectory(ParsedFilename fragments)
            {
                var basePath = transform.Base;
                var relativeFragment = this.fileSystem.Path.GetRelativePath(basePath, fragments.Directory);
                var newDirectory =
                    this.fileSystem.Path.GetFullPath(
                        this.fileSystem.Path.Combine(this.CopyTo.FullName, relativeFragment));
                return fragments with { Directory = newDirectory };
            }
        }

        private FilenameTransform ApplyOffset(FilenameTransform transform)
        {
            if (transform.New.IsFail)
            {
                return transform;
            }

            var newFragments = (ParsedFilename)transform.New;

            if (this.Offset is null)
            {
                return transform;
            }

            var offset = this.Offset.Value;

            return newFragments switch
            {
                // this needs to come first to short-ciruit the switch
                { OffsetDateTime: not null } => transform,
                { LocalDateTime: not null } => transform with
                {
                    New = newFragments with
                    {
                        OffsetDateTime = newFragments.LocalDateTime.Value.WithOffset(offset),
                    },
                },
                _ => transform with { New = NoDateError },
            };
        }

        private FilenameTransform ApplyNewOffset(FilenameTransform transform)
        {
            if (transform.New.IsFail)
            {
                return transform;
            }

            var newFragments = (ParsedFilename)transform.New;

            if (this.NewOffset is null)
            {
                return transform;
            }

            var offset = this.NewOffset.Value;

            return newFragments switch
            {
                { OffsetDateTime: not null } => transform with
                {
                    New = newFragments with
                    {
                        OffsetDateTime = newFragments.OffsetDateTime.Value.WithOffset(offset),
                    },
                },
                { LocalDateTime: not null } => transform with { New = NoOffsetError },
                _ => transform with { New = NoDateError },
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

        private record FilenameTransform(string Base, string Old, ParsedFilename Fragments, Fin<ParsedFilename> New);
    }
}
