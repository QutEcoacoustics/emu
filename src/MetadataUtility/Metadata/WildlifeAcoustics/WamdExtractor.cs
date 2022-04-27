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
            var result = information.IsPcmWaveFile(); // && information.HasValidWamdChunk

            return ValueTask.FromResult(result);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var stream = information.FileStream;

            var wamdChunk = Wamd.GetWamdChunk(stream);

            if (wamdChunk.IsFail)
            {
                this.logger.LogError("Failed to process wamd chunk: {error}", (LanguageExt.Common.Error)wamdChunk);
                return ValueTask.FromResult(recording); ;
            }

            var wamdSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)wamdChunk);

            //var wamdData = Wamd.ExtractMetadata(wamdSpan);

            //update recording

            return ValueTask.FromResult(recording);
        }
    }
}
