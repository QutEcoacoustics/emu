// <copyright file="ProgramParserTests.Data.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Vendors.WildlifeAcoustics.Programs;

using System.Collections.Generic;
using System.Linq;
using Emu.Audio.Vendors.WildlifeAcoustics;
using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
using Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes;
using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
using Emu.Models;
using LanguageExt;
using NodaTime;
using UnitsNet.NumberExtensions.NumberToEnergy;
using UnitsNet.NumberExtensions.NumberToInformation;
using static Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums.EventType;
using static Emu.Extensions.NodaTime.Helpers;
using static LanguageExt.Prelude;
using static NodaTime.Duration;
using static NodaTime.LocalTime;

public partial class ProgramParserTests
{
    public const string FnqSchedule = "WA_SM4/2.1.1_HighPrecisionDateStamp/SONGMETR.SM4S";

    private static Dictionary<string, SongMeterProgram> programs = null;

    public static Dictionary<string, SongMeterProgram> Programs => programs ??= MakePrograms();

    public static IEnumerable<object[]> SM4Programs() => Filter<SongMeter4Program>("SM4S");

    public static IEnumerable<object[]> SM4ProgramsInRecording() => Filter<SongMeter4Program>("wav");

    public static IEnumerable<object[]> SM3Programs() => Filter<SongMeter3Program>("PGM");

    public static IEnumerable<object[]> SM3ProgramsInRecordings() => Filter<SongMeter3Program>("wav");

    private static IEnumerable<object[]> Filter<T>(string extension) =>
        Programs
            .Where(x => x.Value is T && x.Key.EndsWith(extension))
            .Select(x => new object[] { x.Key });

