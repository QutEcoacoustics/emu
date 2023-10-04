// <copyright file="WaveHeaderExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata
{
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Audio.WAVE;
    using Emu.Fixes.FrontierLabs;
    using Emu.Models;
    using Emu.Models.Notices;
    using Emu.Utilities;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using Rationals;

    public class WaveHeaderExtractor : IMetadataOperation
    {
        private readonly ILogger<WaveHeaderExtractor> logger;
        private readonly DataSizeOffBy44 fl008;

        public WaveHeaderExtractor(ILogger<WaveHeaderExtractor> logger, DataSizeOffBy44 fl008)
        {
            this.logger = logger;
            this.fl008 = fl008;
        }

        public string Name => nameof(WaveHeaderExtractor);

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.IsPcmWaveFile() && !information.IsPreallocatedHeader();

            return ValueTask.FromResult(result);
        }

        public async ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var stream = information.FileStream;

            var riffChunk = Wave.FindRiffChunk(stream);

            // special case FL008 handling
            var affectedByFl008 = false;
            if (riffChunk.Match((r) => r.OutOfBounds, (_) => false))
            {
                var checkResult = await this.fl008.CheckAffectedAsync(information.Path);
                affectedByFl008 = checkResult.Status == Fixes.CheckStatus.Affected;
            }

            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var formatChunk = waveChunk.Bind(w => Wave.FindFormatChunk(stream, w));
            var dataChunk = waveChunk.Bind(w => Wave.FindDataChunk(stream, w, allowOutOfBounds: affectedByFl008));

            if (formatChunk.IsFail)
            {
                this.logger.LogError("Failed to process wave file: {error}", formatChunk);
                return recording;
            }

            // now tweak data chunk if FL008 was detected
            if (affectedByFl008)
            {
                dataChunk = dataChunk.Map(r => this.fl008.ModifyDataRange(r));
                recording = recording.AddNotices(this.fl008.Notice);
            }

            return NonAsyncPart(stream, formatChunk.ThrowIfFail(), dataChunk, recording);
        }

        private static Recording NonAsyncPart(
            Stream stream,
            RangeHelper.Range formatChunk,
            Fin<RangeHelper.Range> dataChunk,
            Recording recording)
        {
            var formatSpan = RangeHelper.ReadRange(stream, formatChunk);

            var sampleRate = Wave.GetSampleRate(formatSpan);
            var bitsPerSample = Wave.GetBitsPerSample(formatSpan);
            var byteRate = Wave.GetByteRate(formatSpan);
            var channels = Wave.GetChannels(formatSpan);

            var samples = dataChunk.Bind(d => Wave.GetTotalSamples(d, channels, bitsPerSample));
            var duration = samples.Map(s => new Rational((uint)samples!, (uint)sampleRate));

            var errors = samples.Fails().Concat(duration.Fails()).Distinct();

            return recording with
            {
                DurationSeconds = duration.IfFailNullable(),
                TotalSamples = samples.IfFailNullable(),
                SampleRateHertz = sampleRate,
                Channels = channels,
                BitsPerSecond = byteRate * BinaryHelpers.BitsPerByte,
                BitDepth = (byte)bitsPerSample,
                MediaType = Wave.Mime,
                Notices = recording.Notices.Concat(Error.FromExpectedErrors(errors)),
            };
        }
    }
}
