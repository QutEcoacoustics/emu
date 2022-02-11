


namespace MetadataUtility.Metadata.FrontierLabs
{
    using System.Threading.Tasks;
    using MetadataUtility.Audio;
    using MetadataUtility.Models;
    using NodaTime;

    public class FlacHeaderExtractor : IMetadataOperation
    {
        public async ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            if (information.Predicates.ContainsKey("isFlacFile"))
            {
                return information.Predicates["isFlacFile"];
            }

            var result = Flac.IsFlacFile(information.FileStream);
            information.Predicates["isFlacFile"] = (bool)result;

            return (bool)result;

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