    private static Dictionary<string, SongMeterProgram> MakePrograms()
    {
        var result = new Dictionary<string, SongMeterProgram>();

        var defaultSm4 = new SongMeter4Program();
        result.Add("WA_SM4/SchedulerExamples/default.SM4S", defaultSm4 with
        {
            ScenarioStart = new(2023, 3, 29, 22, 32, 16),
        });

        result.Add("WA_SM4/SchedulerExamples/ONEMINDUTYCYCLE.SM4S", defaultSm4 with
        {
            ScenarioStart = new(2023, 4, 10, 21, 35, 30),
            Schedule = Arr.create(
                new SimpleScheduleEntry(TimeOfDay, Zero, TimeOfDay, Zero, FromSeconds(60), FromSeconds(60))),
            Bitmap1 = Enumerable.Range(0, 720).Map(x => ((x * 120)..((x * 120) + 60))).ToArr(),
            Bitmap2 = Enumerable.Range(0, 720).Map(x => ((x * 120)..((x * 120) + 60))).ToArr(),
        });

        var fnq_rbs_20190102_044802_010 = defaultSm4 with
        {
            Compression = Compression.W4V4,
            LedSettings = LedSettings.LedFiveMinutesOnly,
            MaxLength = FromHours(1),
            Model = Models.SM4,
            Position = new Location("17.07007 S", "145.37851 E", null, null),
            PositionEnabled = false,
            PreampLeft = Preamp.On26dB,
            PreampRight = Preamp.On26dB,
            Prefix = "FNQ-RBS",
            PrefixEnabled = false,
            SampleRate = 24_000,
            SensitivityLeft = -1f,
            SensitivityRight = -1f,
            Schedule = Arr.create<SimpleScheduleEntry>(
                new(Sunrise, FromHours(-2), Sunrise, FromHours(4)),
                new(Sunset, FromHours(-4), Sunset, FromHours(2))),
            Timezone = Offset.FromHours(10),
            TimezoneEnabled = false,
            TriggerLevel = 12,
            TriggerWindow = 3f,
            MinDuration = 0.001_5f,
            Unknown586 = 12,
            ScenarioMemoryCardA = (ulong)128.Gigabytes().Bytes,
            ScenarioMemoryCardB = (ulong)128.Gigabytes().Bytes,
            ScenarioMicrophone0 = SongMeterMicrophone.Internal,
            ScenarioMicrophone1 = SongMeterMicrophone.Internal,
            ScenarioStart = new(2018, 11, 30, 16, 40, 55),
            Bitmap1 = Arr.create(13740..35340, 53820..75420),
            Bitmap2 = Arr.create(13800..35400, 53820..75420),

        };
        result.Add(FnqSchedule, fnq_rbs_20190102_044802_010);
        result.Add("WA_SM4/2.1.1_HighPrecisionDateStamp/FNQ-RBS_20190102_044802_010.wav", fnq_rbs_20190102_044802_010 with
        {
            TriggerWindow = -1,
            ScenarioTriggerRatio = -1,
            ScenarioBatteryEnergy = -1,
            Bitmap1 = Arr.create(13680..35280, 53760..75360),
            Bitmap2 = Arr.create(13740..35340, 53760..75360),
        });

        result.Add("WA_SM4/SchedulerExamples/SONGMETR_2.SM4S", fnq_rbs_20190102_044802_010 with
        {
            Position = new Location("17.07009 S", "145.37857 W", null, null),
            PositionEnabled = true,
            SolarMode = SolarMode.Civil,
            SensitivityEnabled = true,
            SensitivityLeft = 9f,
            SensitivityRight = -4f,
            MaxLength = FromHours(6) + FromMinutes(15),
            ScenarioMicrophone0 = SongMeterMicrophone.SMM_H1,
            ScenarioMicrophone1 = SongMeterMicrophone.SMM_H2,
            Bitmap1 = Arr.create(0..17340, 38580..60180, 82140..86400),
            Bitmap2 = Arr.create(0..17340, 38640..60240, 82200..86400),
        });

        result.Add("WA_SM4/SchedulerExamples/10_Schedules.SM4S", defaultSm4 with
        {
            Prefix = "ABCDEFG",
            PrefixEnabled = true,
            Position = new Location("89.00000 N", "180.00000 E", null, null),
            PositionEnabled = true,
            Timezone = Offset.FromHours(12),
            TimezoneEnabled = true,
            BatteryCutoffVoltageEnabled = true,
            BatteryCutoffVoltage = 12.0f,
            GainLeft = 59.5,
            GainRight = 48.5,
            HighPassFilterLeft = HighPassFilter.On1000Hz,
            HighPassFilterRight = HighPassFilter.On1000Hz,
            SampleRate = 96_000,
            MaxLength = FromHours(24),
            Compression = Compression.W4V8,
            ScenarioStart = new(2023, 3, 9, 17, 6, 58),
            Schedule = Arr.create(
                new SimpleScheduleEntry(FromMinutesSinceMidnight(1), new(5, 7)),
                new SimpleScheduleEntry(new(6, 1), new(19, 12), MakeDuration(7, 18), MakeDuration(6, 2)),
                new SimpleScheduleEntry(new(2, 0), Midnight, MakeDuration(1, 1), MakeDuration(23, 30)),
                new SimpleScheduleEntry(new(3, 0), Midnight, MakeDuration(23, 58), MakeDuration(23, 59)),
                new SimpleScheduleEntry(new(4, 0), new(0, 1), MakeDuration(4, 15), MakeDuration(4, 15)),
                new SimpleScheduleEntry(new(5, 0), new(3, 0), MakeDuration(4, 16), MakeDuration(4, 16)),
                new SimpleScheduleEntry(Sunrise, FromHours(6), Sunset, MakeDuration(4, 32), MakeDuration(20, 41), MakeDuration(22, 45)),
                new SimpleScheduleEntry(Sunset, MakeDuration(-7, 1), Sunrise, MakeDuration(-5, 29)),
                new SimpleScheduleEntry(new(8, 7), new(19, 11), FromMinutes(1), FromMinutes(2)),
                new SimpleScheduleEntry(new(9, 11), new(23, 57), FromMinutes(1), MakeDuration(7, 6))),
        });

        result.Add("WA_SM4/SchedulerExamples/ADVANCED_SCHEDULE.SM4S", defaultSm4 with
        {
            PrefixEnabled = true,
            Prefix = "ADVANCEDSCHE",
            ScenarioStart = new(2023, 3, 20, 11, 29, 39),
            ScheduleMode = ScheduleMode.Advanced,
            AdvancedSchedule = Arr.create<AdvancedScheduleEntry>(
                new Repeat(),
                new Pause() { Duration = FromHours(3) },
                new Record() { Duration = FromHours(3) },
                new UntilCount() { Count = 0 }),
        });

        result.Add("WA_SM4/SchedulerExamples/Max.SM4S", defaultSm4 with
        {
            Prefix = "ABCDEFG",
            PrefixEnabled = true,
            Timezone = Offset.FromHours(12),
            TimezoneEnabled = true,
            Position = new Location("89.00000 N", "180.00000 E", null, null),
            PositionEnabled = true,
            LedSettings = LedSettings.LedFiveMinutesOnly,
            BatteryCutoffVoltage = 12,
            BatteryCutoffVoltageEnabled = true,
            SensitivityLeft = 10,
            SensitivityRight = -120,
            SensitivityEnabled = true,
            Channels = Channel.Right,
            GainLeft = 59.5,
            GainRight = 48.50,
            PreampLeft = Preamp.Off,
            PreampRight = Preamp.On26dB,
            HighPassFilterLeft = HighPassFilter.On220Hz,
            HighPassFilterRight = HighPassFilter.Off,
            SampleRate = 48_000,
            MaxLength = FromHours(24),
            Compression = Compression.W4V8,
            ScenarioMemoryCardA = (ulong)2048.Gigabytes().Bytes,
            ScenarioMemoryCardB = (ulong)1024.Gigabytes().Bytes,
            ScenarioMicrophone0 = SongMeterMicrophone.SMM_A1,
            ScenarioMicrophone1 = SongMeterMicrophone.SMM_A2,
            ScenarioStart = new(2023, 3, 9, 17, 6, 58),
            Schedule = Arr.create(
                new SimpleScheduleEntry(Sunrise, MakeDuration(-23, 59), Sunrise, MakeDuration(23, 59))),
            Bitmap2 = Arr.create(0..60, 120..86400),
        });

        var minBitmap = Enumerable.Range(0, 21)
            .Map(x =>
            {
                var offset = (x * 3600) + 4920;
                return offset..(offset + 300);
            })
            .ToArr()
            .Add(80100..85500);
        result.Add("WA_SM4/SchedulerExamples/Min.SM4S", defaultSm4 with
        {
            Prefix = "XYZABCDEFGHI",
            PrefixEnabled = false,
            Timezone = Offset.FromHoursAndMinutes(-11, 15),
            TimezoneEnabled = true,
            Position = new Location("88.99990 S", "179.12345 W", default, default),
            PositionEnabled = true,
            SolarMode = SolarMode.Astronomical,
            DelayStart = new LocalDate(2023, 04, 15),
            DelayStartEnabled = true,
            LedSettings = LedSettings.LedFiveMinutesOnly,
            BatteryCutoffVoltage = 0.1f,
            BatteryCutoffVoltageEnabled = true,
            Channels = Channel.Left,
            GainLeft = 0,
            GainRight = 12,
            PreampLeft = Preamp.Off,
            PreampRight = Preamp.Off,
            HighPassFilterRight = HighPassFilter.On1000Hz,
            SampleRate = 8000,
            MaxLength = FromMinutes(12),
            Compression = Compression.None,
            ScenarioStart = new(2023, 3, 9, 17, 6, 58),
            Schedule = Arr.create(
                new SimpleScheduleEntry(new(1, 22), new(22, 17), FromMinutes(55), FromMinutes(5)),
                new SimpleScheduleEntry(new(22, 15), new(23, 45))),
            Bitmap1 = minBitmap,
            Bitmap2 = minBitmap,
        });

        var sm4Bat = defaultSm4 with
        {
            Model = Models.SM4BATFS,
            Channels = Channel.Left,
            Prefix = "BAT",
            PrefixEnabled = true,
            Timezone = Offset.Zero,
            TimezoneEnabled = true,
            Position = new Location("0.00000 N", "0.00000 W", default, default),
            PositionEnabled = true,
            SolarMode = SolarMode.Civil,
            GainLeft = 12,
            GainRight = 12,
            PreampLeft = Preamp.Off,
            PreampRight = Preamp.Off,
            HighPassFilterLeft = HighPassFilter.On16000Hz,
            SampleRate = 500_000,
            MinDuration = 0.001_5f,
            MaxDuration = 0f,
            MinTriggerFrequency = 16_000,
            TriggerLevel = 12,
            TriggerWindow = 3f,
            MaxTriggerTime = 15,
            Compression = Compression.None,
            Unknown586 = 12,
            ScenarioMemoryCardA = (ulong)8.Gigabytes().Bytes,
            ScenarioMemoryCardB = (ulong)16.Gigabytes().Bytes,
            ScenarioMicrophone0 = SongMeterMicrophone.SMM_U1,
            ScenarioTriggerRatio = 0.33f,
            ScenarioBatteryEnergy = 50.WattHours().Joules,
            ScenarioStart = new(2023, 3, 10, 13, 22, 57),
            ScheduleMode = ScheduleMode.Advanced,
            AdvancedSchedule = Arr.create<AdvancedScheduleEntry>(
                new AtSunrise() { Offset = MakeDuration(23, 59, 59) },
                new Record() { Duration = MakeDuration(1, 2, 3) },
                new AtDate() { Date = new(2023, 03, 12) },
                new Record() { Duration = MakeDuration(2, 3, 4) },
                new AtTime() { Time = new(22, 23, 24) },
                new Record() { Duration = FromHours(14) },
                new Repeat(),
                new AtSunset() { Offset = FromHours(-4) },
                new Record() { Duration = MakeDuration(0, 13, 3) },
                new UntilDate() { Date = new(2023, 3, 17) },
                new Repeat(),
                new Record() { Duration = FromHours(24) },
                new Pause() { Duration = MakeDuration(1, 2, 3) },
                new UntilCount() { Count = 99 }),
            Schedule = Arr.create<SimpleScheduleEntry>(
                new(Sunrise, MakeDuration(-1), Sunrise, MakeDuration(1)),
                new(Sunset, MakeDuration(-1), Sunset, MakeDuration(1))),
            Bitmap1 = Arr.create(16560..23760, 62700..69900),
            Bitmap2 = Arr.create(16560..23760, 62640..69840),
        };
        result.Add("WA_SM4/SchedulerExamples/SM4BAT.SM4S", sm4Bat);

        var sm4Bat2 = sm4Bat with
        {
            Timezone = Offset.FromHoursAndMinutes(-2, 30),
            SolarMode = SolarMode.Astronomical,
            DelayStart = new LocalDate(2000, 1, 1),
            DelayStartEnabled = true,
            LedSettings = LedSettings.LedFiveMinutesOnly,
            BatteryCutoffVoltage = 3.5f,
            BatteryCutoffVoltageEnabled = true,
            GainLeft = 0,
            GainRight = 0,
            HighPassFilterLeft = HighPassFilter.Off,
            SampleRate = 192_000,
            MinDuration = 0.000_5f,
            MaxDuration = 0.077f,
            MinTriggerFrequency = 24_000,
            TriggerLevel = -66,
            TriggerWindow = 9.0f,
            MaxTriggerTime = 76,
            Compression = Compression.W4V4,
            ScenarioMemoryCardA = (ulong)256.Gigabytes().Bytes,
            ScenarioMemoryCardB = (ulong)512.Gigabytes().Bytes,
            ScenarioMicrophone0 = SongMeterMicrophone.SMM_U2,
            ScenarioTriggerRatio = 0.99f,
            ScenarioBatteryEnergy = 100.WattHours().Joules,
            AdvancedSchedule = Arr.create<AdvancedScheduleEntry>(
                        new AtSunrise() { Offset = MakeDuration(-23, 59, 59) },
                        new Record() { Duration = MakeDuration(24) },
                        new AtDate() { Date = new(2099, 12, 2) },
                        new Record() { Duration = MakeDuration(2, 3, 4) },
                        new AtTime() { Time = new(22, 23, 24) },
                        new Record() { Duration = FromHours(14) },
                        new Repeat(),
                        new AtSunset() { Offset = FromHours(4) },
                        new Record() { Duration = MakeDuration(0, 13, 3) },
                        new UntilDate() { Date = new(2023, 5, 17) },
                        new Repeat(),
                        new Record() { Duration = FromHours(24) },
                        new Pause() { Duration = MakeDuration(1, 2, 3) },
                        new UntilSunset() { Offset = MakeDuration(-1, 2, 3) },
                        new Record() { Duration = MakeDuration(1, 2, 4) }),
            Bitmap1 = Arr.create(4680..11880, 56580..63780),
            Bitmap2 = Arr.create(4620..11820, 56580..63780),
        };
        result.Add("WA_SM4/SchedulerExamples/SM4BAT_2.SM4S", sm4Bat2);

        result.Add("WA_SM4/SchedulerExamples/SM4BAT_384kHz.SM4S", sm4Bat2 with
        {
            Timezone = Offset.Zero,
            Position = new Location("0.00000 N", "0.00000 W", default, default),
            SolarMode = SolarMode.Actual,
            DelayStart = new(2089, 12, 31),
            BatteryCutoffVoltage = 3.3f,
            SampleRate = 384_000,
            MinDuration = 0.007f,
            MaxDuration = 0.080f,
            MinTriggerFrequency = 23_000,
            TriggerLevel = -60,
            TriggerWindow = 5.5f,
            MaxTriggerTime = 79,
            Compression = Compression.W4V8,
            ScheduleMode = ScheduleMode.Daily,
            ScenarioMemoryCardA = 0,
            ScenarioMemoryCardB = 0,
            ScenarioMicrophone0 = SongMeterMicrophone.SMM_U1,
            ScenarioTriggerRatio = 0.1f,
            ScenarioBatteryEnergy = 72.WattHours().Joules,
            AdvancedSchedule = Arr.create<AdvancedScheduleEntry>(
                new AtSunrise() { Offset = MakeDuration(1, 2, 3) },
                new Record() { Duration = MakeDuration(1, 2, 2) },
                new AtDate() { Date = new(2023, 03, 12) },
                new Record() { Duration = MakeDuration(2, 3, 4) },
                new AtTime() { Time = new(22, 23, 24) },
                new Record() { Duration = FromHours(14) },
                new Repeat(),
                new AtSunset() { Offset = FromHours(4) },
                new Record() { Duration = MakeDuration(0, 13, 0) },
                new UntilDate() { Date = new(2023, 3, 17) },
                new Repeat(),
                new Record() { Duration = FromHours(24) },
                new Pause() { Duration = MakeDuration(1, 2, 3) },
                new UntilCount() { Count = 99 }),
            Bitmap1 = Arr.create(17940..25140, 61620..68820),
            Bitmap2 = Arr.create(18000..25200, 61620..68820),
        });

        var sm4zc = defaultSm4 with
        {
            Model = Models.SM4BATZC,
            Channels = Channel.Left,
            GainLeft = 0,
            GainRight = 0,
            SampleRate = 0,
            PreampLeft = Preamp.Off,
            PreampRight = Preamp.Off,
            Unknown578 = 130,
            Unknown582 = 130,
            Prefix = "BATZ-",
            PrefixEnabled = true,
            HighPassFilterLeft = HighPassFilter.On16000Hz,
            DivisionRatio = 8,
            MinDuration = 0.001_5f,
            MaxDuration = 0.003f,
            MinTriggerFrequency = 30_000,
            TriggerWindow = 0.1f,
            MaxTriggerTime = 15,
            ScenarioMicrophone0 = SongMeterMicrophone.SMM_U1,
            ScenarioStart = new(2023, 3, 10, 13, 23, 22),
            Schedule = Arr.create(
                new SimpleScheduleEntry(Sunset, FromMinutes(-30), Sunrise, FromMinutes(30))),
            Bitmap1 = Arr.create(0..23220, 63240..86400),
            Bitmap2 = Arr.create(0..23220, 63240..86400),
        };
        result.Add("WA_SM4/SchedulerExamples/SM4BAT-ZC.SM4S", sm4zc);

        result.Add("WA_SM4/SchedulerExamples/SM4BAT-ZC_2.SM4S", sm4zc with
        {
            Prefix = "BATZ--------",
            HighPassFilterLeft = HighPassFilter.Off,
            DivisionRatio = 16,
            MinDuration = 0.030f,
            MaxDuration = 0.099f,
            MinTriggerFrequency = 90_000,
            MaxTriggerTime = 9,
            ScenarioStart = new(2023, 3, 10, 13, 23, 22),
        });

        var s4U09523 = defaultSm4 with
        {
            Model = Models.SM4BATFS,
            Prefix = "S4U09523",
            PrefixEnabled = false,
            Timezone = Offset.FromHours(-3),
            TimezoneEnabled = false,
            Position = new Location("45.95026 N", "64.23352 W", null, null),
            PositionEnabled = false,
            LedSettings = LedSettings.LedFiveMinutesOnly,
            Channels = Channel.Left,
            GainLeft = 12,
            GainRight = 12,
            HighPassFilterLeft = HighPassFilter.On16000Hz,
            HighPassFilterRight = HighPassFilter.On16000Hz,
            SampleRate = 256_000,
            MinDuration = 0.001f,
            MaxDuration = 0.050f,
            MinTriggerFrequency = 15_000,
            TriggerLevel = 12,
            TriggerWindow = -1,
            MaxTriggerTime = 15,
            Unknown574 = 15,
            Unknown586 = 12,
            SensitivityLeft = -1f,
            SensitivityRight = -1f,
            ScenarioBatteryEnergy = -1f,
            ScenarioTriggerRatio = -1f,
            ScenarioStart = new(2021, 6, 1, 8, 49, 58),
            Schedule = Arr.create(
                new SimpleScheduleEntry(Sunset, FromMinutes(-30), Sunrise, FromMinutes(30))),
        };
        result.Add("WA_SM4BAT/2.2.1_Normal/S4U09523.SM4S", s4U09523);

        result.Add("WA_SM4BAT/2.2.1_Normal/S4U09523_20210621_205706.wav", s4U09523 with
        {
            Position = s4U09523.Position with { Latitude = 45.7835 },
            ScenarioMicrophone0 = SongMeterMicrophone.SMM_U2,
            ScenarioMemoryCardA = (ulong)32.Gigabytes().Bytes,
            ScenarioStart = new(2021, 6, 21, 9, 59, 32),
            Bitmap1 = Arr.create(0..21420, 74400..86400),
            Bitmap2 = Arr.create(0..21420, 74460..86400),
        });
        result.Add("WA_SM4BAT/2.2.1_Normal/S4U09523_20210621_212111.wav", s4U09523 with
        {
            Position = s4U09523.Position with { Latitude = 45.7835 },
            ScenarioMicrophone0 = SongMeterMicrophone.SMM_U2,
            ScenarioMemoryCardA = (ulong)32.Gigabytes().Bytes,
            ScenarioStart = new(2021, 6, 21, 9, 59, 32),
            Bitmap1 = Arr.create(0..21420, 74400..86400),
            Bitmap2 = Arr.create(0..21420, 74460..86400),
        });

        var defaultSM3 = new SongMeter3Program();
        result.Add("WA_SM3/SchedulerExamples/default.PGM", defaultSM3 with
        {
            ScenarioStart = new(2023, 3, 30, 8, 45, 29),
        });
        result.Add("WA_SM3/SchedulerExamples/SM3_SONGMETR.PGM", defaultSM3 with
        {
            Model = Models.SM3M,
            Prefix = "SONGMETER345",
            PrefixEnabled = true,
            Timezone = Offset.FromHoursAndMinutes(-6, -45),
            TimezoneEnabled = true,
            Position = new Location("1.01 N", "2.23 W", default, default),
            PositionEnabled = true,
            SolarMode = SolarMode.Actual,
            SolarModeEnabled = true,
            BatteryCutoffVoltage = 3.3f,
            BatteryCutoffVoltageEnabled = true,
            SensitivityLeft = 13,
            SensitivityRight = 299.9f,
            SensitivityEnabled = true,
            AdvancedSchedule = Arr.create<AdvancedScheduleEntry>(
                new Hpf() { Channel0 = HighPassFilter.On220Hz, Channel1 = HighPassFilter.On1000Hz },
                new Hpf() { Channel0 = HighPassFilter.Off, Channel1 = HighPassFilter.On16000Hz },
                new Gain() { Channel0 = Mode.Automatic, Channel1 = Mode.Automatic },
                new Gain() { Channel0 = 0, Channel1 = 30.5f },
                new FullSpectrum()
                {
                    Format = Format.Wave,
                    Channel = Channel.Stereo,
                    SampleRate = 384_000,
                    AutoRate = false,
                },
                new FullSpectrum()
                {
                    Format = Format.Wac,
                    Channel = Channel.Right,
                    SampleRate = -1,
                    AutoRate = true,
                },
                new ZeroCrossing() { Channel = Channel.Stereo, ZeroCrossingMode = SM3DivisionRatio.DIV8 },
                new FrequencyMinimum() { Channel0 = 0, Channel1 = 16_000 },
                new FrequencyMaximum() { Channel0 = 192_000, Channel1 = 0 },
                new DurationMinimum() { Channel0 = 0.000_1f, Channel1 = 0 },
                new DurationMaximum() { Channel0 = 0.800f, Channel1 = 0 },
                new TriggerLevel() { Channel0 = Mode.Off, Channel1 = Mode.Automatic },
                new TriggerWindow() { Channel0 = 1, Channel1 = 9.9f },
                new TriggerMaximum() { Channel0 = 0.1f, Channel1 = 0 },
                new Nap() { Duration = Zero },
                new Play() { File = "CALL9.WAV" },
                new Repeat(),
                new AtTime() { Time = Midnight },
                new Record() { Duration = MakeDuration(23, 59, 59) },
                new Feature() { Id = "01 - LED DISABLE", Enabled = false },
                new Feature() { Id = "01 - LED DISABLE", Enabled = true },
                new Pause() { Duration = FromMinutes(1) },
                new Feature() { Id = "02 - 32BIT ENABLE", Enabled = true },
                new UntilDate() { Date = new(2023, 3, 16) }),
            ScenarioMicrophone0 = SongMeterMicrophone.SMM_H2,
            ScenarioMicrophone1 = SongMeterMicrophone.SMM_H2,
            ScenarioTriggerRatio = 0.1f,
            ScenarioBatteryEnergy = 576.WattHours().Joules,
            ScenarioStart = new(2023, 3, 10, 13, 43, 5),
            Unknown498 = 6,
        });
        result.Add("WA_SM3/SchedulerExamples/SM3_SONGMETR2.PGM", defaultSM3 with
        {
            Model = Models.SM3,
            Prefix = "SONGMETER3B",
            PrefixEnabled = true,
            Timezone = Offset.FromHoursAndMinutes(12, 45),
            TimezoneEnabled = true,
            Position = new Location("23.32 S", "16.54 E", default, default),
            PositionEnabled = true,
            SolarMode = SolarMode.Astronomical,
            SolarModeEnabled = false,
            BatteryCutoffVoltage = 9.9f,
            BatteryCutoffVoltageEnabled = false,
            SensitivityLeft = -10,
            SensitivityRight = -299.9f,
            SensitivityEnabled = false,
            ScenarioMicrophone0 = SongMeterMicrophone.Unknown,
            ScenarioMicrophone1 = SongMeterMicrophone.Unknown,
            ScenarioBatteryEnergy = 72.WattHours().Joules,
            ScenarioStart = new(2023, 3, 10, 13, 43, 05),
            AdvancedSchedule = Arr.create<AdvancedScheduleEntry>(
                new Hpf() { Channel0 = HighPassFilter.On220Hz, Channel1 = HighPassFilter.On1000Hz },
                new Hpf() { Channel0 = HighPassFilter.Off, Channel1 = HighPassFilter.On16000Hz },
                new Gain() { Channel0 = Mode.Automatic, Channel1 = Mode.Automatic },
                new Gain() { Channel0 = 0, Channel1 = 30.5f },
                new FullSpectrum()
                {
                    Format = Format.Wave,
                    Channel = Channel.Stereo,
                    SampleRate = 24_000,
                    AutoRate = false,
                },
                new FullSpectrum()
                {
                    Format = Format.Wac,
                    Channel = Channel.Right,
                    SampleRate = -1,
                    AutoRate = true,
                },
                new ZeroCrossing() { Channel = Channel.Auto, ZeroCrossingMode = SM3DivisionRatio.DIV4 },
                new FrequencyMinimum() { Channel0 = 192_000, Channel1 = 16_000 },
                new FrequencyMaximum() { Channel0 = 192_000, Channel1 = 0 },
                new DurationMinimum() { Channel0 = 0.800f, Channel1 = 0.555_5f },
                new DurationMaximum() { Channel0 = 0.070f, Channel1 = 0 },
                new TriggerLevel() { Channel0 = -23, Channel1 = 12 },
                new TriggerWindow() { Channel0 = 9.9f, Channel1 = 1.3f },
                new TriggerMaximum() { Channel0 = 99.7f, Channel1 = 99.9f },
                new Nap() { Duration = FromMinutes(99) },
                new Play() { File = "CALL8.WAV" },
                new Repeat(),
                new AtTime() { Time = new(23, 29, 29) },
                new Record() { Duration = MakeDuration(23, 59, 59) },
                new Feature() { Id = "01 - LED DISABLE", Enabled = false },
                new Feature() { Id = "01 - LED DISABLE", Enabled = true },
                new Pause() { Duration = FromMinutes(1) },
                new Feature() { Id = "02 - 32BIT ENABLE", Enabled = true },
                new UntilDate() { Date = new(2023, 3, 16) },
                new Feature() { Id = "01 - LED DISABLE", Enabled = false },
                new TriggerLevel() { Channel0 = 1, Channel1 = -1 }),
        });

        var standard = Arr.create<AdvancedScheduleEntry>(
                new Hpf() { Channel0 = HighPassFilter.Off, Channel1 = HighPassFilter.Off },
                new Gain() { Channel0 = Mode.Automatic, Channel1 = Mode.Automatic },
                new FullSpectrum() { AutoRate = true, Channel = Channel.Auto, Format = Format.Wave, SampleRate = -1 },
                new ZeroCrossing() { Channel = Channel.Off, ZeroCrossingMode = SM3DivisionRatio.DIV8 },
                new FrequencyMinimum() { Channel0 = 16_000, Channel1 = 16_000 },
                new FrequencyMaximum() { Channel0 = 192_000, Channel1 = 192_000 },
                new DurationMinimum() { Channel0 = 0.001_5f, Channel1 = 0.001_5f },
                new DurationMaximum() { Channel0 = 0, Channel1 = 0 },
                new TriggerLevel() { Channel0 = Mode.Automatic, Channel1 = Mode.Automatic },
                new TriggerWindow() { Channel0 = 3, Channel1 = 3 },
                new TriggerMaximum() { Channel0 = 15, Channel1 = 15 });

        var medeas = defaultSM3 with
        {
            Prefix = "SM304290",
            Timezone = Offset.FromHours(10),
            Position = new Location("36.03 N", "84.16 W", default, default),
            SensitivityLeft = -1,
            SensitivityRight = -1,
            ScenarioMemoryCardA = (ulong)256.Gigabytes().Bytes,
            ScenarioMicrophone0 = SongMeterMicrophone.Internal,
            ScenarioMicrophone1 = SongMeterMicrophone.Internal,
            ScenarioBatteryEnergy = 72.WattHours().Joules,
            ScenarioStart = new(2021, 10, 1, 12, 0, 04),
            Unknown498 = 7,
            AdvancedSchedule = standard + Arr.create<AdvancedScheduleEntry>(
                new AtDate() { Date = new(2021, 09, 21) },
                new AtTime() { Time = new(17, 0, 0) },
                new Repeat(),
                new Record() { Duration = FromHours(1) },
                new UntilCount() { Count = 0 }),
        };
        result.Add("WA_SM3/1.35A/MedeasCove-Cemetary-SM304246_20211001_Data/SM304290_0+1_20211001_120004.wav", medeas);
        result.Add("WA_SM3/1.35A/MedeasCove-Cemetary-SM304246_20211013_Data/SM304246_0+1_20211024_002617.wav", medeas with
        {
            Prefix = "SM304246",
            Position = new Location("41.32 S", "148.23 E", default, default),
            ScenarioStart = new(2021, 10, 24, 0, 26, 17),
            AdvancedSchedule = standard + Arr.create<AdvancedScheduleEntry>(
                new AtDate() { Date = new(2021, 10, 13) },
                new AtTime() { Time = new(16, 0, 0) },
                new Repeat(),
                new Record() { Duration = FromHours(1) },
                new UntilCount() { Count = 0 }),

        });
        return result;
    }
}
