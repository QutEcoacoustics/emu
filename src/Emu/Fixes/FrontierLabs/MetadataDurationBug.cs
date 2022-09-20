// <copyright file="MetadataDurationBug.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.FrontierLabs
{
    using System.Diagnostics;
    using System.IO.Abstractions;
    using Emu.Audio;
    using Emu.Utilities;
    using LanguageExt;
    using LanguageExt.Common;
    using Microsoft.Extensions.Logging;
    using static Emu.Audio.Vendors.FrontierLabs;
    using static Emu.Fixes.CheckStatus;
    using static LanguageExt.Prelude;

    public class MetadataDurationBug : IFixOperation
    {
        public static readonly string EmuPatched = WellKnownProblems.PatchString(Metadata.Problem);
        public static readonly (decimal Min, decimal Max) AffectedFirmwares = (3.17m, 3.28m);

        private readonly ILogger<MetadataDurationBug> logger;
        private readonly IFileSystem fileSystem;

        public MetadataDurationBug(ILogger<MetadataDurationBug> logger, IFileSystem fileSystem)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
        }

        public static OperationInfo Metadata => new(
            WellKnownProblems.FrontierLabsProblems.MetadataDurationBug,
            Fixable: true,
            Safe: true,
            Automatic: true,
            typeof(MetadataDurationBug));

        public async Task<CheckResult> CheckAffectedAsync(string file)
        {
            using var stream = (FileStream)this.fileSystem.File.OpenRead(file);

            return await this.IsAffectedAsync(stream);
        }

        public OperationInfo GetOperationInfo() => Metadata;

        public async Task<FixResult> ProcessFileAsync(string file, DryRun dryRun)
        {
            var affected = await this.CheckAffectedAsync(file);

            if (affected is { Status: Affected })
            {
                using var stream = (FileStream)this.fileSystem.File.Open(file, FileMode.Open, dryRun.FileAccess);
                return await this.FixDuration(stream, affected, dryRun);
            }
            else
            {
                return new FixResult(FixStatus.NoOperation, affected, affected.Message);
            }
        }

        internal async Task<CheckResult> IsAffectedAsync(Stream stream)
        {
            switch (Flac.IsFlacFile(stream).Case)
            {
                case Error e:
                    return new CheckResult(CheckStatus.Error, Severity.None, e.Message);
                case bool isFlac when isFlac == false:
                    return new CheckResult(Unaffected, Severity.None, "Audio recording is not a FLAC file");
            }

            var result = (await ReadFirmwareAsync(stream))
                .Bind(IsAffectedFirmwareVersion);

            var totalSamples = Flac.ReadTotalSamples(stream);
            var countedSamples = await Flac.CountSamplesAsync(stream);
            var isSamplesMismatched = IsAffected(totalSamples, countedSamples);

            if (result.IsSucc && isSamplesMismatched.Case is CheckStatus samplesDifferent)
            {
                var (firmware, firmwareStatus) = ((FirmwareRecord, CheckStatus))result;

                // alrighty: this mess is because we used to do a fairly crude frimware version check.
                // we then had to upgrade to actually counting samples but to maintain backwards compat
                // we still do both.
                var (status, severity, message) = (firmwareStatus, samplesDifferent) switch
                {
                    (_, Affected) => (Affected, Severity.Moderate, "File's duration is wrong"),
                    (Affected, _) => (Affected, Severity.Moderate, "File's duration is wrong"),
                    (Repaired, Unaffected) => (Repaired, Severity.None, "File has already had it's duration repaired"),
                    (_, Unaffected) => (Unaffected, Severity.None, "File not affected"),
                    (Unaffected, _) => (Unaffected, Severity.None, "File not affected"),
                    _ => throw new InvalidOperationException(),
                };

                var data = new MetadaDurationBugData(firmware, totalSamples.ThrowIfFail(), countedSamples.ThrowIfFail());
                return new CheckResult(status, severity, message, data);
            }

            return (result.IsFail ? (Error)result : (Error)isSamplesMismatched) switch
            {
                Error error when error == FirmwareNotFound => new CheckResult(Unaffected, Severity.None, error.Message),
                Error error => new CheckResult(CheckStatus.Error, Severity.None, error.Message),
            };
        }

        private static Fin<(FirmwareRecord Firmware, CheckStatus Status)> IsAffectedFirmwareVersion(FirmwareRecord record)
        {
            var version = record.Version;

            var affected = true switch
            {
                _ when version < AffectedFirmwares.Min => Unaffected,
                _ when version >= AffectedFirmwares.Max => Unaffected,
                _ when record.Tags.IsEmpty => Affected,
                _ when record.Tags.Contains(EmuPatched) => Repaired,
                _ => Affected,
            };

            return (record, affected);
        }

        private static Fin<CheckStatus> IsAffected(Fin<ulong> headerTotalSamples, Fin<ulong> countedSamples)
        {
            // only report true for cases where the difference is exactly double
            Fin<bool> result = from h in headerTotalSamples
                               from c in countedSamples
                               select h != c && (h / c == 2.0);

            return result.Case switch
            {
                Error e => e,
                true => Affected,
                false => Unaffected,
                _ => throw new InvalidOperationException(),
            };
        }

        private async Task<FixResult> FixDuration(FileStream stream, CheckResult check, DryRun dryRun)
        {
            var (firmware, totalSamples, countedSamples) = (MetadaDurationBugData)check.Data;

            var newDuration = totalSamples / 2UL;
            Debug.Assert(newDuration == countedSamples, "Halfing total samples should equal real count of samples");

            this.logger.LogDebug("Changing duration from {old} to {new}", totalSamples, countedSamples);

            var success = dryRun.WouldDo(
                $"write total samples {countedSamples}",
                () => Flac.WriteTotalSamples(stream, countedSamples),
                () => Fin<Unit>.Succ(default));

            if (success.IsFail)
            {
                return new FixResult(FixStatus.NotFixed, check, "Failed to write new total samples value. File is likely corrupt now.");
            }

            await dryRun.WouldDo(
                $"update firmware tag with {EmuPatched}",
                () => WriteFirmware(stream, firmware with { Tags = firmware.Tags.Add(EmuPatched) }));

            return new FixResult(FixStatus.Fixed, check, $"Old total samples was {totalSamples}, new total samples is: {countedSamples}");
        }

        public record MetadaDurationBugData(FirmwareRecord Firmware, ulong HeaderSamples, ulong CountedSamples);
    }
}
