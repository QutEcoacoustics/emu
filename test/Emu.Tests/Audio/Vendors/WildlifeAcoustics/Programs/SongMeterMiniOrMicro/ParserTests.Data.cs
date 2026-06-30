// <copyright file="ParserTests.Data.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
using Emu.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro;
using NodaTime;
using UnitsNet.NumberExtensions.NumberToEnergy;
using UnitsNet.NumberExtensions.NumberToInformation;
using static Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums.EventType;
using static LanguageExt.Prelude;
using static NodaTime.Duration;
using static NodaTime.EmuExtensions;
using static NodaTime.LocalTime;
using Channel = Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums.Channel;
using Duration = NodaTime.Duration;

public partial class ParserTests
{
    public const string RealSample = @"WA_SMM/3.4_NormalAndCorrupt/SMM215_20231117_095200.wav";

    public static IEnumerable<object[]> ProgramsInFiles =>
        new List<(string, Program)>()
        {
            new(
                RealSample,
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Micro or Micro 2",
                        RecorderName = "SMM215",
                        Timezone = Offset.FromHours(11),
                        Position = new()
                        {
                            Latitude = -37.49779,
                            LatitudePrecision = 5,
                            Longitude = 145.98950,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 24000,

                        // even though this is a mono recording both sample rates are set anyway
                        SampleRateRightChannel = 24000,
                        FullSpectrumSampleRate = 256_000,
                        MaximumRecordingLength = FromMinutes(60),
                        Channels = Channel.Left,
                        Unknown47 = 2,
                        GainLeftDecibels = 18,
                        GainRightDecibels = 18,
                        RecordingFormat = RecordingFormat.FullSpectrum,
                        MinimumTriggerFrequency = 16000,
                        SaveNoiseFiles = false,
                        TriggerWindow = FromSeconds(3),
                        MaximumTriggerRecordingLength = FromSeconds(15),
                        NonTriggeredRecording = false,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = true,
                        DelayStart = null,
                        SchedulesCount = 1,
                        Entries = Array(
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Acoustic,
                                DutyCycleOn = FromMinutes(1),
                                DutyCycleOff = FromMinutes(1),
                            }),
                    },
                }),
        }.Select(t => new object[] { t.Item1, t.Item2 });

    public static IEnumerable<object[]> ConfigFiles =>
        new List<(string, Program)>()
        {
            new(
                "WA_SMM/ConfigExamples/1_02ON_3_04OFF.miniconfig",
                new Program()
                 {
                     Configuration = new()
                     {
                         IntendedModel = "Song Meter Micro or Micro 2",
                         RecorderName = "ABCDEFGHIJKL",
                         Timezone = Offset.FromHoursAndMinutes(-12, -45),
                         Position = new()
                         {
                             Latitude = 1.23450,
                             LatitudePrecision = 5,
                             Longitude = 123.45600,
                             LongitudePrecision = 5,
                         },
                         SampleRateLeftChannel = 256_000,
                         SampleRateRightChannel = 256_000,
                         FullSpectrumSampleRate = 256_000,
                         MaximumRecordingLength = FromMinutes(1),
                         Channels = Channel.Left,
                         Unknown47 = 2,
                         GainLeftDecibels = 24,
                         GainRightDecibels = 18,
                         RecordingFormat = RecordingFormat.FullSpectrum,
                         MinimumTriggerFrequency = 16000,
                         SaveNoiseFiles = false,
                         TriggerWindow = FromSeconds(3),
                         MaximumTriggerRecordingLength = FromSeconds(15),
                         NonTriggeredRecording = false,
                         RecordingMode = RecordingQualityMode.HighQuality,
                     },
                     Schedule = new()
                     {
                         ScheduleFromRecording = true,
                         DelayStart = new LocalDateTime(2025, 9, 24, 0, 0, 0),
                         SchedulesCount = 1,
                         Entries = Array(
                             new ScheduleEntry()
                             {
                                 Mode = ScheduleEntryMode.Acoustic,
                                 DutyCycleOn = MakeDuration(1, 2),
                                 DutyCycleOff = MakeDuration(3, 4),
                             }),
                     },
                 }),
            new(
                "WA_SMM/ConfigExamples/1ON_1OFF.miniconfig",
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Micro or Micro 2",
                        RecorderName = "SMM215",
                        Timezone = Offset.FromHoursAndMinutes(11, 00),
                        Position = new()
                        {
                            Latitude = -37.49779,
                            LatitudePrecision = 5,
                            Longitude = 145.98950,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 24_000,
                        SampleRateRightChannel = 24_000,
                        FullSpectrumSampleRate = 256_000,
                        MaximumRecordingLength = FromMinutes(60),
                        Channels = Channel.Left,
                        Unknown47 = 2,
                        GainLeftDecibels = 18,
                        GainRightDecibels = 18,
                        RecordingFormat = RecordingFormat.FullSpectrum,
                        MinimumTriggerFrequency = 16000,
                        SaveNoiseFiles = false,
                        TriggerWindow = FromSeconds(3),
                        MaximumTriggerRecordingLength = FromSeconds(15),
                        NonTriggeredRecording = false,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = true,
                        DelayStart = null,
                        SchedulesCount = 1,
                        Entries = Array(
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Acoustic,
                                DutyCycleOn = FromMinutes(1),
                                DutyCycleOff = FromMinutes(1),
                            }),
                    },
                }),
            new(
                "WA_SMM/ConfigExamples/10_schedules_delayed_start.miniconfig",
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Mini or Mini 2",
                        RecorderName = string.Empty,
                        Timezone = Offset.FromHours(0),
                        Position = new()
                        {
                            Latitude = 0,
                            LatitudePrecision = 5,
                            Longitude = 0,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 24_000,
                        SampleRateRightChannel = 24_000,
                        FullSpectrumSampleRate = 256_000,
                        MaximumRecordingLength = FromMinutes(60),
                        Channels = Channel.Left,
                        Unknown47 = 2,
                        GainLeftDecibels = 18,
                        GainRightDecibels = 18,
                        RecordingFormat = RecordingFormat.FullSpectrum,
                        MinimumTriggerFrequency = 16000,
                        SaveNoiseFiles = false,
                        TriggerWindow = FromSeconds(3),
                        MaximumTriggerRecordingLength = FromSeconds(15),
                        NonTriggeredRecording = false,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = false,
                        DelayStart = new LocalDateTime(2099, 1, 1, 0, 0, 0),
                        SchedulesCount = 10,
                        Entries = Array(
                           new ScheduleEntry()
                           {
                                StartTime = MakeDuration(0),
                                EndTime = MakeDuration(1),
                           },
                           new ScheduleEntry()
                           {
                               StartTime = MakeDuration(2),
                               EndTime = MakeDuration(3),
                           },
                           new ScheduleEntry()
                           {
                               StartTime = MakeDuration(4),
                               EndTime = MakeDuration(5),
                           },
                           new ScheduleEntry()
                           {
                               StartTime = MakeDuration(6),
                               EndTime = MakeDuration(7),
                           },
                           new ScheduleEntry()
                           {
                               StartTime = MakeDuration(8),
                               EndTime = MakeDuration(9),
                           },
                           new ScheduleEntry()
                           {
                               StartTime = MakeDuration(10),
                               EndTime = MakeDuration(11, 2),
                           },
                           new ScheduleEntry()
                           {
                               StartTime = MakeDuration(12),
                               EndTime = MakeDuration(13),
                           },
                           new ScheduleEntry()
                           {
                               StartTime = MakeDuration(14),
                               EndTime = MakeDuration(15),
                           },
                           new ScheduleEntry()
                           {
                               StartTime = MakeDuration(16),
                               EndTime = MakeDuration(17),
                           },
                           new ScheduleEntry()
                           {
                               StartTime = MakeDuration(18),
                               EndTime = MakeDuration(19),
                           }),
                    },
                }),
            new(
                "WA_SMM/ConfigExamples/always_on.miniconfig",
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Mini or Mini 2",
                        RecorderName = "111111111111",
                        Timezone = Offset.FromHoursAndMinutes(12, 15),
                        Position = new()
                        {
                            Latitude = 1.23450,
                            LatitudePrecision = 5,
                            Longitude = 123.45600,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 8_000,
                        SampleRateRightChannel = 8_000,
                        FullSpectrumSampleRate = 256_000,
                        MaximumRecordingLength = FromMinutes(1),
                        Channels = Channel.Left,
                        Unknown47 = 2,
                        GainLeftDecibels = 6,
                        GainRightDecibels = 12,
                        RecordingFormat = RecordingFormat.FullSpectrum,
                        MinimumTriggerFrequency = 16000,
                        SaveNoiseFiles = false,
                        TriggerWindow = FromSeconds(3),
                        MaximumTriggerRecordingLength = FromSeconds(15),
                        NonTriggeredRecording = false,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = true,
                        DelayStart = new LocalDateTime(2010, 1, 1, 0, 0, 0),
                        SchedulesCount = 1,
                        Entries = Array(
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Acoustic,
                            }),
                        DefaultAlwaysOnSchedule = 4,
                    },
                }),
            new(
                "WA_SMM/ConfigExamples/always_on_2359_to_2358.miniconfig",
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Mini or Mini 2",
                        RecorderName = "111111111111",
                        Timezone = Offset.FromHoursAndMinutes(12, 15),
                        Position = new()
                        {
                            Latitude = 1.23450,
                            LatitudePrecision = 5,
                            Longitude = 123.45600,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 8_000,
                        SampleRateRightChannel = 8_000,
                        FullSpectrumSampleRate = 256_000,
                        MaximumRecordingLength = FromMinutes(1),
                        Channels = Channel.Left,
                        Unknown47 = 2,
                        GainLeftDecibels = 6,
                        GainRightDecibels = 12,
                        RecordingFormat = RecordingFormat.FullSpectrum,
                        MinimumTriggerFrequency = 16000,
                        SaveNoiseFiles = false,
                        TriggerWindow = FromSeconds(3),
                        MaximumTriggerRecordingLength = FromSeconds(15),
                        NonTriggeredRecording = false,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = true,
                        DelayStart = new LocalDateTime(2010, 1, 1, 0, 0, 0),
                        SchedulesCount = 1,
                        Entries = Array(
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Acoustic,
                                StartTime = MakeDuration(23, 59),
                                EndTime = MakeDuration(23, 58),
                            }),
                        DefaultAlwaysOnSchedule = 0,
                    },
                }),
            new(
                "WA_SMM/ConfigExamples/always_on_mini_bat.miniconfig",
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Mini Bat or Mini Bat 2",
                        RecorderName = "111111111111",
                        Timezone = Offset.FromHoursAndMinutes(12, 15),
                        Position = new()
                        {
                            Latitude = 1.23450,
                            LatitudePrecision = 5,
                            Longitude = 123.45600,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 256_000,
                        SampleRateRightChannel = 24_000,
                        FullSpectrumSampleRate = 256_000,
                        MaximumRecordingLength = FromHours(1),
                        Channels = Channel.Left,
                        Unknown47 = 0,
                        GainLeftDecibels = 12,
                        GainRightDecibels = 18,
                        RecordingFormat = RecordingFormat.FullSpectrum,
                        MinimumTriggerFrequency = 16000,
                        SaveNoiseFiles = false,
                        TriggerWindow = FromSeconds(3),
                        MaximumTriggerRecordingLength = FromSeconds(16),
                        NonTriggeredRecording = false,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = true,
                        DelayStart = new LocalDateTime(2010, 1, 1, 0, 0, 0),
                        SchedulesCount = 1,
                        Entries = Array(
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Ultrasonic,
                            }),
                        DefaultAlwaysOnSchedule = 1,
                    },
                }),
            new(
                "WA_SMM/ConfigExamples/always_on_micro.miniconfig",
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Micro or Micro 2",
                        RecorderName = "111111111111",
                        Timezone = Offset.FromHoursAndMinutes(12, 15),
                        Position = new()
                        {
                            Latitude = 1.23450,
                            LatitudePrecision = 5,
                            Longitude = 123.45600,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 24_000,
                        SampleRateRightChannel = 24_000,
                        FullSpectrumSampleRate = 256_000,
                        MaximumRecordingLength = FromMinutes(60),
                        Channels = Channel.Left,
                        Unknown47 = 0,
                        GainLeftDecibels = 18,
                        GainRightDecibels = 18,
                        RecordingFormat = RecordingFormat.FullSpectrum,
                        MinimumTriggerFrequency = 16000,
                        SaveNoiseFiles = false,
                        TriggerWindow = FromSeconds(3),
                        MaximumTriggerRecordingLength = FromSeconds(15),
                        NonTriggeredRecording = false,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = true,
                        DelayStart = new LocalDateTime(2010, 1, 1, 0, 0, 0),
                        SchedulesCount = 1,
                        Entries = Array(
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Acoustic,
                            }),
                        DefaultAlwaysOnSchedule = 4,
                    },
                }),
            new(
                "WA_SMM/ConfigExamples/bat_default.miniconfig",
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Mini Bat or Mini Bat 2",
                        RecorderName = string.Empty,
                        Timezone = Offset.Zero,
                        Position = new()
                        {
                            Latitude = 0,
                            LatitudePrecision = 5,
                            Longitude = 0,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 256_000,
                        SampleRateRightChannel = 24_000,
                        FullSpectrumSampleRate = 256_000,
                        MaximumRecordingLength = FromHours(1),
                        Channels = Channel.Left,
                        Unknown47 = 0,
                        GainLeftDecibels = 12,
                        GainRightDecibels = 18,
                        RecordingFormat = RecordingFormat.FullSpectrum,
                        MinimumTriggerFrequency = 16000,
                        SaveNoiseFiles = false,
                        TriggerWindow = FromSeconds(3),
                        MaximumTriggerRecordingLength = FromSeconds(15),
                        NonTriggeredRecording = false,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = false,
                        DelayStart = null,
                        SchedulesCount = 1,
                        Entries = Array(
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Ultrasonic,
                            }),
                        DefaultAlwaysOnSchedule = 1,
                    },
                }),
            new(
                "WA_SMM/ConfigExamples/bat_variant.miniconfig",
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Mini Bat or Mini Bat 2",
                        RecorderName = string.Empty,
                        Timezone = Offset.Zero,
                        Position = new()
                        {
                            Latitude = 0,
                            LatitudePrecision = 5,
                            Longitude = 0,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 384_000,
                        SampleRateRightChannel = 96_000,
                        FullSpectrumSampleRate = 384_000,
                        MaximumRecordingLength = FromMinutes(45),
                        Channels = Channel.Left,
                        Unknown47 = 0,
                        GainLeftDecibels = 6,
                        GainRightDecibels = 18,
                        RecordingFormat = RecordingFormat.ZeroCrossing,
                        MinimumTriggerFrequency = 32000,
                        SaveNoiseFiles = true,
                        TriggerWindow = FromSeconds(6),
                        MaximumTriggerRecordingLength = FromSeconds(9),
                        NonTriggeredRecording = false,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = false,
                        DelayStart = null,
                        SchedulesCount = 1,
                        Entries = Array(
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Ultrasonic,
                            }),
                        DefaultAlwaysOnSchedule = 1,
                    },
                }),
            new(
                "WA_SMM/ConfigExamples/bat_variant_2.miniconfig",
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Mini Bat or Mini Bat 2",
                        RecorderName = string.Empty,
                        Timezone = Offset.Zero,
                        Position = new()
                        {
                            Latitude = 0,
                            LatitudePrecision = 5,
                            Longitude = 0,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 500_000,
                        SampleRateRightChannel = 8_000,
                        FullSpectrumSampleRate = 500_000,
                        MaximumRecordingLength = FromHours(1),
                        Channels = Channel.Left,
                        Unknown47 = 0,
                        GainLeftDecibels = 0,
                        GainRightDecibels = 6,
                        RecordingFormat = RecordingFormat.ZeroCrossingAndFullSpectrum,
                        MinimumTriggerFrequency = 24000,
                        SaveNoiseFiles = true,
                        TriggerWindow = FromSeconds(9),
                        MaximumTriggerRecordingLength = FromMinutes(30),
                        NonTriggeredRecording = true,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = false,
                        DelayStart = null,
                        SchedulesCount = 1,
                        Entries = Array(
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Ultrasonic,
                            }),
                        DefaultAlwaysOnSchedule = 1,
                    },
                }),
            new(
                "WA_SMM/ConfigExamples/bat_variant_2_flip_dont_save_noise_files.miniconfig",
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Mini Bat or Mini Bat 2",
                        RecorderName = string.Empty,
                        Timezone = Offset.Zero,
                        Position = new()
                        {
                            Latitude = 0,
                            LatitudePrecision = 5,
                            Longitude = 0,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 500_000,
                        SampleRateRightChannel = 8_000,
                        FullSpectrumSampleRate = 500_000,
                        MaximumRecordingLength = FromHours(1),
                        Channels = Channel.Left,
                        Unknown47 = 0,
                        GainLeftDecibels = 0,
                        GainRightDecibels = 6,
                        RecordingFormat = RecordingFormat.ZeroCrossingAndFullSpectrum,
                        MinimumTriggerFrequency = 24000,
                        SaveNoiseFiles = false,
                        TriggerWindow = FromSeconds(9),
                        MaximumTriggerRecordingLength = FromSeconds(15),
                        NonTriggeredRecording = false,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = false,
                        DelayStart = null,
                        SchedulesCount = 1,
                        Entries = Array(
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Ultrasonic,
                            }),
                        DefaultAlwaysOnSchedule = 1,
                    },
                }),
            new(
                "WA_SMM/ConfigExamples/complex_schedules_high_quality.miniconfig",
                new Program()
                {
                    Configuration = new()
                    {
                        IntendedModel = "Song Meter Mini or Mini 2",
                        RecorderName = "111111111111",
                        Timezone = Offset.FromHoursAndMinutes(-1, -15),
                        Position = new()
                        {
                            Latitude = -1.23450,
                            LatitudePrecision = 5,
                            Longitude = -123.45600,
                            LongitudePrecision = 5,
                        },
                        SampleRateLeftChannel = 8_000,
                        SampleRateRightChannel = 8_000,
                        FullSpectrumSampleRate = 256_000,
                        MaximumRecordingLength = FromMinutes(1),
                        Channels = Channel.Stereo,
                        Unknown47 = 2,
                        GainLeftDecibels = 6,
                        GainRightDecibels = 18,
                        RecordingFormat = RecordingFormat.FullSpectrum,
                        MinimumTriggerFrequency = 16000,
                        SaveNoiseFiles = false,
                        TriggerWindow = FromSeconds(3),
                        MaximumTriggerRecordingLength = FromSeconds(15),
                        NonTriggeredRecording = false,
                        RecordingMode = RecordingQualityMode.HighQuality,
                    },
                    Schedule = new()
                    {
                        ScheduleFromRecording = true,
                        DelayStart = new LocalDateTime(2010, 1, 1, 0, 0, 0),
                        SchedulesCount = 3,
                        Entries = Array(
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Acoustic,
                                StartTime = MakeDuration(6, 3),
                                EndTime = MakeDuration(7, 6),
                            },
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Acoustic,
                                StartTime = MakeDuration(0, 2),
                                EndTime = MakeDuration(0, 0),
                                StartDateMonth = 9,
                                StartDateDay = 24,
                                EndDateMonth = 9,
                                EndDateDay = 23,
                                DutyCycleDaysOn = 2,
                                DutyCycleDaysOff = 4,
                            },
                            new ScheduleEntry()
                            {
                                Mode = ScheduleEntryMode.Acoustic,
                                StartTime = MakeDuration(1, 2),
                                StartType = Sunrise,
                                EndTime = MakeDuration(-3, 4),
                                EndType = Sunset,
                                StartDateMonth = 1,
                                StartDateDay = 2,
                                EndDateMonth = 12,
                                EndDateDay = 31,
                            }),
                        DefaultAlwaysOnSchedule = 0,
                    },
                }),
            // new(
            //     "WA_SMM/ConfigExamples/complex_schedules_single_mic.miniconfig",
            //      new Program()
            //      {
            //          Configuration = new()
            //          {
            //              IntendedModel = "Song Meter Micro or Micro 2",
            //              RecorderName = "SMM215",
            //              Timezone = Offset.FromHours(10),
            //              Position = new()
            //              {
            //                  Latitude = -17.07007,
            //                  LatitudePrecision = 5,
            //                  Longitude = 145.37851,
            //                  LongitudePrecision = 5,
            //              },
            //              SampleRateLeftChannel = 96000,
            //              SampleRateRightChannel = 96000,
            //              FullSpectrumSampleRate = 256_000,
            //              MaximumRecordingLength = FromMinutes(10),
            //              Channels = Channel.Both,
            //              Unknown47 = 0,
            //              GainLeftDecibels = 0,
            //              GainRightDecibels = 0,
            //              RecordingFormat = RecordingFormat.FullSpectrum,
            //              MinimumTriggerFrequency = 2000,
            //              SaveNoiseFiles = true,
            //              TriggerWindow = FromSeconds(3),
            //              MaximumTriggerRecordingLength = FromSeconds(15),
            //              NonTriggeredRecording = false,
            //              RecordingMode = RecordingQualityMode.HighQuality,
            //          },
            //          Schedule = new()
            //          {
            //              ScheduleFromRecording = false,
            //              DelayStart = null,
            //              SchedulesCount = 1,
            //              Entries = Array(
            //                  new ScheduleEntry()
            //                  {
            //                      Mode = ScheduleEntryMode.Acoustic,
            //                      StartTime = new LocalTime(1, 0),
            //                      EndTime = new LocalTime(2, 0),
            //                      DutyCycleOn = FromMinutes(3),
            //                      DutyCycleOff = FromMinutes(4),
            //                  }),
            //          },
            //      }),
            // new(
            //     "WA_SMM/ConfigExamples/complex_schedules_two_mics_right_channel_only.miniconfig",
            //      new Program()
            //      {
            //          Configuration = new()
            //          {
            //              IntendedModel = "Song Meter Micro or Micro 2",
            //              RecorderName = "SMM215",
            //              Timezone = Offset.FromHours(10),
            //              Position = new()
            //              {
            //                  Latitude = -17.07007,
            //                  LatitudePrecision = 5,
            //                  Longitude = 145.37851,
            //                  LongitudePrecision = 5,
            //              },
            //              SampleRateLeftChannel = 96000,
            //              SampleRateRightChannel = 96000,
            //              FullSpectrumSampleRate = 256_000,
            //              MaximumRecordingLength = FromMinutes(10),
            //              Channels = Channel.Both,
            //              Unknown47 = 0,
            //              GainLeftDecibels = 0,
            //              GainRightDecibels = 0,
            //              RecordingFormat = RecordingFormat.FullSpectrum,
            //              MinimumTriggerFrequency = 2000,
            //              SaveNoiseFiles = true,
            //              TriggerWindow = FromSeconds(3),
            //              MaximumTriggerRecordingLength = FromSeconds(15),
            //              NonTriggeredRecording = false,
            //              RecordingMode = RecordingQualityMode.HighQuality,
            //          },
            //          Schedule = new()
            //          {
            //              ScheduleFromRecording = false,
            //              DelayStart = null,
            //              SchedulesCount = 1,
            //              Entries = Array(
            //                  new ScheduleEntry()
            //                  {
            //                      Mode = ScheduleEntryMode.Acoustic,
            //                      StartTime = new LocalTime(1, 0),
            //                      EndTime = new LocalTime(2, 0),
            //                      DutyCycleOn = FromMinutes(3),
            //                      DutyCycleOff = FromMinutes(4),
            //                  }),
            //          },
            //      }),
            // new(
            //     "WA_SMM/ConfigExamples/default.miniconfig",
            //      new Program()
            //      {
            //          Configuration = new()
            //          {
            //              IntendedModel = "Song Meter Micro or Micro 2",
            //              RecorderName = "SMM215",
            //              Timezone = Offset.FromHours(10),
            //              Position = new()
            //              {
            //                  Latitude = -17.07007,
            //                  LatitudePrecision = 5,
            //                  Longitude = 145.37851,
            //                  LongitudePrecision = 5,
            //              },
            //              SampleRateLeftChannel = 96000,
            //              SampleRateRightChannel = 96000,
            //              FullSpectrumSampleRate = 256_000,
            //              MaximumRecordingLength = FromMinutes(10),
            //              Channels = Channel.Both,
            //              Unknown47 = 0,
            //              GainLeftDecibels = 0,
            //              GainRightDecibels = 0,
            //              RecordingFormat = RecordingFormat.FullSpectrum,
            //              MinimumTriggerFrequency = 2000,
            //              SaveNoiseFiles = true,
            //              TriggerWindow = FromSeconds(3),
            //              MaximumTriggerRecordingLength = FromSeconds(15),
            //              NonTriggeredRecording = false,
            //              RecordingMode = RecordingQualityMode.HighQuality,
            //          },
            //          Schedule = new()
            //          {
            //              ScheduleFromRecording = false,
            //              DelayStart = null,
            //              SchedulesCount = 1,
            //              Entries = Array(
            //                  new ScheduleEntry()
            //                  {
            //                      Mode = ScheduleEntryMode.Acoustic,
            //                      StartTime = new LocalTime(1, 0),
            //                      EndTime = new LocalTime(2, 0),
            //                      DutyCycleOn = FromMinutes(3),
            //                      DutyCycleOff = FromMinutes(4),
            //                  }),
            //          },
            //      }),
            // new(
            //     "WA_SMM/ConfigExamples/test_schedule_overflow_section.miniconfig",
            //      new Program()
            //      {
            //          Configuration = new()
            //          {
            //              IntendedModel = "Song Meter Micro or Micro 2",
            //              RecorderName = "SMM215",
            //              Timezone = Offset.FromHours(10),
            //              Position = new()
            //              {
            //                  Latitude = -17.07007,
            //                  LatitudePrecision = 5,
            //                  Longitude = 145.37851,
            //                  LongitudePrecision = 5,
            //              },
            //              SampleRateLeftChannel = 96000,
            //              SampleRateRightChannel = 96000,
            //              FullSpectrumSampleRate = 256_000,
            //              MaximumRecordingLength = FromMinutes(10),
            //              Channels = Channel.Both,
            //              Unknown47 = 0,
            //              GainLeftDecibels = 0,
            //              GainRightDecibels = 0,
            //              RecordingFormat = RecordingFormat.FullSpectrum,
            //              MinimumTriggerFrequency = 2000,
            //              SaveNoiseFiles = true,
            //              TriggerWindow = FromSeconds(3),
            //              MaximumTriggerRecordingLength = FromSeconds(15),
            //              NonTriggeredRecording = false,
            //              RecordingMode = RecordingQualityMode.HighQuality,
            //          },
            //          Schedule = new()
            //          {
            //              ScheduleFromRecording = false,
            //              DelayStart = null,
            //              SchedulesCount = 1,
            //              Entries = Array(
            //                  new ScheduleEntry()
            //                  {
            //                      Mode = ScheduleEntryMode.Acoustic,
            //                      StartTime = new LocalTime(1, 0),
            //                      EndTime = new LocalTime(2, 0),
            //                      DutyCycleOn = FromMinutes(3),
            //                      DutyCycleOff = FromMinutes(4),
            //                  }),
            //          },
            //      }),
        }.Select(t => new object[] { t.Item1, t.Item2 });
}
