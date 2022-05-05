// <copyright file="WamdExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.WildlifeAcoustics
{
    using System.Threading.Tasks;
    using MetadataUtility.Audio;
    using MetadataUtility.Models;
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

            var tryWamdData = Wamd.ExtractMetadata(stream);

            if (tryWamdData.IsSucc)
            {
                Wamd wamdData = (Wamd)tryWamdData;

                int numMicrophones = wamdData.MicrophoneType.Length;

                // Update recording information with wamd metadata
                recording = recording with
                {
                    StartDate = recording.StartDate ?? (wamdData.StartDate.IsLeft ? (OffsetDateTime?)wamdData.StartDate : null),
                    LocalStartDate = recording.LocalStartDate ?? (wamdData.StartDate.IsRight ? (LocalDateTime?)wamdData.StartDate : null),
                    Sensor = (recording.Sensor ?? new Sensor()) with
                    {
                        Name = recording.Sensor?.Name ?? wamdData.Name,
                        SerialNumber = recording.Sensor?.SerialNumber ?? wamdData.SerialNumber,
                        Firmware = recording.Sensor?.Firmware ?? wamdData.Firmware,
                        Temperature = recording.Sensor?.Temperature ?? wamdData.Temperature,
                        Microphones = recording.Sensor?.Microphones ?? new Microphone[numMicrophones],
                    },
                    Location = (recording.Location ?? new Location()) with
                    {
                        Longitude = recording.Location?.Longitude ?? wamdData.Longitude,
                        Latitude = recording.Location?.Latitude ?? wamdData.Latitude,
                    },
                };

                // Update recording microphone information
                for (int i = 0; i < recording.Sensor.Microphones.Length; i++)
                {
                    recording.Sensor.Microphones[i] = recording.Sensor.Microphones[i] ?? new Microphone() with
                    {
                        Type = wamdData.MicrophoneType[i],
                        Sensitivity = wamdData.MicrophoneSensitivity[i],
                    };
                }
            }
            else
            {
                this.logger.LogError("Error extracting comments: {error}", (LanguageExt.Common.Error)tryWamdData);
            }

            return ValueTask.FromResult(recording);
        }
    }
}
