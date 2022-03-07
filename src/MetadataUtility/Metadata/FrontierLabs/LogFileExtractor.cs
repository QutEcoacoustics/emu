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
        public const int MetadataOffset = 39;
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
            string[] lines = information.FileSystem.File.ReadAllLines(information.KnownSupportFiles["Log file"]);

            int i = 0;

            while (i < lines.Length)
            {
                if (lines[i].Contains("SD Card :"))
                {
                    break;
                }

                i++;
            }

            recording = recording with
            {
                MemoryCard = new MemoryCard() with
                {
                    SDFormatType = lines[++i].Substring(MetadataOffset),
                    SDManufacturerID = uint.Parse(lines[++i].Substring(MetadataOffset)),
                    SDOEMID = lines[++i].Substring(MetadataOffset),
                    SDProductName = lines[++i].Substring(MetadataOffset),
                    SDProductRevision = float.Parse(lines[++i].Substring(MetadataOffset)),
                    SDSerialNumber = uint.Parse(lines[++i].Substring(MetadataOffset)),
                    SDManufactureDate = lines[++i].Substring(MetadataOffset),
                    SDSpeed = uint.Parse(lines[++i].Substring(MetadataOffset)),
                    SDCapacity = uint.Parse(lines[++i].Substring(MetadataOffset)),
                    SDWrCurrentVmin = uint.Parse(lines[++i].Substring(MetadataOffset)),
                    SDWrCurrentVmax = uint.Parse(lines[++i].Substring(MetadataOffset)),
                    SDWriteB1Size = uint.Parse(lines[++i].Substring(MetadataOffset)),
                    SDEraseB1Size = uint.Parse(lines[++i].Substring(MetadataOffset)),
                },
            };

            return ValueTask.FromResult(recording);
        }
    }
}
