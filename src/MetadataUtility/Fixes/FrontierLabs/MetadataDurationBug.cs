// <copyright file="MetadataDurationBug.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Fixes.FrontierLabs
{
    using System.IO.Abstractions;
    using LanguageExt;
    using LanguageExt.Common;
    using MetadataUtility.Audio;
    using MetadataUtility.Utilities;
    using Microsoft.Extensions.Logging;
    using static MetadataUtility.Audio.Vendors.FrontierLabs;

    //public record DurationBugRecord(FirmwareRecord Firmware, DurationBugStatus Status);

    public class MetadataDurationBug : IFixOperation
    {
        public static readonly string EmuPatched = WellKnownProblems.PatchString(Metadata.Problem);
        public static readonly (decimal Min, decimal Max) AffectedFirmwares = (3.17m, 3.28m);

        private readonly ILogger<MetadataDurationBug> logger;
        private readonly FileUtilities fileUtils;
        private readonly IFileSystem fileSystem;

        public MetadataDurationBug(ILogger<MetadataDurationBug> logger, FileUtilities fileUtils)
        {
            this.logger = logger;
            this.fileUtils = fileUtils;
            this.fileSystem = new FileSystem();
        }

        public static OperationInfo Metadata => new(
            WellKnownProblems.FrontierLabs.MetadataDurationBug,
            Fixable: true,
            Safe: true,
            Automatic: true,
            typeof(MetadataDurationBug));

        public async Task<CheckResult> CheckAffectedAsync(string file)
        {
            using var stream = (FileStream)this.fileSystem.File.OpenRead(file);

            return await this.IsAffected(stream);
        }

        public OperationInfo GetOperationInfo() => Metadata;

        public async Task<FixResult> ProcessFileAsync(string file, DryRun dryRun, bool backup)
        {
            var affected = await this.CheckAffectedAsync(file);

            if (affected is { Status: CheckStatus.Affected })
            {
                if (backup)
                {
                    var dest = await this.fileUtils.BackupAsync(file, dryRun);
                    this.logger.LogDebug("File backed up to {destination}", dest);
                }

                using var stream = (FileStream)this.fileSystem.File.Open(file, FileMode.Open, dryRun.FileAccess);
                return await this.FixDuration(stream, affected, dryRun);
            }
            else
            {
                return new FixResult(FixStatus.NoOperation, affected, affected.Message);
            }
        }

        private static Fin<(FirmwareRecord Firmware, CheckStatus Status)> IsAffectedFirmwareVersion(FirmwareRecord record)
        {
            var version = record.Version;

            var affected = true switch
            {
                _ when version < AffectedFirmwares.Min => CheckStatus.Unaffected,
                _ when version >= AffectedFirmwares.Max => CheckStatus.Unaffected,
                _ when record.Tags is null => CheckStatus.Affected,
                _ when record.Tags.Contains(EmuPatched) => CheckStatus.Repaired,
                _ => CheckStatus.Affected,
            };

            return (record, affected);
        }

        private async Task<CheckResult> IsAffected(FileStream stream)
        {
            switch (Flac.IsFlacFile(stream).Case)
            {
                case Error error:
                    return new CheckResult(CheckStatus.Error, Severity.None, error.Message);
                case bool isFlac when isFlac == false:
                    return new CheckResult(CheckStatus.Unaffected, Severity.None, "Audio recording is not a FLAC file");
            }

            var result = (await ReadFirmwareAsync(stream))
                .Bind(IsAffectedFirmwareVersion);

            if (result.IsSucc)
            {
                var (firmware, status) = ((FirmwareRecord, CheckStatus))result;
                var (severity, message) = status switch
                {
                    CheckStatus.Affected => (Severity.Moderate, "File's duration is wrong"),
                    CheckStatus.Unaffected => (Severity.None, "File not affected"),
                    CheckStatus.Repaired => (Severity.None, "File has already had it's duration repaired"),
                    _ => throw new InvalidOperationException(),
                };

                return new CheckResult(status, severity, message, firmware);
            }

            return new CheckResult(CheckStatus.Error, Severity.None, ((Error)result).Message);
        }

        private async Task<FixResult> FixDuration(FileStream stream, CheckResult check, DryRun dryRun)
        {
            var firmware = (FirmwareRecord)check.Data;
            var duration = Flac.ReadTotalSamples(stream);
            if (duration.IsFail)
            {
                return new FixResult(FixStatus.NotFixed, check, "Could not read total samples");
            }

            var old = (ulong)duration;
            var newDuration = old / 2UL;

            this.logger.LogDebug("Changing duration from {old} to {new}", old, newDuration);

            var success = dryRun.WouldDo(
                $"write total samples {newDuration}",
                () => Flac.WriteTotalSamples(stream, newDuration),
                () => Fin<Unit>.Succ(default));

            if (success.IsFail)
            {
                return new FixResult(FixStatus.NotFixed, check, "Failed to write new total samples value. File is likely corrupt now.");
            }

            await dryRun.WouldDo(
                $"update firmware tag with {EmuPatched}",
                () => WriteFirmware(stream, firmware, EmuPatched));

            return new FixResult(FixStatus.Fixed, check, $"Old total samples was {old}, new total samples is: {newDuration}");
        }
    }
}
