// <copyright file="WaveHeaderExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata
{
    using System.Threading.Tasks;
    using MetadataUtility.Audio;
    using MetadataUtility.Models;
    using MetadataUtility.Utilities;
    using Microsoft.Extensions.Logging;
    using Rationals;

    public class WaveHeaderExtractor : IMetadataOperation
    {
        private readonly ILogger<WaveHeaderExtractor> logger;

        public WaveHeaderExtractor(ILogger<WaveHeaderExtractor> logger)
        {
            this.logger = logger;
        }

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.IsPcmWaveFile() && !information.IsPreallocatedHeader();

            return ValueTask.FromResult(result);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var stream = information.FileStream;

            var riffChunk = Wave.FindRiffChunk(stream);
            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var formatChunk = waveChunk.Bind(w => Wave.FindFormatChunk(stream, w));
            var dataChunk = waveChunk.Bind(w => Wave.FindDataChunk(stream, w));

            if (formatChunk.IsFail)
            {
                this.logger.LogError("Failed to process wave file: {error}", formatChunk);
                return new ValueTask<Recording>(recording);
            }

            var formatSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)formatChunk);

            var sampleRate = Wave.GetSampleRate(formatSpan);
            var bitsPerSample = Wave.GetBitsPerSample(formatSpan);
            var byteRate = Wave.GetByteRate(formatSpan);
            var channels = Wave.GetChannels(formatSpan);
            var blockAlign = Wave.GetBlockAlign(formatSpan);

            var samples = dataChunk.Map(d => Wave.GetTotalSamples(d, channels, bitsPerSample));
            var fileLength = stream.Length;

            // TODO: replace with rational type from master branch
            var duration = samples.Map(s => new Rational((uint)samples) / new Rational((uint)sampleRate));

            return ValueTask.FromResult(recording with
            {
                DurationSeconds = duration.IfFail(null),
                SampleRateHertz = sampleRate,
                Channels = (ushort)channels,
                BlockAlign = blockAlign,
                BitsPerSecond = byteRate * BinaryHelpers.BitsPerByte,
                BitDepth = (byte)bitsPerSample,
                FileLengthBytes = (ulong)fileLength,
            });
        }
    }
}
