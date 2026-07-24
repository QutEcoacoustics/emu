// <copyright file="Guano.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.FrontierLabs
{
    using System.Globalization;
    using Emu.Audio.Standards.GUANO;
    using Emu.Models;
    using NodaTime;
    using NodaTime.Text;

    public static class Guano
    {
        public const string Namespace = "FLABS";

        public static readonly GuanoKey BatteryLevelKey = new(Namespace, "BatteryLevel");
        public static readonly GuanoKey EndKey = new(Namespace, "End");
        public static readonly GuanoKey LastTimeSyncKey = new(Namespace, "LastTimeSync");
        public static readonly GuanoKey SdCardFreeKey = new(Namespace, "SdCardFree");
        public static readonly GuanoKey SdCardSizeKey = new(Namespace, "SdCardSize");
        public static readonly GuanoKey SdCardSlotKey = new(Namespace, "SdCardSlot");
        public static readonly GuanoKey StartKey = new(Namespace, "Start");

        private static readonly MicrophoneGuanoKeys[] MicrophoneKeys =
        [
            new(
                new GuanoKey(Namespace, "MicrophoneType1"),
                new GuanoKey(Namespace, "MicrophoneSerial1"),
                new GuanoKey(Namespace, "MicrophoneBuildDate1"),
                new GuanoKey(Namespace, "GainCh1")),
            new(
                new GuanoKey(Namespace, "MicrophoneType2"),
                new GuanoKey(Namespace, "MicrophoneSerial2"),
                new GuanoKey(Namespace, "MicrophoneBuildDate2"),
                new GuanoKey(Namespace, "GainCh2")),
        ];

        public static Recording ApplyVendorEntries(GuanoBlock guano, Recording recording)
        {
            var sensor = recording.Sensor ?? new Sensor();
            var microphones = ApplyMicrophones(guano, sensor.Microphones, recording.Channels);

            return recording with
            {
                TrueStartDate = ParseOffsetDateTime(guano.GetValue(StartKey)) ?? recording.TrueStartDate,
                TrueEndDate = ParseOffsetDateTime(guano.GetValue(EndKey)) ?? recording.TrueEndDate,
                Location = ApplyLocation(guano, recording.Location),
                MemoryCard = ApplyMemoryCard(guano, recording.MemoryCard),
                Sensor = sensor with
                {
                    BatteryLevel = ParsePercentage(guano.GetValue(BatteryLevelKey)) ?? sensor.BatteryLevel,
                    LastTimeSync = ParseOffsetDateTime(guano.GetValue(LastTimeSyncKey)) ?? sensor.LastTimeSync,
                    Microphones = microphones,
                },
            };
        }

        private static MemoryCard ApplyMemoryCard(GuanoBlock guano, MemoryCard memoryCard)
        {
            var capacity = ParseMegabytes(guano.GetValue(SdCardSizeKey));
            var remaining = ParseMegabytes(guano.GetValue(SdCardFreeKey));
            var slot = ParseByte(guano.GetValue(SdCardSlotKey));
            if (capacity is null && remaining is null && slot is null)
            {
                return memoryCard;
            }

            return (memoryCard ?? new MemoryCard()) with
            {
                Capacity = memoryCard?.Capacity ?? capacity,
                Remaining = memoryCard?.Remaining ?? remaining,
                Slot = memoryCard?.Slot ?? slot,
            };
        }

        private static Location ApplyLocation(GuanoBlock guano, Location location)
        {
            if (location is not null || !Location.TryParse(guano.LocPosition, out var parsed))
            {
                return location;
            }

            return parsed with
            {
                HorizontalAccuracy = guano.LocAccuracy,
                CoordinateReferenceSystem = parsed.CoordinateReferenceSystem ?? "WGS84",
            };
        }

        private static Microphone[] ApplyMicrophones(GuanoBlock guano, Microphone[] existing, ushort? channelCount)
        {
            var microphones = new List<Microphone>();

            for (var index = 0; index < MicrophoneKeys.Length; index++)
            {
                var keys = MicrophoneKeys[index];
                var type = guano.GetValue(keys.Type);
                var serial = guano.GetValue(keys.Serial);
                var buildDate = ParseLocalDate(guano.GetValue(keys.BuildDate));
                var gain = ParseDouble(guano.GetValue(keys.Gain));
                var current = existing?.ElementAtOrDefault(index);

                if (channelCount is not null && index >= channelCount.Value && current is null)
                {
                    continue;
                }

                if (type is null && serial is null && buildDate is null && gain is null && current is null)
                {
                    continue;
                }
                current ??= new Microphone();
                microphones.Add(current with
                {
                    Type = type ?? current.Type,
                    UID = serial ?? current.UID,
                    BuildDate = buildDate ?? current.BuildDate,
                    Gain = gain ?? current.Gain,
                    Channel = current.Channel ?? index,
                    ChannelName = current.ChannelName ?? ((char)('A' + index)).ToString(),
                });
            }

            return microphones.Count > 0 ? microphones.ToArray() : existing;
        }

        private static OffsetDateTime? ParseOffsetDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var parsed = OffsetDateTimePattern.Rfc3339.Parse(value);
            return parsed.Success ? parsed.Value : null;
        }

        private static LocalDate? ParseLocalDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var parsed = LocalDatePattern.Iso.Parse(value);
            return parsed.Success ? parsed.Value : null;
        }

        private static double? ParseDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private static double? ParsePercentage(string value)
        {
            var percentage = ParseDouble(value);
            return percentage / 100;
        }

        private static byte? ParseByte(string value)
        {
            return byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private static ulong? ParseMegabytes(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            // `d` is not GUANO numeric syntax. It is likely a decimal-radix marker or a firmware
            // formatting artifact; the field defines the storage unit, so the suffix carries no magnitude.
            // this oddity was observed in test/Fixtures/FL_BAR_LT/_GuanoSample/33901_BigHarbourIslandA1_20250710T130000-0300.wav
            var numeric = value.EndsWith('d') ? value[..^1] : value;
            return ulong.TryParse(numeric, NumberStyles.Integer, CultureInfo.InvariantCulture, out var megabytes)
                ? checked(megabytes * MemoryCard.MegabyteConversion)
                : null;
        }

        private sealed record MicrophoneGuanoKeys(GuanoKey Type, GuanoKey Serial, GuanoKey BuildDate, GuanoKey Gain);
    }
}
