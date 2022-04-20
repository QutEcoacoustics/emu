// <copyright file="WaveHeaderExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.WildlifeAcoustics.SM4BAT
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

            var samples = dataChunk.Map(d => Wave.GetTotalSamples(d, channels, bitsPerSample));
            var fileLength = stream.Length;

            //Wamd chunk metadata
            ushort version;

            if (Wamd.IsWildlifeAcousticsWaveFile(stream))
            {
                ushort subchunkId;
                int wamdOffset = 0;

                //Getting the "wamd" chunk
                var wamdChunk = Wamd.ReadWamdChunk(stream);

                //var wamdChunk = Wamd.ReadWamdChunk(stream, (Wamd.Range)wamdChunkRange);

                //Getting METATAG_VERSION
                version = Wamd.GetVersion(wamdChunk);

                //If METATAG_VERSION is not 1, the file should not be analyzed
                if (version == 1)
                {
                    //Advance to the start of the next subchunk
                    wamdOffset += 8;

                    while (wamdOffset < wamdChunk.Length)
                    {
                        throw new NotImplementedException();

                        subchunkId = Wamd.GetSubchunkId(wamdChunk);

                        switch (subchunkId)
                        {
                            case 01:
                                {
                                    //Run extractor for subchunk with the ID of 01. GetDeviceModel(wamdChunk)

                                    //advance wamdOffset to the start of the next subchunk

                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                        }
                    }
                }
            }

            // TODO: replace with rational type from master branch
            var duration = samples.Map(s => new Rational((uint)samples) / new Rational((uint)sampleRate));

            return ValueTask.FromResult(recording with
            {
                DurationSeconds = duration.IfFail(null),
                SampleRateHertz = sampleRate,
                Channels = (ushort)channels,
                BitsPerSecond = byteRate * BinaryHelpers.BitsPerByte,
                BitDepth = (byte)bitsPerSample,
                FileLengthBytes = (ulong)fileLength,
            });
        }
    }
}
