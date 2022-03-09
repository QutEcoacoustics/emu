// <copyright file="WaveHeaderExtractor.cs" company="QutEcoacoustics">
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
        //public static readonly Error FileNotWavePCM = Error.New("Error reading file: file must be wave format PCM to be processed");

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            //var result = !information.IsWaveFile();
            var result = information.IsWaveFilePCM();

            return ValueTask.FromResult(result);
        }

        public async ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            //Stream.Seek(0, SeekOrigin.Begin);
            //Span<byte> data = stackalloc byte[(int)information.FileStream.Length];

            var samples = Wave.ReadTotalSamples(information.FileStream);
            var sampleRate = Wave.ReadWaveSampleRate(information.FileStream);
            var channels = Wave.ReadWaveChannels(information.FileStream);
            var bitRate = Wave.ReadWaveBitsPerSecond(information.FileStream);
            var fileLength = Wave.ReadWaveFileLength(information.FileStream);

            Duration? duration = samples.IsFail ? null : Duration.FromSeconds((ulong)samples / (ulong)sampleRate);

            return recording with
            {
                DurationSeconds = duration,
                SampleRateHertz = (uint)sampleRate,
                Channels = (byte)channels,
                BitsPerSecond = (uint)bitRate,
                FileLengthBytes = (ulong)fileLength,
            };
        }
    }
}
