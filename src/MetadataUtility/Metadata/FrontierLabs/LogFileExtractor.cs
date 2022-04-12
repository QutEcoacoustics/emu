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

        public static MemoryCard CorrelateSD(List<(MemoryCard MemoryCard, int Line)> memoryCardLogs, (string Recording, int Line) recordingLog)
        {
            MemoryCard memoryCard = memoryCardLogs[0].MemoryCard;

            foreach ((MemoryCard currentMemoryCard, int line) in memoryCardLogs)
            {
                if (line < recordingLog.Line)
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
            LogFile logFile = (LogFile)information.TargetSupportFiles[LogFile.LogFileKey];
            MemoryCard memoryCard = null;
            Sensor sensor = logFile.Sensor;

            var serialNumbers = logFile.MemoryCardLogs.Select(x => x.MemoryCard?.SerialNumber);

            if (serialNumbers.Distinct().Count() == 1)
            {
                memoryCard = logFile.MemoryCardLogs.First().MemoryCard;
            }
            else
            {
                string fileName = information.FileSystem.Path.GetFileName(information.Path);
                IEnumerable<string> recordings = logFile.RecordingLogs.Select(x => x.Recording);

                if (recordings.Contains(fileName))
                {
                    memoryCard = CorrelateSD(logFile.MemoryCardLogs, logFile.RecordingLogs[recordings.ToList().IndexOf(fileName)]);
                }
            }

            if (memoryCard != null)
            {
                recording = recording with
                {
                    MemoryCard = memoryCard,
                };
            }

            recording = recording with
            {
                Sensor = (recording.Sensor ?? new Sensor()) with
                {
                    Firmware = sensor.Firmware,
                    SerialNumber = sensor.SerialNumber,
                    PowerSource = sensor.PowerSource,
                },
            };

            return ValueTask.FromResult(recording);
        }
    }
}
