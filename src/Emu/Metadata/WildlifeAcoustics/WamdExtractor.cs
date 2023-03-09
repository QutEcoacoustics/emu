// <copyright file="WamdExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.WildlifeAcoustics
{
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Audio.Vendors.WildlifeAcoustics.WAMD;
    using Emu.Models;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    public class WamdExtractor : IMetadataOperation
    {
        private readonly ILogger<WamdExtractor> logger;

        public WamdExtractor(ILogger<WamdExtractor> logger)
        {
            this.logger = logger;
        }

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

            if (tryWamdData.IsSucc)
            {
                Wamd wamdData = (Wamd)tryWamdData;

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
                    if (wamdData.FileStartTime.Value.Case is OffsetDateTime o)
                    {
                        recording = recording with
                        {
                            StartDate = recording.StartDate ?? o,
                            TrueStartDate = recording.TrueStartDate ?? o,
                            LocalStartDate = recording.LocalStartDate ?? o.LocalDateTime,
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
                    microphones[i] = recording.Sensor?.Microphones?[i] ?? new Microphone() with
                    {
                        Type = wamdData.MicType[i],
                        Sensitivity = wamdData.MicSensitivity[i],
                        Channel = i,
                    };
                }

                // Update recording information with wamd metadata
                recording = recording with
                {
                    Sensor = (recording.Sensor ?? new Sensor()) with
                    {
                        Make = recording.Sensor?.Make ?? Vendor.WildlifeAcoustics.ToNiceName(),
                        Model = recording.Sensor?.Model ?? wamdData.DevModel,
                        Name = recording.Sensor?.Name ?? wamdData.DevName,
                        SerialNumber = recording.Sensor?.SerialNumber ?? wamdData.DevSerialNum,
                        Firmware = recording.Sensor?.Firmware ?? wamdData.SwVersion,
                        Temperature = recording.Sensor?.Temperature ?? wamdData.TempInt,
                        TemperatureExternal = recording.Sensor?.TemperatureExternal ?? wamdData.TempExt,
                        Microphones = recording.Sensor?.Microphones ?? microphones,
                    },
                    Location = location,
                };
            }
            else
            {
                this.logger.LogError("Error extracting comments: {error}", (LanguageExt.Common.Error)tryWamdData);
            }

            return ValueTask.FromResult(recording);
        }
    }
}
