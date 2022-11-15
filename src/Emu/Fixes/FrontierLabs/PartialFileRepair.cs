// <copyright file="PartialFileRepair.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.FrontierLabs
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Filenames;
    using Emu.Fixes;
    using Emu.Fixes.OpenEcoacoustics;
    using Emu.Metadata;
    using Emu.Metadata.FrontierLabs;
    using Emu.Models;
    using Emu.Utilities;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    using static Emu.Audio.Vendors.FrontierLabs;
    using static Emu.Fixes.CheckStatus;
    using Error = LanguageExt.Common.Error;

    public class PartialFileRepair : IFixOperation
    {
        public const string ProblemFileName = "data";

        public static readonly string EmuPatched = WellKnownProblems.PatchString(Metadata.Problem);

        private readonly ILogger<PartialFileRepair> logger;
        private readonly IFileSystem fileSystem;
        private readonly FileUtilities fileUtilities;
        private readonly FilenameGenerator filenameGenerator;
        private readonly MetadataDurationBug metadataDurationBug;
        private readonly IMetadataOperation[] metadataExtractors;

        public PartialFileRepair(
            ILogger<PartialFileRepair> logger,
            IFileSystem fileSystem,
            FileUtilities fileUtilities,
            MetadataRegister register,
            FilenameGenerator filenameGenerator,
            MetadataDurationBug metadataDurationBug)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
            this.fileUtilities = fileUtilities;
            this.filenameGenerator = filenameGenerator;
            this.metadataDurationBug = metadataDurationBug;
            this.metadataExtractors = new IMetadataOperation[]
            {
                register.Get<FlacHeaderExtractor>(),
                register.Get<FlacCommentExtractor>(),
                register.Get<ExtensionInferer>(),
            };
        }

        public static OperationInfo Metadata => new(
            WellKnownProblems.FrontierLabsProblems.PartialDataFiles,
            Fixable: true,
            Safe: false,
            Automatic: true,
            typeof(PartialFileRepair),
            Suffix: "partial");

        public async Task<CheckResult> CheckAffectedAsync(string file)
        {
            var filename = this.fileSystem.Path.GetFileName(file);

            using var stream = this.fileSystem.File.OpenRead(file);

            // predicates
            var filenameMatches = filename == ProblemFileName;
            var isEmpty = stream.Length == 0;
            var isFlac = Flac.IsFlacFile(stream);
            var firmware = await ReadFirmwareAsync(stream);
            var wasRepaired = firmware.Map(HasBeenRepaired).IfFail(false);

            bool isPreallocatedWaveFile = false;
            if (!isFlac.IfFail(false))
            {
                isPreallocatedWaveFile = (await IsPreallocatedFile(stream, this.fileUtilities)).IfFail(false);
            }

            var data = new Data(isEmpty, isFlac.IfFail(false), isPreallocatedWaveFile, firmware);

            return filenameMatches switch
            {
                false when wasRepaired => new CheckResult(Repaired, Severity.None, "File has already been reconstructed"),
                false => new CheckResult(NotApplicable, Severity.None, "File is not named `data`"),

                // there is a case where sometimes the partial files are empty
                // we take care of those too
                true when data.IsEmpty => new CheckResult(Affected, Severity.Severe, "Partial file detected", data),

                true when data.IsPreallocatedFile =>
                    new CheckResult(Affected, Severity.Severe, "Partial file detected: " + PreAllocatedHeader.Message, data),

                true when data.IsFlac == false =>
                    new CheckResult(
                        CheckStatus.Error,
                        Severity.None,
                        "Unsupported format. Please send this example to the EMU repository so we can code for this case."),

                // and that's it... a file named data and it is a FLAC file.
                // the data files can have errors, but since they're already marked as error files we'll work with those
                // in the next stage.
                true => new CheckResult(Affected, Severity.Moderate, "Partial file detected", data),
            };

            static bool HasBeenRepaired(FirmwareRecord firmwareRecord) => firmwareRecord.Tags.Contains(EmuPatched);
        }

        public OperationInfo GetOperationInfo() => Metadata;

        public async Task<FixResult> ProcessFileAsync(string file, DryRun dryRun)
        {
            var affected = await this.CheckAffectedAsync(file);

            if (affected is { Status: Affected })
            {
                var data = (Data)affected.Data;
                return await this.FixFile(file, affected, data, dryRun);
            }
            else
            {
                return new FixResult(FixStatus.NoOperation, affected, affected.Message);
            }
        }

        private async Task<FixResult> FixFile(string file, CheckResult affected, Data data, DryRun dryRun)
        {
            string newBasename = null;
            FixStatus fixStatus = default;
            string message = null;
            FirmwareRecord firmwareRecord = default;

            string directory = this.fileSystem.Path.GetDirectoryName(file);

            // use a readonly file stream for information gathering phase
            bool fail = false;
            using (var target = new TargetInformation(this.fileSystem, directory, file))
            {
                // special case empty partial files
                if (data.IsEmpty)
                {
                    fail = true;
                    this.logger.LogDebug("File is empty, renaming, halting");
                    newBasename = EmptyFile.Metadata.GetErrorName(this.fileSystem, file);
                    message = "Partial file was empty";
                    fixStatus = FixStatus.Renamed;
                }

                // special case preallcoated partial files
                if (data.IsPreallocatedFile)
                {
                    fail = true;
                    this.logger.LogDebug("File is a stub, renaming, halting");
                    newBasename = PreAllocatedHeader.Metadata.GetErrorName(this.fileSystem, file);
                    message = "Partial file was a stub and has no useable data";
                    fixStatus = FixStatus.Renamed;
                }

                if (!fail)
                {
                    // get firmware so we can stamp a fix
                    this.logger.LogDebug("Reading firmware");

                    var firmwareTask = await ReadFirmwareAsync(target.FileStream);
                    if (firmwareTask.Case is FirmwareRecord firmware)
                    {
                        firmwareRecord = firmware with { Tags = firmware.Tags.Add(EmuPatched) };
                    }
                    else
                    {
                        fail = true;
                        newBasename = Metadata.GetErrorName(this.fileSystem, file);
                        message = "Error while checking firmware: " + ((Error)firmwareTask).Message;
                        fixStatus = FixStatus.NotFixed;
                    }
                }

                if (!fail)
                {
                    // generate a new name for the file
                    newBasename = await this.GenerateUniqueName(target);
                }
            }

            if (!fail)
            {
                // now apply duration changes and firmware tag
                using var writer = this.fileSystem.File.Open(file, FileMode.Open, dryRun.FileAccess);
                var fragmentPath = this.fileSystem.Path.Combine(directory, newBasename + ".truncated_part");
                var durationResults = await this.FixDuration(fragmentPath, dryRun, writer);
                if (durationResults.Case is (Seq<string> patch, ulong oldSamples, ulong newSamples, long truncated))
                {
                    firmwareRecord = firmwareRecord! with { Tags = firmwareRecord.Tags.Concat(patch) };

                    // mark the file as having been processed
                    await dryRun.WouldDo(
                        $"update firmware tag with {EmuPatched}",
                        () => WriteFirmware(writer, firmwareRecord));

                    fixStatus = FixStatus.Fixed;
                    message = $"Partial file repaired. New name is {newBasename}. Samples count was {oldSamples}, new samples count is: {newSamples}. File truncated at {truncated}.";
                }
                else
                {
                    // fail: the file is corrupt in some other manner
                    fail = true;
                    newBasename = Metadata.GetErrorName(this.fileSystem, file);
                    fixStatus = FixStatus.NotFixed;
                    message = "Error while checking duration: " + ((Error)durationResults).Message;
                }
            }

            // and rename the file
            string newPath = this.fileUtilities.Rename(file, newBasename, dryRun);

            return new FixResult(fixStatus, affected, message, newPath);
        }

        private async Task<Fin<(Seq<string> Patch, ulong OldSamples, ulong NewSamples, long Truncated)>> FixDuration(string fragmentPath, DryRun dryRun, Stream stream)
        {
            // get sample counts so we can fix the sample duration if needed
            this.logger.LogDebug("Reading duration");
            var totalSamples = Flac.ReadTotalSamples(stream);

            // some files include desyncs (because they are partially written FLAC files) so we can't
            // use the shortcut form of CountSamplesAsync that just scans the end of the file.
            var frames = await Flac.FindFramesAsync(stream, fullScan: true);
            var countedSamples = frames.Bind(Flac.CalculateSampleCountFromFrameList);
            var isSamplesMismatched = from t in totalSamples
                                      from c in countedSamples
                                      select t != c;

            if (isSamplesMismatched.IsFail)
            {
                return (Error)isSamplesMismatched;
            }

            // first check if we have a case of FL010
            var fl010Affected = await this.metadataDurationBug.IsAffectedAsync(stream);
            var patchString = Seq<string>.Empty;
            long truncate;
            ulong newSamples;
            if (fl010Affected.Status is CheckStatus.Affected)
            {
                patchString = patchString.Add(MetadataDurationBug.EmuPatched);
            }

            // we assume there's trailing data in the file - even when there might not be.
            // Since our current frame parser does not tell us when the end of a frame is we just
            // step back one frame and truncate a little bit earlier.
            // one frame in a typical recording is 5 milliseconds. Not much in the scheme of things.
            var all = frames.ThrowIfFail();
            var last = all[^1];

            newSamples = Flac.CalculateSampleCountFromFrameList(new[] { all[^3], all[^2] }).ThrowIfFail();

            // truncate at the start of the last found frame.
            truncate = last.Offset;

            this.logger.LogDebug("Changing duration from {old} to {new}", totalSamples, newSamples);
            var success = dryRun.WouldDo(
                $"write total samples {newSamples}",
                () => Flac.WriteTotalSamples(stream, newSamples),
                () => Fin<Unit>.Succ(default));

            using var fragementStream = this.fileSystem.File.OpenWrite(fragmentPath);
            this.logger.LogDebug(
                "Truncating file from {old} to {new}, fragment written to {fragment}",
                stream.Length,
                truncate,
                fragmentPath);

            await this.fileUtilities.TruncateSplitAsync(stream, fragementStream, truncate, dryRun);

            return (patchString, totalSamples.ThrowIfFail(), newSamples, truncate);
        }

        private async ValueTask<string> GenerateUniqueName(TargetInformation target)
        {
            // get metadata from within the file so we can rename the file
            this.logger.LogDebug("Extracting metadata");
            var recording = new Recording();

            foreach (var extractor in this.metadataExtractors)
            {
                recording = await extractor.ProcessFileAsync(target, recording);
            }

            if (!recording.StartDate.HasValue)
            {
                return "unknown_date.flac";
            }

            const string Template = "{StartDate}_recovered{Extension}";
            var newBasename = this.filenameGenerator.Reconstruct(Template, recording).ThrowIfFail();

            // we've seen some cases where the partial data file exists because
            // of a clock update on the sensor. The sensor records a short period and then restarts the recording after
            // the clock update. So we get two files with date, and hence the same name,
            // which is the problem that triggered the partial data file in the first place.
            // However we use a unique name so in practice this should not be a problem.
            var path = this.fileSystem.Path.Combine(target.Base, newBasename);
            this.logger.LogTrace("Checking if {path} exists", path);
            if (this.fileSystem.File.Exists(path))
            {
                throw new NotSupportedException($"A unique name could not be determined for data file. Please report this case to the EMU reposiotiry. Problem path: {target.Path}");
            }

            this.logger.LogDebug("New filename: {name}", newBasename);
            return newBasename;
        }

        private record Data(bool IsEmpty, bool IsFlac, bool IsPreallocatedFile, Fin<FirmwareRecord> Firmware);
    }
}
