// <copyright file="WamdExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.WildlifeAcoustics
{
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes;
    using Emu.Audio.Vendors.WildlifeAcoustics.WAMD;
    using Emu.Models;
    using Emu.Models.Notices;
    using LanguageExt;
    using LanguageExt.Pipes;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    public class WamdExtractor : IRawMetadataOperation
    {
        private readonly ILogger<WamdExtractor> logger;

        public WamdExtractor(ILogger<WamdExtractor> logger)
        {
            this.logger = logger;
        }

        public string Name => "WAMD";

        public bool UseWamdOffsets { get; internal set; } = true;

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            // TODO: Add support for .wac (or other Wildlife Acoustic) files
            var result = information.IsPcmWaveFile() && information.HasVersion1WamdChunk();

            return ValueTask.FromResult(result);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var stream = information.FileStream;

            var tryWamdData = WamdParser.ExtractMetadata(stream);

            if (tryWamdData.IsFail)
            {
                this.logger.LogError("Error extracting WAMD metadata: {error}", (LanguageExt.Common.Error)tryWamdData);
                return ValueTask.FromResult(recording);
            }

            if (tryWamdData.IsSucc)
            {
                (Wamd wamdData, List<Notice> notices) = tryWamdData.ThrowIfFail();

                this.logger.LogDebug("Extracted WAMD metadata: {wamdData}", wamdData);
                this.logger.LogDebug("Extracted WAMD notices: {notices}", notices);
                int numMicrophones = wamdData.MicType.Length;

                var location = recording.Location;
                if (wamdData.GpsFirst is not null)
                {
                    location = wamdData.GpsFirst;
                }
                else if (wamdData.PosLast is not null)
                {
                    location = wamdData.PosLast;
                }

                if (wamdData.FileStartTime is not null)
                {
                    if (wamdData.FileStartTime.Value.Case is OffsetDateTime offsetDateTime)
                    {
                        var useOffsets = this.UseWamdOffsets;
                        recording = recording with
                        {
                            StartDate = recording.StartDate == null && useOffsets ? offsetDateTime : recording.StartDate,
                            TrueStartDate = recording.TrueStartDate ?? offsetDateTime,
                            LocalStartDate = recording.LocalStartDate ?? offsetDateTime.LocalDateTime,
                        };
                    }
                    else
                    {
                        var localDateTime = (LocalDateTime)wamdData.FileStartTime.Value;
                        recording = recording with
                        {
                            LocalStartDate = recording.LocalStartDate ?? localDateTime,
                        };
                    }
                }

                var microphones = new Microphone[numMicrophones];
                for (int i = 0; i < numMicrophones; i++)
                {
                    double? gain = null;
                    if (wamdData.DevParams is SongMeter4Program program)
                    {
                        // preamp only applies to internal microphones
                        var externalMicrophone = wamdData.MicType[i] is "U2" or "U1";
                        gain = i switch
                        {
                            0 when externalMicrophone => program.GainLeft,
                            1 when externalMicrophone => program.GainRight,
                            0 => program.GainLeft + (int)program.PreampLeft,
                            1 => program.GainRight + (int)program.PreampRight,
                            _ => throw new NotImplementedException("Not enough channels"),
                        };
                    }
                    else if (wamdData.DevParams is SongMeter3Program sm3Program)
                    {
                        var gains = sm3Program.AdvancedSchedule.Where(x => x is Gain).Cast<Gain>();
                        var length = gains.Length();
                        if (length == 0)
                        {
                            gain = 0;
                        }
                        else if (length > 1)
                        {
                            throw new NotSupportedException("Don't know how to work with more than one gain entry " + Meta.CallToAction);
                        }

                        // https://www.wildlifeacoustics.com/uploads/user-guides/SM3-USER-GUIDE-20200805.pdf page 36
                        gain = (i switch
                        {
                            0 => gains.First().Channel0,
                            1 => gains.First().Channel1,
                            _ => throw new NotImplementedException("Not enough channels"),
                        })
                        .IfLeft(GuessAutoGain);
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported program type " + Meta.CallToAction);
                    }

                    float GuessAutoGain(Audio.Vendors.WildlifeAcoustics.Programs.Enums.Mode mode)
                    {
                        if (mode == Audio.Vendors.WildlifeAcoustics.Programs.Enums.Mode.Automatic)
                        {
                            return wamdData.MicType[i] switch
                            {
                                "IN" => 24,
                                "U2" => 24,
                                "NA" => 24,

                                // gain auto should be 0 for hydrophones but I don't have any sample recordings to test against
                                string s => throw new NotSupportedException($"Don't recognize the microphone type `{s}`" + Meta.CallToAction),
                            };
                        }

                        return 0;
                    }

                    microphones[i] = recording.Sensor?.Microphones?[i] ?? new Microphone() with
                    {
                        Type = wamdData.MicType[i],
                        Sensitivity = wamdData.MicSensitivity[i],
                        Channel = i,
                        Gain = gain,
                    };
                }

                // Update recording information with wamd metadata
                recording = recording with
                {
                    Sensor = (recording.Sensor ?? new Sensor()) with
                    {
                        Make = recording.Sensor?.Make ?? Vendor.WildlifeAcoustics.GetEnumMemberValueOrDefault(),
                        Model = recording.Sensor?.Model ?? wamdData.DevModel,
                        Name = recording.Sensor?.Name ?? wamdData.DevName,
                        SerialNumber = recording.Sensor?.SerialNumber ?? wamdData.DevSerialNum,
                        Firmware = recording.Sensor?.Firmware ?? wamdData.SwVersion,
                        Temperature = recording.Sensor?.Temperature ?? wamdData.TempInt,
                        TemperatureExternal = recording.Sensor?.TemperatureExternal ?? wamdData.TempExt,
                        Microphones = recording.Sensor?.Microphones ?? microphones,
                    },
                    Location = location,
                    Notices = recording.Notices.Concat(notices),
                };
            }
            else
            {
                this.logger.LogError("Error extracting comments: {error}", (LanguageExt.Common.Error)tryWamdData);
            }

            return ValueTask.FromResult(recording);
        }

        public ValueTask<MetadataExtractionResult> ProcessFileAsync(TargetInformation information)
        {
            var stream = information.FileStream;

            var tryWamdData = WamdParser.ExtractMetadata(stream);

            if (tryWamdData.IsSucc)
            {
                (Wamd wamdData, List<Notice> notices) = tryWamdData.ThrowIfFail();
                return ValueTask.FromResult<MetadataExtractionResult>(new(wamdData, notices.ToSeq()));
            }

            return ValueTask.FromResult<MetadataExtractionResult>(null);
        }
    }
}
