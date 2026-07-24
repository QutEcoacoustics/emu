// <copyright file="GuanoExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata
{
    using Emu.Audio.Standards.GUANO;
    using Emu.Metadata.TitleyScientific;
    using Emu.Metadata.WildlifeAcoustics;
    using Emu.Models;
    using Emu.Models.Notices;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using Rationals;

    public class GuanoExtractor : IRawMetadataOperation
    {
        private readonly ILogger<GuanoExtractor> logger;

        public GuanoExtractor(ILogger<GuanoExtractor> logger)
        {
            this.logger = logger;
        }

        public string Name => "GUANO";

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.IsPcmWaveFile() && information.HasGuanoChunk();
            return ValueTask.FromResult(result);
        }

        public ValueTask<MetadataExtractionResult> ProcessFileAsync(TargetInformation information)
        {
            var tryGuano = GuanoParser.ReadGuanoBlock(information.FileStream);

            if (tryGuano.IsSucc)
            {
                (var guano, var notices) = tryGuano.ThrowIfFail();
                return ValueTask.FromResult<MetadataExtractionResult>(new(guano, notices.ToSeq()));
            }

            return ValueTask.FromResult<MetadataExtractionResult>(new(null));
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var tryGuano = GuanoParser.ReadGuanoBlock(information.FileStream);
            if (tryGuano.IsFail)
            {
                this.logger.LogWarning("Failed to extract GUANO metadata from {path}: {error}", information.Path, (LanguageExt.Common.Error)tryGuano);
                return ValueTask.FromResult(recording);
            }

            (var guano, var notices) = tryGuano.ThrowIfFail();

            // we prioritize the GUANO metadata over the existing recording metadata
            // on the assumption it is more accurate (since it is the newest metadata standard)
            OffsetDateTime? guanoTimestampOffset = null;
            LocalDateTime? guanoTimestampLocal = null;
            var parsedTimestamp = guano.Timestamp;
            if (parsedTimestamp.IsSucc)
            {
                parsedTimestamp.ThrowIfFail().Match(
                    Left: local =>
                    {
                        guanoTimestampLocal = local;
                        return 0;
                    },
                    Right: offset =>
                    {
                        guanoTimestampOffset = offset;
                        guanoTimestampLocal = offset.LocalDateTime;
                        return 0;
                    });
            }
            else if (!string.IsNullOrWhiteSpace(guano.GetValue("Timestamp")))
            {
                notices.Add(new Warning(((LanguageExt.Common.Error)parsedTimestamp).Message));
            }

            var startDate = guanoTimestampOffset ?? recording.StartDate;
            var localStart = guanoTimestampLocal ?? recording.LocalStartDate;
            var make = NormalizeVendorMake(guano.Make) ?? recording.Sensor?.Make;

            var sensor = (recording.Sensor ?? new Sensor()) with
            {
                Make = make,
                Model = guano.Model ?? recording.Sensor?.Model,
                Firmware = guano.FirmwareVersion ?? recording.Sensor?.Firmware,
                SerialNumber = guano.Serial ?? recording.Sensor?.SerialNumber,
                Temperature = guano.TemperatureInt ?? recording.Sensor?.Temperature,
                TemperatureExternal = guano.TemperatureExt ?? recording.Sensor?.TemperatureExternal,
            };

            var location = guano.Location ?? recording.Location;

            var otherFields = new Dictionary<string, string>(recording.OtherFields ?? new Dictionary<string, string>(StringComparer.Ordinal));
            foreach (var entry in guano.VendorEntries)
            {
                otherFields[$"(GUANO) {entry.Key.FullKey}"] = entry.Value;
            }

            // we prioritize the opposite for duration and sample rate: our existing methods which gets the data from the wave header are more
            // accurate than the GUANO metadata, which is often rounded to a lower precision.
            var duration = recording.DurationSeconds ?? (guano.Length is not null ? Rational.Approximate((decimal)guano.Length.Value, 6) : null);
            var sampleRate = recording.SampleRateHertz ?? guano.Samplerate;

            var modified = recording with
            {
                StartDate = startDate,
                TrueStartDate = guanoTimestampOffset ?? recording.TrueStartDate ?? startDate,
                LocalStartDate = localStart,
                DurationSeconds = duration,
                SampleRateHertz = sampleRate,
                Sensor = sensor,
                Location = location,
                OtherFields = otherFields,
                Notices = recording.Notices.Concat(notices),
            };

            return ValueTask.FromResult(this.ApplyVendorMetadata(guano, modified, information.Path));
        }

        private static string NormalizeVendorMake(string make)
        {
            return string.Equals(make, WildlifeAcoustics.Guano.AlternateMake, StringComparison.OrdinalIgnoreCase)
                ? WildlifeAcoustics.Guano.Make
                : make;
        }

        private Recording ApplyVendorMetadata(GuanoBlock guano, Recording recording, string path)
        {
            switch (guano.PrimaryVendorNamespace)
            {
                case null:
                    return recording;
                case WildlifeAcoustics.Guano.Namespace:
                    return WildlifeAcoustics.Guano.ApplyVendorEntries(guano, recording);
                case TitleyScientific.Guano.Namespace:
                    return TitleyScientific.Guano.ApplyVendorEntries(guano, recording);
                case FrontierLabs.Guano.Namespace:
                    return FrontierLabs.Guano.ApplyVendorEntries(guano, recording);
                default:
                    this.logger.LogWarning(
                        "Found unknown GUANO vendor namespace {namespace} in {path}. {callToAction}",
                        guano.PrimaryVendorNamespace,
                        path,
                        Meta.CallToAction);
                    return recording;
            }
        }
    }
}
