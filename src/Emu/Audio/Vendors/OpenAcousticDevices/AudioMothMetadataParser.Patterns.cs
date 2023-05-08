// <copyright file="AudioMothMetadataParser.Patterns.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.OpenAcousticDevices
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public partial class AudioMothMetadataParser
    {
        private const string Date = @"(?<Date>\d{2}:\d{2}:\d{2} \d{2}\/\d{2}\/\d{4})";
        private const string Offset = @"\((?<Offset>UTC)\)";
        private const string OffsetHours = @"\((?<Offset>UTC([-+]\d\d?)?)\)";
        private const string OffsetFull = @"\((?<Offset>UTC([-+]\d\d?(:\d\d)?)?)\)";
        private const string SerialNumber = @"(?<SerialNumber>[0-9A-F]{16})";
        private const string GainSettingInt = @"gain setting (?<GainSettingInt>\d)";
        private const string GainSettingWord = @"(?<GainSettingWord>low|low-medium|medium|medium-high|high) gain";
        private const string BatLow25 = @"(?<BatLow25>less than 2\.5)";
        private const string BatLow36 = @"(?<BatLow36>< 3\.6)";
        private const string BatLow36Word = @"(?<BatLow36>less than 3\.6)";
        private const string BatHigh50 = @"(?<BatHigh50>> 5\.0)";
        private const string BatHigh49 = @"(?<BatHigh49>greater than 4\.9)";
        private const string BatteryVoltage = @"(?<BatteryVoltage>\d+\.\d+)";
        private const string BatteryState = $@"battery state was ({BatLow36}|{BatHigh50}|{BatteryVoltage})V";
        private const string BatteryState121 = $@"battery state was ({BatLow36Word}|{BatHigh49}|{BatteryVoltage})V";
        private const string BatteryState140 = $@"battery state was ({BatLow25}|{BatHigh49}|{BatteryVoltage})V";
        private const string BatteryState160 = $@"battery was ({BatLow25}|{BatHigh49}|{BatteryVoltage})V";
        private const string CancelLowBat = "low battery voltage";
        private const string CancelLowBat140 = "low voltage";
        private const string CancelSwitch = "change of switch position";
        private const string CancellSwitch160 = "switch position change";
        private const string CancelFileSizeLimit = "file size limit";
        private const string CancelMicChanged = "microphone change";
        private const string CancelMagneticSwitch = "magnetic switch";
        private const string CancelledDueTo = "Recording cancelled before completion due to";
        private const string CancelledDueTo160 = "Recording stopped due to";
        private const string CancelledDueTo170 = "Recording stopped (due to|by)";
        private const string Cancel = $@"( {CancelledDueTo} (?<Cancel>{CancelLowBat}|{CancelSwitch}).)?";
        private const string Cancel140 = $@"( {CancelledDueTo} (?<Cancel>{CancelLowBat140}|{CancelSwitch}).)?";
        private const string Cancel142 = $@"( {CancelledDueTo} (?<Cancel>{CancelLowBat140}|{CancelSwitch}|{CancelFileSizeLimit}).)?";
        private const string Cancel150 = $@"( {CancelledDueTo} (?<Cancel>{CancelLowBat140}|{CancelSwitch}|{CancelFileSizeLimit}|{CancelMicChanged}).)?";
        private const string Cancel160 = $@"( {CancelledDueTo160} (?<Cancel>{CancelLowBat140}|{CancellSwitch160}|{CancelFileSizeLimit}|{CancelMicChanged}).)?";
        private const string Cancel170 = $@"( {CancelledDueTo170} (?<Cancel>{CancelLowBat140}|{CancellSwitch160}|{CancelFileSizeLimit}|{CancelMicChanged}|{CancelMagneticSwitch})\.)?";
        private const string Temperature = @"temperature was (?<Temperature>-?\d{1,2}\.\d)C";

        private const string AmplitudeThreshold = @"( Amplitude threshold was (?<AmplitudeThreshold>\d+)\.)?";

        // value can be percentage, decibels, or another scalar?
        private const string AmplitudeVarying = @"Amplitude threshold was (?<AmplitudeThreshold>\d{1,4}|\d{1,4}\.?\d{0,4}%|-?\d{1,4} dB)";
        private const string MinimumTriggerDuration = @"with (?<MinimumTriggerDuration>\d{1,4})s minimum trigger duration";
        private const string Trigger = $@"( {AmplitudeVarying} {MinimumTriggerDuration}\.)?";

        private const string FrequencyTriggerCenter = @"(?<FrequencyTriggerCenter>\d+.\d+)kHz";
        private const string FrequencyTriggerWindow = @"window length of (?<FrequencyTriggerWindow>\d+) samples";
        private const string FrequencyTriggerThreshold = @"(?<FrequencyTriggerThreshold>\d{1,3}(.\d{1,4})?)%";

        private const string FrequencyTrigger = $@"Frequency trigger \({FrequencyTriggerCenter} and {FrequencyTriggerWindow}\) threshold was {FrequencyTriggerThreshold}";
        private const string Trigger180 = $@"( ({AmplitudeVarying}|{FrequencyTrigger}) {MinimumTriggerDuration}\.)?";

        private const string LowPassFilter = @"Low-pass filter applied with cut-off frequency of (?<LowPassFilter>\d{1,4}\.\d)kHz";
        private const string HighPassFilter = @"High-pass filter applied with cut-off frequency of (?<HighPassFilter>\d{1,4}\.\d)kHz";
        private const string BandPassFilter = @"Band-pass filter applied with cut-off frequencies of (?<BandPassFilterLow>\d{1,4}\.\d)kHz and (?<BandPassFilterHigh>\d{1,4}\.\d)kHz";
        private const string Filter = $@"( ({LowPassFilter}|{HighPassFilter}|{BandPassFilter})\.)?";

        private const string LowPassFilter160 = @"Low-pass filter with frequency of (?<LowPassFilter>\d{1,4}\.\d)kHz applied";
        private const string HighPassFilter160 = @"High-pass filter with frequency of (?<HighPassFilter>\d{1,4}\.\d)kHz applied";
        private const string BandPassFilter160 = @"Band-pass filter with frequencies of (?<BandPassFilterLow>\d{1,4}\.\d)kHz and (?<BandPassFilterHigh>\d{1,4}\.\d)kHz applied";
        private const string Filter160 = $@"( ({LowPassFilter160}|{HighPassFilter160}|{BandPassFilter160})\.)?";

        private const string DeploymentId = @"(?<DeploymentId>[0-9A-Za-z]{16})";
        private const string Deployment = $@"(during deployment {DeploymentId}|by AudioMoth {SerialNumber})";
        private const string ExternalMic = @"(?<ExternalMic>using external microphone )?";

        // this list was guessed at from two sources:
        // - https://github.com/mbsantiago/metamoth/blob/1eaae415e666950612013f8576c0c40b0107bc45/src/metamoth/parsing.py
        // - and firmware source code https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/blame/master/src/main.c
        //    - each tag was inspected for differences
        private static readonly List<CommentVersion> CommentVersions = new()
        {
            new(Comment100(), default, "1.0"),
            new(Comment101(), default, "1.0.1", "1.1.0"),
            new(Comment120(), default, "1.2.0"),
            new(Comment121(), default, "1.2.1"),
            new(Comment122(), Fix122, "1.2.2", "1.3.0"),
            new(Comment140(), default, "1.4.0", "1.4.1"),
            new(Comment142(), default, "1.4.2", "1.4.3"),
            new(Comment150(), default, "1.5.0"),
            new(Comment160(), default, "1.6.0"),
            new(Comment170(), default, "1.7.0", "1.7.1"),
            new(Comment180(), default, "1.8.0", "1.8.1"),
        };

        [GeneratedRegex($"^AudioMoth {SerialNumber}$")]
        private static partial Regex Artist();

        // valid for 1.0.0
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/blob/311c979fb75f705890654d7d364dd68656d8975c/main.c#L137-L172
        [GeneratedRegex($"^Recorded at {Date} by AudioMoth {SerialNumber} at {GainSettingInt} while {BatteryState}$")]
        private static partial Regex Comment100();

        // valid for 1.0.1 and 1.1.0
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/blob/3b28dee74d871d22cbf6198cbd271424e1b2a8e8/main.c#L137-L169
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/blob/d393d7535acd25c2753b7c6d99753c1a111a1ca0/main.c#L137-L169
        [GeneratedRegex($"^Recorded at {Date} {Offset} by AudioMoth {SerialNumber} at {GainSettingInt} while {BatteryState}$")]
        private static partial Regex Comment101();

        // 1.2.0
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/blob/d88da0cea1b38032c636cdf101c2998d1f6a31ca/main.c#L137-L179
        [GeneratedRegex(@$"^Recorded at {Date} {OffsetHours} by AudioMoth {SerialNumber} at {GainSettingInt} while {BatteryState}\.$")]
        private static partial Regex Comment120();

        // 1.2.1
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/blob/902d47072028b6199946bf87a63d29366bd726f9/main.c#L151-L223
        [GeneratedRegex(@$"^Recorded at {Date} {OffsetHours} by AudioMoth {SerialNumber} at {GainSettingInt} while {BatteryState121}\.{Cancel}$")]
        private static partial Regex Comment121();

        // 1.2.2 and 1.3.0
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/compare/1.2.1...1.2.2#diff-a0cb465674c1b01a07d361f25a0ef2b0214b7dfe9412b7777f89add956da10ecL151-L223
        // Well known problem OAD004.
        [GeneratedRegex(@$"^Recorded at {Date} {OffsetFull} by AudioMoth {SerialNumber} at {GainSettingInt} while {BatteryState121}\.{Cancel}$")]
        private static partial Regex Comment122();

        // 1.4.0 and 1.4.1
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/compare/1.3.0...1.4.0#diff-a0cb465674c1b01a07d361f25a0ef2b0214b7dfe9412b7777f89add956da10ecL151-R285
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/compare/1.4.0...1.4.1 (no changes)
        [GeneratedRegex(
            @$"^Recorded at {Date} {OffsetFull} by AudioMoth {SerialNumber} at {GainSettingWord} setting while {BatteryState140} and {Temperature}\.{AmplitudeThreshold}{Filter}{Cancel140}$")]
        private static partial Regex Comment140();

        // 1.4.2 and 1.4.3 and 1.4.4
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/compare/1.4.1...1.4.2#diff-a0cb465674c1b01a07d361f25a0ef2b0214b7dfe9412b7777f89add956da10ecL189-L285
        [GeneratedRegex(
            @$"^Recorded at {Date} {OffsetFull} by AudioMoth {SerialNumber} at {GainSettingWord} setting while {BatteryState140} and {Temperature}\.{AmplitudeThreshold}{Filter}{Cancel142}$")]
        private static partial Regex Comment142();

        // 1.5.0
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/compare/1.4.4...1.5.0#diff-a0cb465674c1b01a07d361f25a0ef2b0214b7dfe9412b7777f89add956da10ecL196-L300
        [GeneratedRegex(
            @$"^Recorded at {Date} {OffsetFull} {Deployment} {ExternalMic}at {GainSettingWord} setting while {BatteryState140} and {Temperature}\.{AmplitudeThreshold}{Filter}{Cancel150}$")]
        private static partial Regex Comment150();

        // 1.6.0
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/compare/1.5.0...1.6.0#diff-a0cb465674c1b01a07d361f25a0ef2b0214b7dfe9412b7777f89add956da10ecL242-L366
        [GeneratedRegex(
            @$"^Recorded at {Date} {OffsetFull} {Deployment} {ExternalMic}at {GainSettingWord} while {BatteryState160} and {Temperature}\.{Trigger}{Filter160}{Cancel160}$")]
        private static partial Regex Comment160();

        // 1.7.0 and 1.7.1
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/compare/1.6.0...1.7.0#diff-e0cf5b28d9b6b600f0af2bc78e8fd30ec675fd731a5da86f0c4283ffc0e40176L368-L510
        [GeneratedRegex(
            @$"^Recorded at {Date} {OffsetFull} {Deployment} {ExternalMic}at {GainSettingWord} while {BatteryState160} and {Temperature}\.{Trigger}{Filter160}{Cancel170}$")]
        private static partial Regex Comment170();

        // 1.8.0 and 1.8.1
        // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/compare/1.7.1...1.8.0#diff-e0cf5b28d9b6b600f0af2bc78e8fd30ec675fd731a5da86f0c4283ffc0e40176L474-L493
        [GeneratedRegex(
            @$"^Recorded at {Date} {OffsetFull} {Deployment} {ExternalMic}at {GainSettingWord} while {BatteryState160} and {Temperature}\.{Trigger180}{Filter160}{Cancel170}$")]
        private static partial Regex Comment180();
    }
}
