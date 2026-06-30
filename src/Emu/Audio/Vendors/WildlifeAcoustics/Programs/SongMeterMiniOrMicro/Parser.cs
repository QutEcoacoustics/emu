// <copyright file="Parser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using Emu.Models;
    using LanguageExt;
    using NodaTime;
    using static System.Buffers.Binary.BinaryPrimitives;
    using static Emu.Utilities.BinaryHelpers;
    using static NodaTime.Duration;
    using Duration = NodaTime.Duration;
    using Error = LanguageExt.Common.Error;

    /// <summary>
    /// See WildlifeAcoustics\schedule_structure.md for more information.
    /// This parser is for the Song Meter Mini and Song Meter Micro models.
    /// </summary>
    public static class Parser
    {
        public const int MagicNumberSize = 4;
        public const int ConfigSize = 128;
        public const int ScheduleSize = 128;
        public static readonly byte[] SmmsChunk = "SMMS"u8.ToArray();

        public static readonly byte[] ConfigurationIdentifier = new byte[] { 0x00, 0x00, 0x01, 0x01 };
        public static readonly byte[] ScheduleIdentifier = new byte[] { 0x00, 0x00, 0x00, 0x02 };
        public static readonly byte[] DeployedScheduleIdentifier = new byte[] { 0x00, 0x00, 0x01, 0x02 };

        private static readonly Error TooShort = Error.New(
            $"Error reading file: file is not long enough to have a {Encoding.ASCII.GetString(SmmsChunk)} header");

        private static readonly Error NotFound = Error.New(
            $"Could not find {Encoding.ASCII.GetString(SmmsChunk)} chunks");

        private static readonly Error IncorrectSize = Error.New(
            $"SMMS config was not {MagicNumberSize + ConfigSize + ScheduleSize} bytes.");

        public static async ValueTask<Fin<Program>> GetProgramFromConfigFileAsync(Stream stream)
        {
            if (stream.Length < MagicNumberSize)
            {
                return TooShort;
            }

            stream.Position = 0;
            var header = new byte[MagicNumberSize];
            var read = await stream.ReadAsync(header);

            if (read != MagicNumberSize)
            {
                return TooShort;
            }

            if (!header.SequenceEqual(SmmsChunk))
            {
                return NotFound;
            }

            if (stream.Length < MagicNumberSize + ConfigSize + ScheduleSize)
            {
                return IncorrectSize;
            }

            var payload = new byte[ConfigSize + ScheduleSize];
            read = await stream.ReadAsync(payload);
            if (read != ConfigSize + ScheduleSize)
            {
                return IncorrectSize;
            }

            // parse the configuration and schedule
            return from config in ParseConfiguration(payload.AsSpan(0, ConfigSize))
                   from schedule in ParseSchedule(payload.AsSpan(ConfigSize, ScheduleSize))
                   select new Program() { Configuration = config, Schedule = schedule };
        }

        public static Fin<Configuration> ParseConfiguration(ReadOnlySpan<byte> data)
        {
            if (data.Length != ConfigSize)
            {
                return Error.New($"Configuration data must be exactly {ConfigSize} bytes long.");
            }

            if (!data[..4].SequenceEqual(ConfigurationIdentifier))
            {
                return Error.New("Configuration data does not start with the correct identifier." +
                    $" Expected {BitConverter.ToString(ConfigurationIdentifier)} but got {BitConverter.ToString(data[..4].ToArray())}");
            }

            Debug.Assert(data[62..75].IsNull(), "Bytes between 62 and 75 should be null.");
            Debug.Assert(data[77..].IsNull(), "Bytes after 77 should be null.");

            return new Configuration()
            {
                IntendedModel = ReadUInt16BigEndian(data[4..6]) switch
                {
                    0x0101 => "Song Meter Mini Bat or Mini Bat 2",
                    0x0202 => "Song Meter Mini or Mini 2",
                    0x0402 => "Song Meter Micro or Micro 2",
                    _ => "Unknown." + Meta.CallToAction,
                },
                RecorderName = Encoding.ASCII.GetString(data[6..18]).TrimEnd('\0'),
                Timezone = ProgramParser.ReadOffsetUnsignedMinutes(data[18..22]),
                Position = ReadLocation(data[22..30]),
                SampleRateLeftChannel = ReadUInt32LittleEndian(data[30..34]),
                SampleRateRightChannel = ReadUInt32LittleEndian(data[34..38]),
                FullSpectrumSampleRate = ReadUInt32LittleEndian(data[38..42]),
                MaximumRecordingLength = Duration.FromSeconds(
                    ReadUInt32LittleEndian(data[42..46])),
                Channels = data[46] switch
                {
                    1 => Channel.Left,
                    2 => Channel.Right,
                    3 => Channel.Stereo,
                    _ => throw new InvalidOperationException(
                        $"Invalid channel configuration: {data[46]}. " + Meta.CallToAction),
                },
                Unknown47 = data[47],
                GainLeftDecibels = ReadGain(data[48]),
                GainRightDecibels = ReadGain(data[49]),
                RecordingFormat = (RecordingFormat)data[50],
                MinimumTriggerFrequency = data[51] * 1000,
                Unknown52 = data[52],
                Unknown53 = data[53],
                Unknown54 = data[54],
                Unknown55 = data[55],
                Unknown56 = data[56],
                SaveNoiseFiles = data[57] switch
                {
                    0 => true,
                    2 => false,
                    _ => throw new InvalidOperationException(
                        $"Invalid value for SaveNoiseFiles: {data[57]}. " + Meta.CallToAction),
                },
                TriggerWindow = FromSeconds(data[58] / 10.0),
                MaximumTriggerRecordingLength = FromSeconds(ReadUInt16LittleEndian(data[59..61]) / 10.0),
                Unknown61 = data[61],
                NonTriggeredRecording = data[75] switch
                {
                    0 => false,
                    1 => true,
                    _ => throw new InvalidOperationException(
                        $"Invalid value for NonTriggeredRecording: {data[75]}. " + Meta.CallToAction),
                },
                RecordingMode = (RecordingQualityMode)data[76],
            };
        }

        public static Fin<Schedule> ParseSchedule(ReadOnlySpan<byte> data)
        {
            if (data.Length != ScheduleSize)
            {
                return Error.New($"Schedule data must be exactly {ScheduleSize} bytes long.");
            }

            var isSchedule = data[..4].SequenceEqual(ScheduleIdentifier);
            var isDeployedSchedule = data[..4].SequenceEqual(DeployedScheduleIdentifier);
            if (!isSchedule && !isDeployedSchedule)
            {
                return Error.New("Schedule data does not start with the correct identifier." +
                    $" Expected {BitConverter.ToString(ScheduleIdentifier)} or {BitConverter.ToString(DeployedScheduleIdentifier)}" +
                    $" but got {BitConverter.ToString(data[..4].ToArray())}");
            }

            var scheduleCount = data[8];
            const int maxSchedules = 10;
            const int mainScheduleSectionStart = 18;
            const int mainScheduleEntrySize = 8;
            const int overflowScheduleSectionStart = 104;
            const int overflowScheduleEntrySize = 2;
            Range gapBetweenMainAndOverflow = new(98, 103);

            Debug.Assert(scheduleCount <= maxSchedules, "Found more than 10 schedules, which should be impossible.");

            Debug.Assert(
                data[9..mainScheduleSectionStart].IsNull(),
                "Bytes between schedule count and main schedule section should be null.");
            Debug.Assert(
                data[gapBetweenMainAndOverflow].IsNull(),
                "Bytes between main schedule section and overflow schedule section should be null.");
            Debug.Assert(
                data[(overflowScheduleSectionStart + (scheduleCount * overflowScheduleEntrySize))..].IsNull(),
                "Bytes after overflow schedule section should be null.");

            var entries = new ScheduleEntry[maxSchedules];
            for (int i = 0; i < maxSchedules; i++)
            {
                var main1 = mainScheduleSectionStart + (i * mainScheduleEntrySize);
                var main2 = main1 + 4;
                var overflow = overflowScheduleSectionStart + (i * overflowScheduleEntrySize);

                // I assumed this was a uint64 in the schedule structure document,
                // but now that I get to implementation it makes more sense to assume
                // it's two uint32s, for two reasons:
                // 1. There are no other 64-bit values in the program, but there are
                //    several 32-bit values.
                // 2. Uint32 can be more efficient for writing small bit ranges
                //    e.g. a 1 bit operation wastes 31 bits in a uint32 but wastes
                //    63 bits in a uint64.
                entries[i] = ParseScheduleEntry(
                    ReadUInt32LittleEndian(data[main1..]),
                    ReadUInt32LittleEndian(data[main2..]),
                    ReadUInt16LittleEndian(data[overflow..]),
                    shouldBeEmpty: i > scheduleCount - 1);
            }

            return new Schedule()
            {
                ScheduleFromRecording = isDeployedSchedule,
                DelayStart = ReadDelayStart(data[4..8]),
                SchedulesCount = scheduleCount,
                Entries = entries.Where(e => e != null).ToArr(),
                DefaultAlwaysOnSchedule = data[103],
            };
        }

        private static Location ReadLocation(ReadOnlySpan<byte> bytes) =>
            new()
            {
                Latitude = unchecked(ReadInt32LittleEndian(bytes)) / 1E5,
                LatitudePrecision = 5,

                // WA uses negative values for east longitudes
                Longitude = ReadInt32LittleEndian(bytes[4..]) / 1E5 * -1,
                LongitudePrecision = 5,
            };

        private static byte ReadGain(byte gain) => gain switch
        {
            0 => 0,
            1 => 6,
            2 => 12,
            3 => 18,
            4 => 24,
            _ => throw new InvalidOperationException($"Invalid gain value: {gain}. " + Meta.CallToAction),
        };

        private static LocalDateTime? ReadDelayStart(ReadOnlySpan<byte> bytes)
        {
            var value = ReadUInt32LittleEndian(bytes);

            return value == 0
                ? null
                : ProgramParser.WildlifeAcousticsEpoch.AtMidnight().PlusSeconds(value);
        }

        private static ScheduleEntry ParseScheduleEntry(uint main1, uint main2, ushort overflow, bool shouldBeEmpty)
        {
            const uint defaultMode = 1u << 31;
            if (shouldBeEmpty)
            {
                Debug.Assert(main1 == 0, "Expected empty schedule entry to have main section of zero.");

                Debug.Assert(
                    main2 == 0x00 || main2 == defaultMode,
                    "Expected empty schedule entry to have main section of 0x00.");
                Debug.Assert(overflow == 0, "Expected empty schedule entry to have overflow section of zero.");
                return null;
            }

            return new ScheduleEntry(main1, main2, overflow);
        }
    }
}
