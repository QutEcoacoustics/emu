// <copyright file="LogFileExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.FrontierLabs
{
    using System.Threading.Tasks;
    using MetadataUtility.Models;
    using Microsoft.Extensions.Logging;

    public class LogFileExtractor : IMetadataOperation
    {
        private readonly ILogger<LogFileExtractor> logger;

        public LogFileExtractor(ILogger<LogFileExtractor> logger)
        {
            this.logger = logger;
        }

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.HasBarltSupportFile();

            return ValueTask.FromResult(result);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {


            return ValueTask.FromResult(recording);
        }
    }
}
