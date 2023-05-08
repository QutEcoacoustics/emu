// <copyright file="AudioMothMetadataParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.OpenAcousticDevices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Emu.Audio.WAVE;
    using Emu.Models.Notices;
    using LanguageExt;
    using LanguageExt.Common;
    using LanguageExt.UnsafeValueAccess;
    using NodaTime;
    using NodaTime.Text;
    using UnitsNet.NumberExtensions.NumberToFrequency;
    using UnitsNet.NumberExtensions.NumberToRatio;
    using Error = LanguageExt.Common.Error;

    public partial class AudioMothMetadataParser
    {
        //public const string DeviceName = "AudioMoth";
        public static readonly byte[] DeviceName = "AudioMoth"u8.ToArray();

        public static readonly Error CommentError = Error.New("Could not parse the AudioMoth comment. No patterns matched the comment string. " + Meta.CallToAction);

        private static readonly Error MissingInfo = Error.New("AudioMoth file has a LIST chunk but is missing the comment chunk");
        private static readonly Error BadList = Error.New("AudioMoth file has a unknown LIST chunk type");
        private static readonly Error ArtistMismatch = Error.New("Could not parse the AudioMoth artist. " + Meta.CallToAction);

        private static readonly LocalDateTimePattern DatePattern = LocalDateTimePattern.CreateWithInvariantCulture("HH:mm:ss dd/MM/yyyy");
        private static readonly OffsetPattern OffsetPatternShort = OffsetPattern.CreateWithInvariantCulture("+H");
        private static readonly OffsetPattern OffsetPatternLong = OffsetPattern.CreateWithInvariantCulture("+H:mm");

        public static Fin<bool> HasAudioMothListChunk(Stream stream)
        {
            return FindChunk(stream).Map(Check);

            static bool Check(byte[] bytes)
            {
                return bytes.AsSpan().IndexOf(DeviceName) >= 0;
            }
        }

        public static Fin<AudioMothComment> ParseAudioMothListChunk(Stream stream, Option<LocalDateTime> filenameDatestamp)
        {
            var bytes = FindChunk(stream);
            if (bytes.IsFail)
            {
                return (Error)bytes;
            }

            var span = bytes.ThrowIfFail().AsSpan();
            return Wave.ParseListChunk(span).Case switch
            {
                Error e => e,
                InfoList info => ParseInfoList(info, filenameDatestamp),
                _ => BadList,
            };
        }

        public static Fin<string> ParseArtist(string artist)
        {
            // get serial number from artist because it is sometimes not present in comment
            // when deployment id is set
            var matchArtist = Artist().Match(artist);
            if (!matchArtist.Success)
            {
                return ArtistMismatch;
            }
            else
            {
                return matchArtist.Groups[nameof(SerialNumber)].Value;
            }
        }

        public static Fin<AudioMothComment> ParseComment(string comment, AudioMothComment result, Option<LocalDateTime> filenameDatestamp)
        {
            bool anyMatched = false;

            foreach (var version in CommentVersions)
            {
                var match = version.CommentParser.Match(comment);
                if (!match.Success)
                {
                    continue;
                }

                anyMatched = true;

                var (voltage, limit) = ParseBatteryState(match);

                result = result with
                {
                    // these fields should always be present in the comment
                    // we could match any number of regexes, return all versions and let the user sort it out
                    PossibleFirmwares = result.PossibleFirmwares.Concat(version.Versions.Map(Version.Parse)),
                    Datestamp = ParseDate(match),
                    GainSetting = ParseGain(match),
                    BatteryLevel = limit,
                    Voltage = voltage,

                    SerialNumber = result.SerialNumber ??
                        (match.Groups[nameof(SerialNumber)] is { Success: true } serial ? serial.Value : null),

                    // now for optionals/newer comments
                    RecordingState = ParseRecordingState(match),
                    Temperature = ParseTemperature(match),
                    DeploymentId = match.Groups[nameof(DeploymentId)] is { Success: true } deployment ? deployment.Value : null,
                    ExternalMicrophone = match.Groups[nameof(ExternalMic)].Success,
                };

                result = ParseFilter(match, result);
                result = ParseTrigger(match, result);

                if (version.Transform is not null)
                {
                    result = version.Transform(filenameDatestamp, result);
                }
            }

            return anyMatched ? result : CommentError;
        }

        private static Fin<byte[]> FindChunk(Stream stream)
        {
            var riffChunk = Wave.FindRiffChunk(stream);
            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var listChunk = waveChunk.Bind(w => Wave.ReadListChunk(stream, w, Wave.InfoChunkId));

            return listChunk.Bind<byte[]>(x => x.IsSome ? (byte[])x : Wave.ChunkNotFound(Wave.InfoChunkId));
        }

        private static Fin<AudioMothComment> ParseInfoList(InfoList info, Option<LocalDateTime> filenameDatestamp)
        {
            var artist = info.Entries.FirstOrDefault(x => Wave.InfoArtistChunkId.SequenceEqual(x.Type))?.Text;
            var comment = info.Entries.FirstOrDefault(x => Wave.InfoCommentChunkId.SequenceEqual(x.Type))?.Text;

            // we allow a missing artist comment
            string serialNumber = null;
            if (artist is not null)
            {
                var aritstResult = ParseArtist(artist);
                if (aritstResult.IsFail)
                {
                    return (Error)aritstResult;
                }

                serialNumber = (string)aritstResult;
            }

            if (comment is null)
            {
                return MissingInfo;
            }

            var justArtist = new AudioMothComment(SerialNumber: serialNumber);

            var withComment = ParseComment(comment, justArtist, filenameDatestamp);

            return withComment;
        }

        private static OffsetDateTime ParseDate(Match match)
        {
            var localDate = DatePattern.Parse(match.Groups[nameof(Date)].Value);
            var offset = match.Groups[nameof(Offset)].Value;

            if (offset == "UTC" || offset == string.Empty)
            {
                return localDate.Value.WithOffset(NodaTime.Offset.Zero);
            }
            else
            {
                // trim off UTC
                offset = offset[3..];
                if (OffsetPatternShort.Parse(offset) is { Success: true } offsetValue)
                {
                    return localDate.Value.WithOffset(offsetValue.Value);
                }
                else if (OffsetPatternLong.Parse(offset) is { Success: true } offsetValue2)
                {
                    return localDate.Value.WithOffset(offsetValue2.Value);
                }
            }

            throw new ArgumentException("Cannot parse date");
        }

        private static GainSetting ParseGain(Match match)
        {
            if (match.Groups[nameof(GainSettingInt)] is { Success: true } value)
            {
                return (GainSetting)int.Parse(value.Value);
            }

            if (match.Groups[nameof(GainSettingWord)] is { Success: true } value2)
            {
                return value2.Value switch
                {
                    "low" => GainSetting.Low,
                    "low-medium" => GainSetting.LowMedium,
                    "medium" => GainSetting.Medium,
                    "medium-high" => GainSetting.MediumHigh,
                    "high" => GainSetting.High,
                    _ => throw new ArgumentException($"Cannot parse gain type: {value2.Value}"),
                };
            }

            throw new ArgumentException("Cannot parse gain");
        }

        private static (double Voltage, BatteryLimit Level) ParseBatteryState(Match match)
        {
            return match.Groups[nameof(BatteryVoltage)] switch
            {
                Group g when g.Success => (double.Parse(g.Value), BatteryLimit.Voltage),
                _ when match.Groups[nameof(BatLow25)] is { Success: true } => (2.5, BatteryLimit.LessThan),
                _ when match.Groups[nameof(BatLow36)] is { Success: true } => (3.6, BatteryLimit.LessThan),
                _ when match.Groups[nameof(BatHigh49)] is { Success: true } => (4.9, BatteryLimit.GreaterThan),
                _ when match.Groups[nameof(BatHigh50)] is { Success: true } => (5.0, BatteryLimit.GreaterThan),
                _ => throw new ArgumentException("Cannot parse battery state"),
            };
        }

        private static RecordingState ParseRecordingState(Match match)
        {
            return match.Groups[nameof(Cancel)].Value switch
            {
                CancelLowBat => RecordingState.LowBattery,
                CancelLowBat140 => RecordingState.LowBattery,
                CancelSwitch => RecordingState.SwitchChanged,
                CancellSwitch160 => RecordingState.SwitchChanged,
                CancelFileSizeLimit => RecordingState.FileSizeLimit,
                CancelMicChanged => RecordingState.MicrophoneChanged,
                CancelMagneticSwitch => RecordingState.MagneticSwitch,
                "" => RecordingState.OK,
                string s => throw new ArgumentException($"Cannot parse cancel reason {s}"),
            };
        }

        private static double? ParseTemperature(Match match)
        {
            if (match.Groups[nameof(Temperature)] is { Success: true } temperature)
            {
                return double.Parse(temperature.Value);
            }

            return null;
        }

        private static AudioMothComment ParseTrigger(Match match, AudioMothComment comment)
        {
            double? minimum = double.TryParse(match.Groups[nameof(MinimumTriggerDuration)].Value, out var d) ? d : null;

            if (match.Groups[nameof(AmplitudeThreshold)] is { Success: true } amplitude)
            {
                comment = comment with
                {
                    TriggerType = TriggerType.Amplitude,

                    // there are three types of threshold: 16-bit, percentage, and decibel
                    // the value stored is always a raw amplitude value, but audio moth presents it in the
                    // comment in the same way it was set by the user. i.e. a dB threshold is stored as
                    // an linear amplitude threshold, but then formatted back to dB when presented in the comment.
                    // https://github.com/OpenAcousticDevices/AudioMoth-Configuration-App/blob/0ede251bf4a9d0e2989b53728e1367235ab2e164/saveLoad.js#L65-L72
                    // https://github.com/OpenAcousticDevices/AudioMoth-Configuration-App/blob/0ede251bf4a9d0e2989b53728e1367235ab2e164/saveLoad.js#L582
                    // https://github.com/OpenAcousticDevices/AudioMoth-Configuration-App/blob/0ede251bf4a9d0e2989b53728e1367235ab2e164/uiIndex.js#L734-L753
                    // regardless, we convert it to a unit interval here, no matter the input
                    AmplitudeTriggerThreshold = amplitude.Value switch
                    {
                        string s when s.EndsWith(" dB") => Math.Pow(10, double.Parse(s[..^3]) / 20.0),
                        string s when s.EndsWith("%") => double.Parse(s[..^1]) / 100,
                        string s => double.Parse(s) / 32768,
                    },
                    MinimumTriggerDuration = minimum,
                };
            }
            else if (match.Groups[nameof(FrequencyTriggerCenter)] is { Success: true } frequencyTriggerCenter)
            {
                comment = comment with
                {
                    TriggerType = TriggerType.Frequency,
                    FrequencyTriggerCenter = double.Parse(frequencyTriggerCenter.Value).Kilohertz().Hertz,
                    FrequencyTriggerWindow = int.Parse(match.Groups[nameof(FrequencyTriggerWindow)].Value),
                    FrequencyTriggerThreshold = double.Parse(match.Groups[nameof(FrequencyTriggerThreshold)].Value).Percent().DecimalFractions,
                    MinimumTriggerDuration = minimum,
                };
            }

            return comment;
        }

        private static AudioMothComment ParseFilter(Match match, AudioMothComment comment)
        {
            double? low = null, high = null;

            if (match.Groups[nameof(LowPassFilter)] is { Success: true } lowFilter)
            {
                low = double.Parse(lowFilter.Value).Kilohertz().Hertz;
            }

            if (match.Groups[nameof(HighPassFilter)] is { Success: true } highFilter)
            {
                high = double.Parse(highFilter.Value).Kilohertz().Hertz;
            }

            (double, double)? bandpass = null;
            if (match.Groups["BandPassFilterLow"] is { Success: true } bandpassLow)
            {
                var bandpassHigh = match.Groups["BandPassFilterHigh"];

                bandpass = (double.Parse(bandpassLow.Value).Kilohertz().Hertz, double.Parse(bandpassHigh.Value).Kilohertz().Hertz);
            }

            return comment with
            {
                LowPassFilter = low,
                HighPassFilter = high,
                BandPassFilter = bandpass,
            };
        }

        private static AudioMothComment Fix122(Option<LocalDateTime> filenameDatestamp, AudioMothComment current)
        {
            // Well known problem OAD004
            // https://github.com/ecoacoustics/known-problems/blob/main/open_acoustic_devices/OAD004.md
            //
            // If we detect the problem with can disambiguate the firmware version based on the comment string.

            // cases:
            // 1. no datestamp, no comment - shouldn't be here
            // 2. no datestamp, but a comment - file has been renamed, we cant fix this
            // 3. datestamp and comment, and dates match, no need to fix
            // 4. datestamp and comment, but dates don't match, we need to fix
            //
            // Note: don't trample other firmware metadata

            Notice notice;
            if (filenameDatestamp.IsNone)
            {
                // without a file date stamp to check against there's nothing we can do
                // especially since we're not sure this is a 1.2.2 firmware - a range of firmwares could match
                if (current.Datestamp.Offset.IsWholeHourOffset())
                {
                    return current;
                }
                else
                {
                    // issue a warning though
                    notice = new Warning(
                        "Datestamp may be incorrect, we can't tell for sure though",
                        WellKnownProblems.OpenAcousticDevices.IncorrectHeaderDate);

                    return current with { Notices = current.Notices.Add(notice) };
                }
            }

            // we assume audio moth datestamps are always in UTC
            var fileDate = filenameDatestamp.ValueUnsafe().WithOffset(NodaTime.Offset.Zero);

            // if we have datestamp and it matches comment, then the issue has been fixed
            // convert to instanct because offsetdatetime compares equality by components
            // - i.e. checks the date is at the same offset as the other
            if (current.Datestamp.ToInstant() == fileDate.ToInstant())
            {
                if (current.Datestamp.Offset.IsWholeHourOffset())
                {
                    return current;
                }

                var exclude122 = new Version("1.2.2");
                return current with { PossibleFirmwares = current.PossibleFirmwares.Where(f => f != exclude122) };
            }

            notice = new Warning(
                    "Incorrect datestamp found in header, used filename datestamp instead",
                    WellKnownProblems.OpenAcousticDevices.IncorrectHeaderDate);

            var exclude130 = new Version("1.3.0");
            return current with
            {
                // if we have a datestamp, then we can use the filename datestamp
                Datestamp = fileDate.WithOffset(current.Datestamp.Offset),
                PossibleFirmwares = current.PossibleFirmwares.Where(f => f != exclude130),
                Notices = current.Notices.Add(notice),
            };
        }

        private record CommentVersion(Regex CommentParser, Func<Option<LocalDateTime>, AudioMothComment, AudioMothComment> Transform = default, params string[] Versions);
    }
}
