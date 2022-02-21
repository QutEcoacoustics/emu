// <copyright file="FlacHeaderExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.FrontierLabs
{
    using System.Threading.Tasks;
    using MetadataUtility.Audio;
    using MetadataUtility.Models;
    using NodaTime;

    public class WaveHeaderExtractor : IMetadataOperation
    {
        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = !information.IsWaveFile();

            return ValueTask.FromResult(result);
        }

        public async ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var samples = Wave.ReadTotalSamples(information.FileStream);

            var sampleRate = Wave.ReadWaveSampleRate(information.FileStream);

            var channels = Wave.ReadWaveChannels(information.FileStream);

            var BitRate = Wave.ReadWaveBitsPerSecond(information.FileStream);

            var FileLength = Wave.ReadWaveFileLength(information.FileStream);

            Duration? duration = samples.IsFail ? null : Duration.FromSeconds((ulong)samples / (ulong)sampleRate);

            return recording with
            {
                DurationSeconds = duration,
                SampleRateHertz = (ulong)sampleRate,
                Channels = (byte)channels,
                BitsPerSecond = (uint)BitRate,
                FileLengthBytes = (ulong)FileLength,
            };
        }
    }
}
