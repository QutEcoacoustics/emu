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
        public const int SerialNumberLineOffset = 6;
        public const string LogFileKey = "Log file";
        private readonly ILogger<LogFileExtractor> logger;

        public LogFileExtractor(ILogger<LogFileExtractor> logger)
        {
            this.logger = logger;
        }

        public static (int, int)? CorrelateSD(string filename, List<(int Serial, int Line)> sdCards, string[] lines)
        {
            (int, int) sdCard = (0, 0);

            for (int i = 0; i < lines.Length(); i++)
            {
                if (lines[i].Contains(filename))
                {
                    foreach ((int Serial, int Line) sd in sdCards)
                    {
                        if (sd.Line < i)
                        {
                            sdCard = sd;
                        }
                        else
                        {
                            return sdCard;
                        }
                    }
                }
            }

            return null;
        }

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.HasBarltSupportFile();

            return ValueTask.FromResult(result);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            string[] lines = information.FileSystem.File.ReadAllLines(information.KnownSupportFiles[LogFileKey]);

            int i = 0;

            // List to store sd card serial numbers and line numbers
            List<(int, int)> sdCards = new List<(int, int)>();

            while (i < lines.Length)
            {
                if (lines[i].Contains("SD Card :"))
                {
                    sdCards.Add((int.Parse(lines[i + SerialNumberLineOffset].Substring(MetadataOffset)), i));
                }

                i++;
            }

            //Doesn't exist??
            (int, int)? sdCard = sdCards[0];

            foreach ((int, int) sd in sdCards)
            {
                if (sd.Item1 != sdCard?.Item1)
                {
                    sdCard = CorrelateSD(information.FileSystem.Path.GetFileName(information.Path), sdCards, lines);
                }
            }

            if (sdCard != null)
            {
                i = (int)sdCard.Value.Item2;
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
