// <copyright file="LogFileExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.FrontierLabs
{
    using System.Threading.Tasks;
    using Emu.Metadata.SupportFiles.FrontierLabs;
    using Emu.Models;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using static Emu.Metadata.SupportFiles.FrontierLabs.LogFile;

    public class LogFileExtractor : IMetadataOperation
    {
        private readonly ILogger<LogFileExtractor> logger;

        public LogFileExtractor(ILogger<LogFileExtractor> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Correlates data to its corresponding recording.
        /// Uses timestamps taken from each recording and each data record.
        /// </summary>
        /// <param name="dataRecords">Data from log file.</param>
        /// <param name="recordingRecord">Recording name and timestamp from log file.</param>
        /// <param name="recording">Recording metadata.</param>
        /// <returns>
        /// Data corresponding to the given recording.
        /// Used to correlate sensor, memory card and location data from log file to recordings.
        /// </returns>
        public static T CorrelateRecord<T>(List<DataRecord<T>> dataRecords, RecordingRecord recordingRecord, Recording recording)
        {
            T data = dataRecords.First().Data;

            // If there is one distinct record of this data type, correlation using timestamps isn't needed
            if (dataRecords.Select(s => s.Data).Distinct().Count() == 1)
            {
                return data;
            }

            LocalDateTime recordingTimeStamp;

            if (recordingRecord != null)
            {
                recordingTimeStamp = recordingRecord.TimeStamp;
            }
            else if (recording.LocalStartDate != null)
            {
                recordingTimeStamp = (LocalDateTime)recording.LocalStartDate;
            }
            else
            {
                return default;
            }

            // Correlate data record to recording using timestamps in the log file
            foreach (DataRecord<T> record in dataRecords)
            {
                if (record.TimeStamp > recordingTimeStamp)
                {
                    return data;
                }
                else
                {
                    data = record.Data;
                }
            }

            return data;
        }

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.HasBarltLogFile();

            return ValueTask.FromResult(result);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            LogFile logFile = (LogFile)information.TargetSupportFiles[LogFile.LogFileKey];

            string filename = information.FileSystem.Path.GetFileName(information.Path);
            var recordingRecord = logFile.RecordingLogs.Where(x => x.Name.Contains(filename)).FirstOrDefault();

            Sensor sensor = null;
            MemoryCard memoryCard = null;
            Location location = null;

            sensor = logFile.SensorLogs.Length() == 0 ? null : CorrelateRecord(logFile.SensorLogs, recordingRecord, recording);
            memoryCard = logFile.MemoryCardLogs.Length() == 0 ? null : CorrelateRecord(logFile.MemoryCardLogs, recordingRecord, recording);
            location = logFile.LocationLogs.Length() == 0 ? null : CorrelateRecord(logFile.LocationLogs, recordingRecord, recording);

            recording = recording with
            {
                MemoryCard = recording.MemoryCard == null ? memoryCard : recording.MemoryCard with
                {
                    FormatType = recording.MemoryCard?.FormatType ?? memoryCard?.FormatType,
                    ManufacturerID = recording.MemoryCard?.ManufacturerID ?? memoryCard?.ManufacturerID,
                    OEMID = recording.MemoryCard?.OEMID ?? memoryCard?.OEMID,
                    ProductName = recording.MemoryCard?.ProductName ?? memoryCard?.ProductName,
                    ProductRevision = recording.MemoryCard?.ProductRevision ?? memoryCard?.ProductRevision,
                    SerialNumber = recording.MemoryCard?.SerialNumber ?? memoryCard?.SerialNumber,
                    ManufactureDate = recording.MemoryCard?.ManufactureDate ?? memoryCard?.ManufactureDate,
                    Speed = recording.MemoryCard?.Speed ?? memoryCard?.Speed,
                    Capacity = recording.MemoryCard?.Capacity ?? memoryCard?.Capacity,
                    WrCurrentVmin = recording.MemoryCard?.WrCurrentVmin ?? memoryCard?.WrCurrentVmin,
                    WrCurrentVmax = recording.MemoryCard?.WrCurrentVmax ?? memoryCard?.WrCurrentVmax,
                    WriteBlSize = recording.MemoryCard?.WriteBlSize ?? memoryCard?.WriteBlSize,
                    EraseBlSize = recording.MemoryCard?.EraseBlSize ?? memoryCard?.EraseBlSize,
                },
                Sensor = (recording.Sensor ?? new Sensor()) with
                {
                    Firmware = recording.Sensor?.Firmware ?? sensor?.Firmware,
                    SerialNumber = recording.Sensor?.SerialNumber ?? sensor?.SerialNumber,
                    PowerSource = recording.Sensor?.PowerSource ?? sensor?.PowerSource,
                    BatteryLevel = recording.Sensor?.BatteryLevel ?? recordingRecord?.BatteryLevel,
                    Voltage = recording.Sensor?.BatteryLevel ?? recordingRecord?.Voltage,
                    Microphones = recording.Sensor?.Microphones ?? recordingRecord?.Microphones,
                },
                Location = recording.Location == null ? location : recording.Location with
                {
                    Longitude = recording.Location?.Longitude ?? location?.Longitude,
                    Latitude = recording.Location?.Latitude ?? location?.Latitude,
                },
            };

            return ValueTask.FromResult(recording);
        }
    }
}
