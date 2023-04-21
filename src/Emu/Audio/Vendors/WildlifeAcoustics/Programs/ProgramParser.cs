// <copyright file="ProgramParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs
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
    using UnitsNet.NumberExtensions.NumberToEnergy;
    using static System.Buffers.Binary.BinaryPrimitives;
    using static Emu.Utilities.BinaryHelpers;
    using static SongMeter4Program;
    using Duration = NodaTime.Duration;
    using Error = LanguageExt.Common.Error;

    /// <summary>
    /// See WildlifeAcoustics\schedule_structure.md for more information.
    /// </summary>
    public static class ProgramParser
    {
        public const int LargestProgram = 1200;
        public static readonly LocalDate WildlifeAcousticsEpoch = new(2000, 1, 1);

        public static readonly byte[] Sm4pChunkId = "SM4P"u8.ToArray();
        public static readonly byte[] Sm3pChunkId = "SM3P"u8.ToArray();

        private static readonly Error TooShort = Error.New(
            $"Error reading file: file is not long enough to have a {Encoding.ASCII.GetString(Sm4pChunkId)} or {Encoding.ASCII.GetString(Sm3pChunkId)} header");

        private static readonly Error NotFound = Error.New(
            $"Could not find {Encoding.ASCII.GetString(Sm4pChunkId)} or {Encoding.ASCII.GetString(Sm3pChunkId)} chunks");

        private static readonly Error IncorrectSize = Error.New(
            "Schedule was not either 1124 bytes (SM4) or 500 bytes (SM3).");

        private static readonly Error NotSM3P = Error.New(
            "Schedule does not have a SM3P chunk.");

        private static readonly Error NotSM4P = Error.New(
            "Schedule does not have a SM4P chunk.");

        /// <summary>
        /// Reads a SongMeter settings file (common extension: .SM4S or .PGM) and returns
        /// a range of bytes if the file contains the SM4P or SM3P chunk.
        /// </summary>
        /// <remarks>
        /// Currently only tested for SM4 and SM3 settings.
        /// </remarks>
        /// <param name="stream">The file stream.</param>
        /// <returns>Range representing the position of the sm4p chunk.</returns>
        public static async ValueTask<Fin<SongMeterProgram>> GetProgramFromScheduleFileAsync(Stream stream)
        {
            if (stream.Length < 4)
            {
                return TooShort;
            }

            stream.Position = 0;
            var buffer = new byte[4];
            var offset = stream.Read(buffer);

            if (offset != 4)
            {
                return TooShort;
            }

            if (buffer.SequenceEqual(Sm3pChunkId) || buffer.SequenceEqual(Sm4pChunkId))
            {
                // the chunk is assumed to be the entire file.
                var fullBuffer = new byte[stream.Length];
                stream.Position = 0;
                await stream.ReadAsync(fullBuffer);

                return Parse(fullBuffer);
            }

            return NotFound;
        }

        /// <summary>
        /// Extracts metadata from a songmeter program.
        /// Works either for a SM3 or SM4 program.
        /// </summary>
        /// <returns>Wamd metadata.</returns>
        public static Fin<SongMeterProgram> Parse(ReadOnlySpan<byte> bytes)
        {
            const int SM4Size = 1124;
            const int SM3Size = 500;

            return bytes.Length switch
            {
                SM4Size => ParseSM4Program(bytes),
                SM3Size => ParseSM3Program(bytes),
                _ => IncorrectSize,
            };
        }

        private static Fin<SongMeterProgram> ParseSM3Program(ReadOnlySpan<byte> bytes)
        {
            if (!bytes[0..4].SequenceEqual(Sm3pChunkId))
            {
                return NotSM3P;
            }

            //var firstSchedule = ReadUInt16LittleEndian(bytes[4..]);
            var lastSchedule = ReadUInt16LittleEndian(bytes[8..]);

            var programsSection = bytes.Slice(16, 396);
            var programs = ParseAdvancecdSchedule(programsSection);
            Debug.Assert(programs.Length == lastSchedule, "last program index should match number of programs");

            var unknown488 = bytes[488];
            var model = ReadUInt16LittleEndian(bytes[472..]) switch
            {
                0 => Models.SM3,
                1 when unknown488 == 0x40 => Models.SM3M,
                1 when unknown488 == 0x48 => Models.SM3BAT,
                _ => throw new NotSupportedException("Unable to determine model"),
            };

            return new SongMeter3Program()
            {
                AdvancedSchedule = programs,
                Prefix = Encoding.Unicode.GetString(bytes.Slice(412, 24)).TrimEnd('\0'),
                PrefixEnabled = ReadBool16LittleEndian(bytes[444..]),
                Timezone = ReadOffset(bytes[446..]),
                TimezoneEnabled = ReadBool16LittleEndian(bytes[450..]),
                Position = ReadLocation(bytes[452..]),
                PositionEnabled = ReadBool16LittleEndian(bytes[456..]),
                SolarMode = (SolarMode)ReadUInt16LittleEndian(bytes[458..]),
                SolarModeEnabled = ReadBool16LittleEndian(bytes[460..]),
                BatteryCutoffVoltage = ReadCutOffVoltage(bytes[462..]),
                BatteryCutoffVoltageEnabled = ReadBool16LittleEndian(bytes[464..]),
                SensitivityLeft = ReadSensitivity(bytes[466..]),
                SensitivityRight = ReadSensitivity(bytes[468..]),
                SensitivityEnabled = ReadBool16LittleEndian(bytes[470..]),
                Model = model,
                ScenarioMemoryCardA = ReadMemoryCardSize(bytes[474..]),
                ScenarioMemoryCardB = ReadMemoryCardSize(bytes[476..]),
                ScenarioMemoryCardC = ReadMemoryCardSize(bytes[478..]),
                ScenarioMemoryCardD = ReadMemoryCardSize(bytes[480..]),
                ScenarioMicrophone0 = (SongMeterMicrophone)ReadUInt16LittleEndian(bytes[482..]),
                ScenarioMicrophone1 = (SongMeterMicrophone)ReadUInt16LittleEndian(bytes[484..]),
                ScenarioTriggerRatio = ReadTriggerRatio(bytes[486..]),
                ScenarioBatteryEnergy = ReadBatteryEnergy(bytes[488..]),
                ScenarioStart = ReadDelayStart(bytes[492..]),
                Unknown498 = ReadUInt16LittleEndian(bytes[482..]),
            };
        }

        private static Fin<SongMeterProgram> ParseSM4Program(ReadOnlySpan<byte> bytes)
        {
            if (!bytes[0..4].SequenceEqual(Sm4pChunkId))
            {
                return NotSM4P;
            }

            var firstSchedule = ReadUInt16LittleEndian(bytes[4..]);
            var lastSchedule = ReadUInt16LittleEndian(bytes[8..]);

            var scheulesSection = bytes.Slice(20, 80);
            var scheules = ParseSimpleSchedule(scheulesSection, firstSchedule, lastSchedule);

            var advancedSchedulesSection = bytes.Slice(728, 396);
            var advancedSchedules = ParseAdvancecdSchedule(advancedSchedulesSection);

            var preAmpFlags = ReadUInt16LittleEndian(bytes[558..]);
            var model = ReadUInt16LittleEndian(bytes[604..]) switch
            {
                0 => Models.SM4,
                1 => Models.SM4BATFS,
                2 => Models.SM4BATZC,
                _ => throw new NotSupportedException("Unable to determine model"),
            };

            return new SongMeter4Program()
            {
                AdvancedSchedule = advancedSchedules,
                Schedule = scheules,
                Bitmap1 = ParseBitmap(bytes[100..280]),
                Bitmap2 = ParseBitmap(bytes[280..460]),
                Prefix = Encoding.Unicode.GetString(bytes.Slice(460, 24)).TrimEnd('\0'),
                PrefixEnabled = ReadBool16LittleEndian(bytes[492..]),
                Timezone = ReadOffset(bytes[494..]),
                TimezoneEnabled = ReadBool16LittleEndian(bytes[498..]),
                Position = ReadLocation(bytes[500..], bytes[644..]),
                PositionEnabled = ReadBool16LittleEndian(bytes[504..]),
                SolarMode = (SolarMode)ReadUInt16LittleEndian(bytes[506..]),
                DelayStart = ReadDelayStart(bytes[512..]).Date,
                DelayStartEnabled = ReadBool16LittleEndian(bytes[516..]),
                BatteryCutoffVoltage = ReadCutOffVoltage(bytes[524..]),
                BatteryCutoffVoltageEnabled = ReadBool16LittleEndian(bytes[526..]),
                LedSettings = (LedSettings)ReadUInt16LittleEndian(bytes[528..]),
                SensitivityLeft = ReadSensitivity(bytes[532..]),
                SensitivityRight = ReadSensitivity(bytes[534..]),
                SensitivityEnabled = ReadBool16LittleEndian(bytes[536..]),
                TriggerWindow = bytes[539] switch
                {
                    0xFF => -1,
                    byte b => TriggerWindow.Convert(b),
                },
                Channels = (Channel)ReadUInt16LittleEndian(bytes[540..]),
                GainLeft = ReadUInt16LittleEndian(bytes[544..]) / 2.0f,
                GainRight = ReadUInt16LittleEndian(bytes[546..]) / 2.0f,
                HighPassFilterLeft = ReadHighPassFilter(bytes[548..], model),
                HighPassFilterRight = ReadHighPassFilter(bytes[550..], model),
                SampleRate = ReadTwoUInt16LittleEndianAsOneUInt32(bytes[552..]),
                DivisionRatio = ReadUInt16LittleEndian(bytes[556..]) switch
                {
                    0 => 8,
                    1 => 16,
                    ushort u => throw new NotSupportedException($"Division ratio {u} was unexpected"),
                },
                PreampLeft = (preAmpFlags & PreampLeftFlag) == PreampLeftFlag ? Preamp.On26dB : Preamp.Off,
                PreampRight = (preAmpFlags & PreampRightFlag) == PreampRightFlag ? Preamp.On26dB : Preamp.Off,
                MinDuration = DurationMinimum.Convert(ReadUInt16LittleEndian(bytes[560..])),
                MaxDuration = ReadUInt16LittleEndian(bytes[564..]) / 1000.0f,
                MinTriggerFrequency = ReadUInt16LittleEndian(bytes[570..]) * 1000u,
                Unknown574 = ReadUInt16LittleEndian(bytes[574..]),
                Unknown578 = ReadUInt16LittleEndian(bytes[578..]),
                Unknown582 = ReadUInt16LittleEndian(bytes[582..]),
                TriggerLevel = ReadInt16LittleEndian(bytes[584..]),
                Unknown586 = ReadUInt16LittleEndian(bytes[586..]),
                MaxTriggerTime = ReadUInt16LittleEndian(bytes[592..]),
                MaxLength = Duration.FromMinutes(ReadUInt16LittleEndian(bytes[596..])),
                Compression = (Compression)ReadUInt16LittleEndian(bytes[600..]),
                Model = model,
                ScenarioMemoryCardA = ReadMemoryCardSize(bytes[608..]),
                ScenarioMemoryCardB = ReadMemoryCardSize(bytes[610..]),
                ScenarioMicrophone0 = (SongMeterMicrophone)ReadUInt16LittleEndian(bytes[612..]),
                ScenarioMicrophone1 = (SongMeterMicrophone)ReadUInt16LittleEndian(bytes[614..]),
                ScenarioTriggerRatio = ReadTriggerRatio(bytes[616..]),
                ScenarioBatteryEnergy = ReadBatteryEnergy(bytes[618..]),
                ScenarioStart = ReadDelayStart(bytes[620..]),
                ScheduleMode = (ScheduleMode)ReadUInt16LittleEndian(bytes[640..]),
            };
        }

        private static Offset ReadOffset(ReadOnlySpan<byte> bytes) =>
            Offset.FromHoursAndMinutes(
                ReadInt16LittleEndian(bytes),
                ReadInt16LittleEndian(bytes[2..]));

        private static LocalDateTime ReadDelayStart(ReadOnlySpan<byte> bytes) =>
            WildlifeAcousticsEpoch
                .AtMidnight()
                .PlusSeconds(
                    ((uint)ReadUInt16LittleEndian(bytes)) << 16 | ReadUInt16LittleEndian(bytes[2..]));

        private static Location ReadLocation(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> highPrecisionBytes = default) =>
            (highPrecisionBytes.Length >= 8) switch
            {
                true => new()
                {
                    Latitude = unchecked((int)ReadTwoUInt16LittleEndianAsOneUInt32(highPrecisionBytes)) / 100_000.0,
                    LatitudePrecision = 5,
                    Longitude = (unchecked((int)ReadTwoUInt16LittleEndianAsOneUInt32(highPrecisionBytes[4..])) / 100_000.0) * -1,
                    LongitudePrecision = 5,
                },
                false => new()
                {
                    Latitude = ReadInt16LittleEndian(bytes) / 100.0,
                    LatitudePrecision = 2,
                    Longitude = (ReadInt16LittleEndian(bytes[2..]) / 100.0) * -1,
                    LongitudePrecision = 2,
                },
            };

        private static float ReadCutOffVoltage(ReadOnlySpan<byte> bytes) =>
            ReadUInt16LittleEndian(bytes) / 10.0f;

        private static float ReadSensitivity(ReadOnlySpan<byte> bytes) =>
            ReadInt16LittleEndian(bytes) switch
            {
                unchecked((short)0xFFFF) => -1,
                short s => s / 10.0f,
            };

        private static HighPassFilter ReadHighPassFilter(ReadOnlySpan<byte> bytes, string model) =>
            ReadUInt16LittleEndian(bytes) switch
            {
                1 when model == Models.SM4BATFS => HighPassFilter.On16000Hz,
                1 when model == Models.SM4BATZC => HighPassFilter.On16000Hz,
                ushort u => (HighPassFilter)u,
            };

        private static ulong ReadMemoryCardSize(ReadOnlySpan<byte> bytes)
        {
            var value = ReadUInt16LittleEndian(bytes);
            return value == 0 ? 0 : (1ul << value) * (ulong)1E9;
        }

        private static float ReadTriggerRatio(ReadOnlySpan<byte> bytes)
        {
            return ReadUInt16LittleEndian(bytes) switch
            {
                0xFFFF => -1f,
                ushort u => u / 100f,
            };
        }

        private static double ReadBatteryEnergy(ReadOnlySpan<byte> bytes)
        {
            return ReadUInt16LittleEndian(bytes) switch
            {
                0xFFFF => -1f,
                ushort u => u.WattHours().Joules,
            };
        }

        private static Arr<AdvancedScheduleEntry> ParseAdvancecdSchedule(ReadOnlySpan<byte> bytes)
        {
            var result = new List<AdvancedScheduleEntry>(100);
            for (var i = 0; i <= bytes.Length; i += 4)
            {
                // the programs are encoded as two consecutive 2-byte little endian numbers
                // so the byte order is really weird:
                // bytes in file: b0 b1 b2 b3
                // decoding order (highest to lowest): b1 b0 b3 b2
                uint entry = ReadTwoUInt16LittleEndianAsOneUInt32(bytes[i..]);

                // when a program is 0 it's just 4 empty bytes, so can stop parsing it
                if (entry == 0)
                {
                    break;
                }

                result.Add(AdvancedScheduleEntry.Create(entry));
            }

            return result.ToArr();
        }

        private static Arr<SimpleScheduleEntry> ParseSimpleSchedule(ReadOnlySpan<byte> bytes, uint first, uint last)
        {
            if (bytes.Length != 80)
            {
                throw new ArgumentException("Simple schedule block not 80 bytes", nameof(bytes));
            }

            var max = last - first + 1;

            // 8 null bytes is still a valid schedule!
            // so use the first and last indexes to only pull out valid schedules
            var result = new List<SimpleScheduleEntry>(10);
            for (int i = 0, count = 0; count < max; i += 8, count++)
            {
                // the programs are encoded as two consecutive 2-byte little endian numbers
                // so the byte order is really weird:
                // bytes in file: b0 b1 b2 b3 b4 b5 b6 b7 b8
                // decoding order (highest to lowest): b1 b0 b3 b2 b5 b4 b7 b6
                ulong entry = ReadFourUInt16LittleEndianAsOneUInt64(bytes[i..]);

                result.Add(new SimpleScheduleEntry(entry));
            }

            return result.ToArr();
        }

        private static Arr<Range> ParseBitmap(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != 180)
            {
                throw new ArgumentException("Bitmap block not 180 bytes", nameof(bytes));
            }

            var result = Arr<Range>.Empty;

            // read each byte as in the bitmap, creating ranges
            // of where bits are 1 (on)
            var lastStart = -1;
            var inRange = false;
            for (var i = 0; i < bytes.Length; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    var isOne = ((bytes[i] >> j) & 1) == 1;
                    if (!inRange && isOne)
                    {
                        inRange = true;
                        lastStart = ConvertIndex(i, j);
                    }
                    else if (inRange && !isOne)
                    {
                        // convert minute indices into seconds to adhere to our units
                        // of measure policy
                        result = result.Add(
                            new Range(
                                ToSeconds(lastStart),
                                ToSeconds(ConvertIndex(i, j))));

                        inRange = false;
                        lastStart = -1;
                    }

                    // in the other two cases, just continue because we're in a run
                    // of consecutive values
                }
            }

            // edge case... we're still in a range at the end of the bitmap
            if (inRange)
            {
                result = result.Add(new Range(ToSeconds(lastStart), ToSeconds(1440)));
            }

            return result;

            static int ConvertIndex(int i, int j) => (i * 8) + j;
            static int ToSeconds(int i) => i * 60;
        }
    }
}
