// <copyright file="LogFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.SupportFiles.FrontierLabs
{
    using System.Text.RegularExpressions;
    using LanguageExt;
    using MetadataUtility.Models;

    public class LogFile : SupportFile
    {
        public const string FrontierLabsLogString = "FRONTIER LABS Bioacoustic Audio Recorder";
        public const string LogFileKey = "Frontier Labs Log file";
        public const string FirmwareString = "Firmware:";
        public const string SerialNumberString = "Serial Number:";
        public const string ConfigString = "Config:";
        public const string SDCardString = "SD Card :";
        public const string RecordingString = "New recording started:";
        public static readonly Regex LogFileRegex = new Regex(@".*logfile.*txt");
        public static readonly Regex FirmwareRegex = new Regex(@"V?\d+");

        public LogFile(string filePath)
        {
            this.FilePath = filePath;
        }

        public List<(MemoryCard MemoryCard, int Line)> MemoryCardLogs { get; } = new List<(MemoryCard MemoryCard, int Line)>();

        public List<(string Recording, int Line)> RecordingLogs { get; } = new List<(string Recording, int Line)>();

        public Sensor Sensor { get; set; }

        public static void FindLogFile(TargetInformation information, IEnumerable<string> supportFiles)
        {
            IEnumerable<string> logFiles = supportFiles.Where(x => LogFileRegex.IsMatch(x));

            int length = logFiles.Length();

            string first = logFiles.FirstOrDefault();

            // If one log file was found, return true and add it to known support files for the target
            if (length == 1)
            {
                LogFile logFile = GetLogFile(first);

                information.TargetSupportFiles.Add(LogFileKey, logFile);
            }
            else if (length > 1)
            {
                foreach (string log in logFiles)
                {
                    LogFile logFile = GetLogFile(log);

                    // If more than one log file is found, we must check for the file name in the log file
                    // Won't apply to later firmware versions
                    foreach ((string recording, int line) in logFile.RecordingLogs)
                    {
                        if (recording.Contains(information.FileSystem.Path.GetFileName(information.Path)))
                        {
                            information.TargetSupportFiles.Add(LogFileKey, logFile);
                            return;
                        }
                    }
                }
            }

            return;
        }

        public static LogFile GetLogFile(string log)
        {
            IEnumerable<string> knownSupportFilePaths = TargetInformation.KnownSupportFiles.Select(x => x.FilePath);
            LogFile logFile;

            if (knownSupportFilePaths.Contains(log))
            {
                logFile = (LogFile)TargetInformation.KnownSupportFiles[knownSupportFilePaths.ToList().IndexOf(log)];
            }
            else
            {
                logFile = new LogFile(log);
                logFile.ExtractInformation();
                TargetInformation.KnownSupportFiles.Add(logFile);
            }

            return logFile;
        }

        public override void ExtractInformation()
        {
            int i;
            bool isLogFile = false;
            string line;

            string firmware = null;
            string serialNumber = string.Empty, powerSource = string.Empty;

            using (StreamReader reader = new StreamReader(this.FilePath))
            {
                // Extract information found at the beginning of the log file
                for (i = 0; i < 8; i++)
                {
                    line = reader.ReadLine() ?? string.Empty;

                    if (line.Contains(FrontierLabsLogString))
                    {
                        isLogFile = true;
                    }
                    else if (line.Contains(FirmwareString))
                    {
                        string firmwareString = line.Split().Where(x => FirmwareRegex.IsMatch(x)).FirstOrDefault();

                        firmwareString = firmwareString?.StartsWith("V") ?? false ? firmwareString[1..] : firmwareString;

                        firmware = firmwareString;
                    }
                    else if (line.Contains(SerialNumberString))
                    {
                        serialNumber = line.Split().Last();
                    }
                    else if (line.Contains(ConfigString))
                    {
                        powerSource = line.Split().Last();
                    }
                }

                if (!isLogFile)
                {
                    return;
                }
                else
                {
                    this.Sensor = new Sensor() with
                    {
                        Firmware = firmware,
                        SerialNumber = serialNumber,
                        PowerSource = powerSource,
                    };
                }

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(SDCardString))
                    {
                        try
                        {
                            MemoryCard memoryCard = new MemoryCard() with
                            {
                                FormatType = reader.ReadLine()?.Split().Last(),
                                ManufacturerID = byte.Parse(reader.ReadLine()!.Split().Last()),
                                OEMID = reader.ReadLine()?.Split().Last(),
                                ProductName = reader.ReadLine()?.Split().Last(),
                                ProductRevision = float.Parse(reader.ReadLine()!.Split().Last()),
                                SerialNumber = uint.Parse(reader.ReadLine()!.Split().Last()),
                                ManufactureDate = reader.ReadLine()?.Split().Last(),
                                Speed = uint.Parse(reader.ReadLine()!.Split().Last()),
                                Capacity = uint.Parse(reader.ReadLine()!.Split().Last()),
                                WrCurrentVmin = uint.Parse(reader.ReadLine()!.Split().Last()),
                                WrCurrentVmax = uint.Parse(reader.ReadLine()!.Split().Last()),
                                WriteB1Size = uint.Parse(reader.ReadLine()!.Split().Last()),
                                EraseB1Size = uint.Parse(reader.ReadLine()!.Split().Last()),
                            };

                            this.MemoryCardLogs.Add((memoryCard, i));
                        }
                        catch (ArgumentNullException)
                        {
                            return;
                        }
                    }
                    else if (line.Contains(RecordingString))
                    {
                        string recording = line;

                        this.RecordingLogs.Add((recording, i));
                    }
                }

                return;
            }
        }
    }
}
