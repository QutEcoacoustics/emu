// <copyright file="WamdExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.WildlifeAcoustics
{
    using System.Threading.Tasks;
    using MetadataUtility.Audio;
    using MetadataUtility.Models;
    using Microsoft.Extensions.Logging;

    public class WamdExtractor : IMetadataOperation
    {
        private readonly ILogger<WamdExtractor> logger;

        public WamdExtractor(ILogger<WamdExtractor> logger)
        {
            this.logger = logger;
        }

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.IsPcmWaveFile() && information.HasVersion1WamdChunk();

            return ValueTask.FromResult(result);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var stream = information.FileStream;

            var wamdChunk = Wamd.GetWamdChunk(stream);

            if (wamdChunk.IsFail)
            {
                this.logger.LogError("Failed to process wamd chunk: {error}", (LanguageExt.Common.Error)wamdChunk);
                return ValueTask.FromResult(recording);
            }

            var wamdSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)wamdChunk);

            Wamd wamdData = Wamd.ExtractMetadata(wamdSpan);

            int numMicrophones = wamdData.MicrophoneType.Length;

            // Update recording information with wamd metadata
            recording = recording with
            {
                StartDate = recording.StartDate ?? wamdData.StartDate,
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

            return ValueTask.FromResult(recording);
        }
    }
}
