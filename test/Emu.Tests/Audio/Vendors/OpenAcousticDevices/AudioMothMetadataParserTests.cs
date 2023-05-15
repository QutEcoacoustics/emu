// <copyright file="AudioMothMetadataParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Vendors.OpenAcousticDevices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Audio.Vendors.OpenAcousticDevices;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes;
    using Emu.Models.Notices;
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using FluentAssertions.LanguageExt;
    using LanguageExt;
    using NodaTime;
    using static Emu.Audio.Vendors.OpenAcousticDevices.BatteryLimit;
    using static Emu.Audio.Vendors.OpenAcousticDevices.GainSetting;

    public class AudioMothMetadataParserTests
    {
        // These comments were generated by the metamoth project which has random data generation test suite.
        // I didn't feel the need to fully replicate that implementation so I just print()ed some sample comments
        // and made them into static test cases.
        // These DO NOT represent real comments taken from real AudioMoth files.
        private static readonly Dictionary<string, AudioMothComment> Cases = new()
        {
            {
                "Recorded at 00:00:00 01/01/2000 by AudioMoth 0000000000000000 at gain setting 0 while battery state was < 3.6V",
                new(V("1.0"), D(2000), "0000000000000000", Low, BatteryLimit.LessThan, 3.6)
            },
            {
                "Recorded at 02:17:02 06/09/0834 by AudioMoth 00000000000039FC at gain setting 3 while battery state was 3.70V",
                new(V("1.0"), D(834, 9, 6, 2, 17, 2), "00000000000039FC", MediumHigh, Voltage, 3.7)
            },
            {
                "Recorded at 03:46:20 26/02/8542 by AudioMoth 0000000000002855 at gain setting 3 while battery state was > 5.0V",
                new(V("1.0"), D(8542, 2, 26, 3, 46, 20), "0000000000002855", MediumHigh, GreaterThan, 5)
            },
            {
                "Recorded at 16:27:35 03/11/0278 (UTC) by AudioMoth 00000000000015DF at gain setting 3 while battery state was 4.4V",
                new(V("1.0.1", "1.1.0"), D(278, 11, 3, 16, 27, 35), "00000000000015DF", MediumHigh, Voltage, 4.4)
            },
            {
                "Recorded at 00:00:00 01/01/8789 (UTC) by AudioMoth 0000000000000000 at gain setting 0 while battery state was < 3.6V",
                new(V("1.0.1", "1.1.0"), D(8789), "0000000000000000", Low, BatteryLimit.LessThan, 3.6)
            },
            {
                "Recorded at 08:27:14 28/06/0666 (UTC) by AudioMoth 000000000000A26B at gain setting 2 while battery state was > 5.0V",
                new(V("1.0.1", "1.1.0"), D(666, 6, 28, 8, 27, 14), "000000000000A26B", Medium, GreaterThan, 5)
            },
            {
                "Recorded at 00:00:00 01/01/2010 (UTC) by AudioMoth 0000000000000000 at gain setting 0 while battery state was < 3.6V.",
                new(V("1.2.0"), D(2010), "0000000000000000", Low, BatteryLimit.LessThan, 3.6)
            },
            {
                "Recorded at 07:26:31 16/05/1232 (UTC-1) by AudioMoth 00000000000021ED at gain setting 4 while battery state was 4.6V.",
                new(V("1.2.0"), D(1232, 5, 16, 7, 26, 31, -1), "00000000000021ED", High, Voltage, 4.6)
            },
            {
                "Recorded at 18:47:49 16/08/9923 (UTC-10) by AudioMoth 00000000000000B0 at gain setting 2 while battery state was > 5.0V.",
                new(V("1.2.0"), D(9923, 8, 16, 18, 47, 49, -10), "00000000000000B0", Medium, GreaterThan, 5)
            },
            {
                "Recorded at 09:10:17 29/07/4314 (UTC+6) by AudioMoth 370102001EA00202 at gain setting 4 while battery state was 3.6V.",
                new(V("1.2.0"), D(4314, 7, 29, 9, 10, 17, 6), "370102001EA00202", High, Voltage, 3.6)
            },
            {
                "Recorded at 10:24:38 25/12/9597 (UTC+12) by AudioMoth 14053EE3004A008A at gain setting 1 while battery state was 4.3V.",
                new(V("1.2.0"), D(9597, 12, 25, 10, 24, 38, 12), "14053EE3004A008A", LowMedium, Voltage, 4.3)
            },
            {
                "Recorded at 00:00:00 01/01/2007 (UTC) by AudioMoth 0000000000000000 at gain setting 0 while battery state was less than 3.6V.",
                new(V("1.2.1"), D(2007), "0000000000000000", Low, BatteryLimit.LessThan, 3.6)
            },
            {
                "Recorded at 14:29:40 26/06/0002 (UTC+9) by AudioMoth 8617E12423CDB378 at gain setting 3 while battery state was 4.5V. Recording cancelled before completion due to low battery voltage.",
                new(V("1.2.1"), D(2, 6, 26, 14, 29, 40, 9), "8617E12423CDB378", MediumHigh, Voltage, 4.5, RecordingState.LowBattery)
            },
            {
                "Recorded at 04:49:37 25/11/0427 (UTC+7) by AudioMoth 0000000000000000 at gain setting 1 while battery state was greater than 4.9V. Recording cancelled before completion due to change of switch position.",
                new(V("1.2.1"), D(427, 11, 25, 4, 49, 37, 7), "0000000000000000", LowMedium, GreaterThan, 4.9, RecordingState.SwitchChanged)
            },
            {
                "Recorded at 02:22:53 05/09/7932 (UTC-4) by AudioMoth 0000000000000002 at gain setting 1 while battery state was less than 3.6V.",
                new(V("1.2.1"), D(7932, 9, 5, 2, 22, 53, -4), "0000000000000002", LowMedium, BatteryLimit.LessThan, 3.6)
            },
            {
                "Recorded at 11:09:12 16/07/8379 (UTC-9) by AudioMoth 000000000000004B at gain setting 4 while battery state was greater than 4.9V. Recording cancelled before completion due to change of switch position.",
                new(V("1.2.1"), D(8379, 7, 16, 11, 9, 12, -9), "000000000000004B", High, GreaterThan, 4.9, RecordingState.SwitchChanged)
            },
            {
                "Recorded at 15:57:14 20/08/4570 (UTC+12:34) by AudioMoth 0000000000000001 at gain setting 0 while battery state was 4.0V. Recording cancelled before completion due to change of switch position.",
                new(V("1.3.0", "1.2.2"), D(4570, 8, 20, 15, 57, 14, 12, 34), "0000000000000001", Low, Voltage, 4, RecordingState.SwitchChanged, Notices: Seq.create<Notice>(
                    new Warning(
                        "Datestamp may be incorrect, we can't tell for sure though",
                        WellKnownProblems.OpenAcousticDevices.IncorrectHeaderDate)))
            },
            {
                "Recorded at 00:42:42 06/03/1398 (UTC+10:03) by AudioMoth 000000000000E16B at gain setting 3 while battery state was 4.5V. Recording cancelled before completion due to low battery voltage.",
                new(V("1.3.0", "1.2.2"), D(1398, 3, 6, 0, 42, 42, 10, 3), "000000000000E16B", MediumHigh, Voltage, 4.5, RecordingState.LowBattery, Notices: Seq.create<Notice>(
                    new Warning(
                        "Datestamp may be incorrect, we can't tell for sure though",
                        WellKnownProblems.OpenAcousticDevices.IncorrectHeaderDate)))
            },
            {
                "Recorded at 00:00:00 01/01/3467 (UTC) by AudioMoth 0000000000000000 at gain setting 0 while battery state was less than 3.6V.",
                new(V("1.3.0", "1.2.2"), D(3467), "0000000000000000", Low, BatteryLimit.LessThan, 3.6)
            },
            {
                "Recorded at 00:00:00 01/01/2000 (UTC) by AudioMoth 0000000000000000 at low gain setting while battery state was less than 2.5V and temperature was 0.0C.",
                new(V("1.4.0", "1.4.1"), D(2000), "0000000000000000", Low, BatteryLimit.LessThan, 2.5, RecordingState.OK, 0)
            },
            {
                "Recorded at 22:58:10 28/05/1263 (UTC-9:45) by AudioMoth 0000000000009F83 at medium gain setting while battery state was 2.9V and temperature was 37.0C. Amplitude threshold was 44. Band-pass filter applied with cut-off frequencies of 11.3kHz and 4.9kHz.",
                new(V("1.4.0", "1.4.1"), D(1263, 5, 28, 22, 58, 10, -9, -45), "0000000000009F83", Medium, Voltage, 2.9, RecordingState.OK, 37, TriggerType.Amplitude, 44 / 32768.0, null, null, null, null, null, null, (11300, 4900))
            },
            {
                "Recorded at 04:37:04 08/04/1343 (UTC+1:03) by AudioMoth 000000000000A348 at medium-high gain setting while battery state was 2.5V and temperature was 1.0C. Amplitude threshold was 25. Band-pass filter applied with cut-off frequencies of 2.0kHz and 9.0kHz.",
                new(V("1.4.0", "1.4.1"), D(1343, 4, 8, 4, 37, 4, 1, 3), "000000000000A348", MediumHigh, Voltage, 2.5, RecordingState.OK, 1, TriggerType.Amplitude, 25 / 32768.0, null, null, null, null, null, null, (2000, 9000))
            },
            {
                "Recorded at 00:38:55 27/11/9840 (UTC-2:07) by AudioMoth 0000000000000002 at high gain setting while battery state was 2.5V and temperature was -7.0C. Amplitude threshold was 2. High-pass filter applied with cut-off frequency of 0.1kHz.",
                new(V("1.4.0", "1.4.1"), D(9840, 11, 27, 0, 38, 55, -2, -7), "0000000000000002", High, Voltage, 2.5, RecordingState.OK, -7, TriggerType.Amplitude, 2 / 32768.0, null, null, null, null, null, 100, null)
            },
            {
                "Recorded at 18:53:23 24/08/1396 (UTC+0:01) by AudioMoth 0000000000000A37 at high gain setting while battery state was 4.6V and temperature was -4.0C. Low-pass filter applied with cut-off frequency of 2.9kHz.",
                new(V("1.4.2", "1.4.3"), D(1396, 8, 24, 18, 53, 23, 0, 1), "0000000000000A37", High, Voltage, 4.6, RecordingState.OK, -4, TriggerType.None, null, null, null, null, null, 2900, null, null)
            },
            {
                "Recorded at 02:36:12 19/05/0675 (UTC-2:26) by AudioMoth 0000000000000000 at low gain setting while battery state was 3.9V and temperature was -10.0C. Amplitude threshold was 22. Band-pass filter applied with cut-off frequencies of 6.3kHz and 11.7kHz. Recording cancelled before completion due to file size limit.",
                new(V("1.4.2", "1.4.3"), D(675, 5, 19, 2, 36, 12, -2, -26), "0000000000000000", Low, Voltage, 3.9, RecordingState.FileSizeLimit, -10, TriggerType.Amplitude, 22 / 32768.0, null, null, null, null, null, null, (6300, 11700))
            },
            {
                "Recorded at 07:42:47 27/10/2261 (UTC+0:09) by AudioMoth 0000000000000000 at medium-high gain setting while battery state was 4.5V and temperature was 7.0C. Amplitude threshold was 6. High-pass filter applied with cut-off frequency of 1.9kHz.",
                new(V("1.4.2", "1.4.3"), D(2261, 10, 27, 7, 42, 47, 0, 9), "0000000000000000", MediumHigh, Voltage, 4.5, RecordingState.OK, 7, TriggerType.Amplitude, 6 / 32768.0, null, null, null, null, null, 1900, null)
            },
            {
                "Recorded at 18:14:39 18/02/0865 (UTC+3:01) by AudioMoth 0000000000000302 at low gain setting while battery state was less than 2.5V and temperature was -10.0C. Amplitude threshold was 6. Band-pass filter applied with cut-off frequencies of 10.9kHz and 15.7kHz. Recording cancelled before completion due to low voltage.",
                new(V("1.4.2", "1.4.3"), D(865, 2, 18, 18, 14, 39, 3, 1), "0000000000000302", Low, BatteryLimit.LessThan, 2.5, RecordingState.LowBattery, -10, TriggerType.Amplitude, 6 / 32768.0, null, null, null, null, null, null, (10900, 15700))
            },
            {
                "Recorded at 03:00:50 08/03/1815 (UTC+5:26) during deployment 00000000A4168C14 at medium-high gain while battery was 4.1V and temperature was 7.0C. Amplitude threshold was -62 dB with 51s minimum trigger duration. Band-pass filter with frequencies of 15.1kHz and 6.0kHz applied. Recording stopped due to low voltage.",
                new(V("1.6.0"), D(1815, 3, 8, 3, 0, 50, 5, 26), null, MediumHigh, Voltage, 4.1, RecordingState.LowBattery, 7, TriggerType.Amplitude, 0.0007943282347242813, null, null, null, 51, null, null, (15100, 6000), "00000000A4168C14", false)
            },
            {
                "Recorded at 19:18:04 18/05/4757 (UTC-10:19) by AudioMoth 00000000000000E2 at low gain while battery was 4.7V and temperature was 1.0C. Amplitude threshold was 97.5% with 8s minimum trigger duration. Band-pass filter with frequencies of 19.0kHz and 19.8kHz applied.",
                new(V("1.6.0"), D(4757, 5, 18, 19, 18, 4, -10, -19), "00000000000000E2", Low, Voltage, 4.7, RecordingState.OK, 1, TriggerType.Amplitude, 0.975, null, null, null, 8, null, null, (19000, 19800))
            },
            {
                "Recorded at 09:19:09 03/05/0399 (UTC+2:29) by AudioMoth 0000000000000000 using external microphone at low-medium gain while battery was 3.4V and temperature was -4.0C. Amplitude threshold was 85 with 58s minimum trigger duration. Band-pass filter with frequencies of 9.5kHz and 5.7kHz applied. Recording stopped due to file size limit.",
                new(V("1.6.0"), D(399, 5, 3, 9, 19, 9, 2, 29), "0000000000000000", LowMedium, Voltage, 3.4, RecordingState.FileSizeLimit, -4, TriggerType.Amplitude, 85 / 32768.0, null, null, null, 58, null, null, (9500, 5700), null, true)
            },
            {
                "Recorded at 19:19:09 03/05/2023 (UTC+2:29) during deployment 0000000000001234 using external microphone at low-medium gain while battery was 3.4V and temperature was -30.0C. Frequency trigger (3.3kHz and window length of 512 samples) threshold was 70% with 1s minimum trigger duration. Band-pass filter with frequencies of 9.5kHz and 5.7kHz applied. Recording stopped by magnetic switch.",
                new(V("1.8.0", "1.8.1"), D(2023, 5, 3, 19, 19, 9, 2, 29), null, LowMedium, Voltage, 3.4, RecordingState.MagneticSwitch, -30, TriggerType.Frequency, null, 3300, 512, 0.7, 1, null, null, (9500, 5700), "0000000000001234", true)
            },
        };

        public static IEnumerable<object[]> Comments() => Cases.Keys.Select(k => new object[] { k });

        [Theory]
        [MemberData(nameof(Comments))]
        public void ParsesValidData(string comment)
        {
            var expected = Cases[comment];

            var actual = AudioMothMetadataParser.ParseComment(comment, new(), Option<LocalDateTime>.None);

            actual.Should().BeSuccess();

            // in our test cases we know which firmware we're targetting but the parser can still return more possible
            // matches, so we only test expected firmware versions are somewhere in actual
            actual.ThrowIfFail().PossibleFirmwares.Should().Contain(expected.PossibleFirmwares);
            actual.ThrowIfFail().Should().BeEquivalentTo(expected, options => options.Excluding(x => x.PossibleFirmwares));
        }

        [Fact]
        public void UsesFileDateFor122WhenOffsetHasMinutes()
        {
            // in 1.2.2 if a timezone has minutes the datestamp in the comment is encoded incorrectly
            // Utc time: 2000-01-01T01:30
            // UTC offset: +09:30
            // expected offset time: 2000-01-01T11:00 +09:30
            // reported time in comment due to bug: 2000-01-01T10:31:30 +09:30

            // audiomoth datestamps are always in UTC
            var filenameDatestamp = D(2000, 1, 1, 1, 30, 0, 0, 0);
            var correctDatestamp = D(2000, 1, 1, 11, 00, 00, 9, 30);
            var buggedDateStamp = D(2000, 1, 1, 10, 31, 30, 9, 30);

            string badComment = "Recorded at 10:31:30 01/01/2000 (UTC+9:30) by AudioMoth 0000000000000001 at gain setting 0 while battery state was 4.0V.";
            string goodComment = "Recorded at 11:00:00 01/01/2000 (UTC+9:30) by AudioMoth 0000000000000001 at gain setting 0 while battery state was 4.0V.";

            // if no filename datestamp if provided we have no information for which to fix this file
            var actual = AudioMothMetadataParser.ParseComment(badComment, new(), Option<LocalDateTime>.None);
            actual.Should().BeSuccess(result =>
            {
                result.Notices.Should().BeEquivalentTo(
                    new Warning(
                        "Datestamp may be incorrect, we can't tell for sure though",
                        WellKnownProblems.OpenAcousticDevices.IncorrectHeaderDate).AsArray());

                result.PossibleFirmwares.Should().Contain(V("1.2.2", "1.3.0"));
                result.Datestamp.Should().Be(buggedDateStamp);
            });

            // if we provide a filename datestamp, and the header contains a differing date, then we've encountered the bug
            actual = AudioMothMetadataParser.ParseComment(badComment, new(), Option<LocalDateTime>.Some(filenameDatestamp.LocalDateTime));
            actual.Should().BeSuccess(result =>
            {
                result.Notices.Should().BeEquivalentTo(
                    new Warning(
                        "Incorrect datestamp found in header, used filename datestamp instead",
                        WellKnownProblems.OpenAcousticDevices.IncorrectHeaderDate).AsArray());

                result.PossibleFirmwares.Should().Contain(V("1.2.2"));
                result.PossibleFirmwares.Should().NotContain(V("1.3.0"));

                result.Datestamp.Should().Be(correctDatestamp);
            });

            // finally if the there is a filename and it matches the header date then this is the 1.3.0 version of the firmware
            // and we can exclude 1.2.2 as a possible firmware
            actual = AudioMothMetadataParser.ParseComment(goodComment, new(), Option<LocalDateTime>.Some(filenameDatestamp.LocalDateTime));
            actual.Should().BeSuccess(result =>
            {
                result.Notices.Should().BeEmpty();
                result.PossibleFirmwares.Should().NotContain(V("1.2.2"));
                result.PossibleFirmwares.Should().Contain(V("1.3.0"));
                result.Datestamp.Should().Be(correctDatestamp);
            });
        }

        private static OffsetDateTime D(int year, int month = 1, int day = 1, int hour = 0, int minute = 0, int second = 0, int offsetHours = 0, int offsetMinutes = 0)
        {
            return new OffsetDateTime(new(year, month, day, hour, minute, second), Offset.FromHoursAndMinutes(offsetHours, offsetMinutes));
        }

        private static Seq<Version> V(params string[] versions) => versions.Select(v => Version.Parse(v)).ToSeq();
    }
}