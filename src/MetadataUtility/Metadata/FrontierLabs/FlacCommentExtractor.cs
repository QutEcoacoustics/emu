// <copyright file="FlacCommentExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.FrontierLabs
{
    using System.Threading.Tasks;
    using MetadataUtility.Models;
    using Microsoft.Extensions.Logging;

    public class FlacCommentExtractor : IMetadataOperation
    {
        private readonly ILogger<FlacCommentExtractor> logger;

        public FlacCommentExtractor(ILogger<FlacCommentExtractor> logger)
        {
            this.logger = logger;
        }

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.IsFlacFile() && information.HasFrontierLabsVorbisComment();

            return ValueTask.FromResult(result);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            return ValueTask.FromResult(recording);
        }
    }
}
