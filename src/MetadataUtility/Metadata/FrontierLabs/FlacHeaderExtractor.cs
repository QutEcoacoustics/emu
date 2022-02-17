// <copyright file="FlacHeaderExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.FrontierLabs
{
    using System.Threading.Tasks;
    using MetadataUtility.Audio;
    using MetadataUtility.Models;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    public class FlacHeaderExtractor : IMetadataOperation
    {
        private readonly ILogger<FlacHeaderExtractor> logger;

        public FlacHeaderExtractor(ILogger<FlacHeaderExtractor> logger)
        {
            this.logger = logger;
        }

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.IsFlacFile();

            return ValueTask.FromResult(result);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var samples = Flac.ReadTotalSamples(information.FileStream);
            var sampleRate = Flac.ReadSampleRate(information.FileStream);
            var channels = Flac.ReadNumChannels(information.FileStream);
            var bitDepth = Flac.ReadBitRate(information.FileStream);

            Duration? duration = samples.IsFail || sampleRate.IsFail || (uint)sampleRate == 0 ? null : Duration.FromSeconds((double)samples / (double)sampleRate);
            uint? bitRate = sampleRate.IsFail || bitDepth.IsFail || channels.IsFail ? null : (uint)sampleRate * (uint)bitDepth * (uint)channels;

            recording = recording with
            {
                DurationSeconds = duration,
                SampleRateHertz = sampleRate.IsFail ? null : (uint)sampleRate,
                Channels = channels.IsFail ? null : (byte)channels,
                BitDepth = bitDepth.IsFail ? null : (byte)bitDepth,
                BitsPerSecond = bitRate,
            };

            return ValueTask.FromResult(recording);
        }
    }
}
