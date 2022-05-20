// <copyright file="LogFileExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.FrontierLabs
{
    using System.Threading.Tasks;
    using MetadataUtility.Metadata.SupportFiles.FrontierLabs;
    using MetadataUtility.Models;
    using Microsoft.Extensions.Logging;

    public class LogFileExtractor : IMetadataOperation
    {
        private readonly ILogger<LogFileExtractor> logger;

        public LogFileExtractor(ILogger<LogFileExtractor> logger)
        {
            this.logger = logger;
        }

        public static MemoryCard CorrelateSD(List<(MemoryCard MemoryCard, int ItemNumber)> memoryCardLogs, LogFile.RecordingRecord recording)
        {
            MemoryCard memoryCard = memoryCardLogs[0].MemoryCard;

            foreach ((MemoryCard currentMemoryCard, int itemNumber) in memoryCardLogs)
            {
                if (itemNumber < recording.ItemNumber)
                {
                    memoryCard = currentMemoryCard;
                }
                else
                {
                    return memoryCard;
                }
            }

            return null;
        }

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.HasBarltLogFile();

            return ValueTask.FromResult(result);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            // Retrieve all information parsed from the log file
            LogFile logFile = (LogFile)information.TargetSupportFiles[LogFile.LogFileKey];
            Sensor sensor = logFile.Sensor;

            string filename = information.FileSystem.Path.GetFileName(information.Path);
            var recordingRecord = logFile.RecordingLogs.Where(x => x.Name.Contains(filename)).FirstOrDefault();

            var serialNumbers = logFile.MemoryCardLogs.Select(x => x.MemoryCard?.SerialNumber);

            // Correlate memory card data
            MemoryCard memoryCard = null;

            if (serialNumbers.Distinct().Count() == 1)
            {
                memoryCard = logFile.MemoryCardLogs.First().MemoryCard;
            }
            else if (recordingRecord != null)
            {
                memoryCard = CorrelateSD(logFile.MemoryCardLogs, recordingRecord);
            }

            if (memoryCard != null)
            {
                recording = recording with
                {
                    MemoryCard = (recording.MemoryCard ?? new MemoryCard()) with
                    {
                        FormatType = recording.MemoryCard?.FormatType ?? memoryCard.FormatType,
                        ManufacturerID = recording.MemoryCard?.ManufacturerID ?? memoryCard.ManufacturerID,
                        OEMID = recording.MemoryCard?.OEMID ?? memoryCard.OEMID,
                        ProductName = recording.MemoryCard?.ProductName ?? memoryCard.ProductName,
                        ProductRevision = recording.MemoryCard?.ProductRevision ?? memoryCard.ProductRevision,
                        SerialNumber = recording.MemoryCard?.SerialNumber ?? memoryCard.SerialNumber,
                        ManufactureDate = recording.MemoryCard?.ManufactureDate ?? memoryCard.ManufactureDate,
                        Speed = recording.MemoryCard?.Speed ?? memoryCard.Speed,
                        Capacity = recording.MemoryCard?.Capacity ?? memoryCard.Capacity,
                        WrCurrentVmin = recording.MemoryCard?.WrCurrentVmin ?? memoryCard.WrCurrentVmin,
                        WrCurrentVmax = recording.MemoryCard?.WrCurrentVmax ?? memoryCard.WrCurrentVmax,
                        WriteBlSize = recording.MemoryCard?.WriteBlSize ?? memoryCard.WriteBlSize,
                        EraseBlSize = recording.MemoryCard?.EraseBlSize ?? memoryCard.EraseBlSize,
                    },
                };
            }

            recording = recording with
            {
                Sensor = (recording.Sensor ?? new Sensor()) with
                {
                    Firmware = recording.Sensor?.Firmware ?? sensor.Firmware,
                    SerialNumber = recording.Sensor?.SerialNumber ?? sensor.SerialNumber,
                    PowerSource = recording.Sensor?.PowerSource ?? sensor.PowerSource,
                    BatteryLevel = recording.Sensor?.BatteryLevel ?? recordingRecord?.BatteryLevel,
                    Voltage = recording.Sensor?.BatteryLevel ?? recordingRecord?.Voltage,
                    Microphones = recording.Sensor?.Microphones ?? recordingRecord?.Microphones,
                },
            };

            return ValueTask.FromResult(recording);
        }
    }
}
