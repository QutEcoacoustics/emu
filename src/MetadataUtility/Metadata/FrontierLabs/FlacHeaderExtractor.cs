// <copyright file="FlacHeaderExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.FrontierLabs
{
    using System.Threading.Tasks;
    using MetadataUtility.Audio;
    using MetadataUtility.Models;
    using NodaTime;

    public class FlacHeaderExtractor : IMetadataOperation
    {
        public ValueTask<bool> CanProcess(TargetInformation information)
        {
            var result = !information.IsFlacFile();

            return ValueTask.FromResult(result);
        }

        public async ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var samples = Flac.ReadTotalSamples(information.FileStream);

            var sampleRate = 22050;
            Duration? duration = samples.IsFail ? null : Duration.FromSeconds((double)samples / sampleRate);

            return recording with
            {
                DurationSeconds = duration,
            };
        }
    }
}
