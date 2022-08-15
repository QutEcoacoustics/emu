// <copyright file="FlacHeaderExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.FrontierLabs
{
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Models;
    using Microsoft.Extensions.Logging;
    using Rationals;

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
            var bitDepth = Flac.ReadBitDepth(information.FileStream);
            var md5 = Flac.ReadMD5(information.FileStream);

            Rational? duration = samples.IsFail || sampleRate.IsFail || (uint)sampleRate == 0 ? null : (new Rational((ulong)samples) / new Rational((uint)sampleRate));
            uint? bitRate = sampleRate.IsFail || bitDepth.IsFail || channels.IsFail ? null : (uint)sampleRate * (uint)bitDepth * (uint)channels;

            recording = recording with
            {
                DurationSeconds = recording.DurationSeconds ?? duration,
                SampleRateHertz = recording.SampleRateHertz ?? (sampleRate.IsFail ? null : (uint)sampleRate),
                TotalSamples = recording.TotalSamples ?? (samples.IsFail ? null : (ulong)samples),
                Channels = recording.Channels ?? (channels.IsFail ? null : (byte)channels),
                BitDepth = recording.BitDepth ?? (bitDepth.IsFail ? null : (byte)bitDepth),
                BitsPerSecond = recording.BitsPerSecond ?? bitRate,
                EmbeddedChecksum = recording.EmbeddedChecksum ?? md5.Match(
                        Succ: x => new Checksum() { Type = "MD5", Value = x.ToHexString() },
                        Fail: null),
                MediaType = Flac.Mime,

                // in cases where no filename extension was available, we can backfill a recommended extension
                Extension = recording.Extension ?? Flac.Extension,
            };

            return ValueTask.FromResult(recording);
        }
    }
}
