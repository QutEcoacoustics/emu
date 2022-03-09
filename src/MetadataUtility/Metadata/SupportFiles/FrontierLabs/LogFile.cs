// <copyright file="LogFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.SupportFiles.FrontierLabs
{
    using LanguageExt;
    using MetadataUtility.Models;

    public class LogFile : SupportFile
    {
        public const string FrontierLabsLogString = "FRONTIER LABS Bioacoustic Audio Recorder";
        public const string LogFileKey = "Log file";
        public const string FirmwareString = "Firmware:";
        public const int FirmwareTokenOffset = 2;
        public const int SerialNumberLineOffset = 6;
        public const int MetadataOffset = 39;
        public const int RecordingOffset = 45;
        public const string SDCardString = "SD Card :";
        public const string RecordingString = "New recording started:";
        public const string Pattern = "*logfile*.txt";

        public LogFile(string file)
        {
            this.File = file;
        }

        public List<(MemoryCard MemoryCard, int Line)> MemoryCardsLogs { get; } = new List<(MemoryCard MemoryCard, int Line)>();

        public List<(string Recording, int Line)> RecordingLogs { get; } = new List<(string Recording, int Line)>();

        public float Firmware { get; set; }

        public static Fin<bool> HasLogFile(TargetInformation information)
        {
            List<string> logFiles = FindSupportFiles(information, Pattern);

            // If no log files were found, return false
            if (logFiles.Length() == 0)
            {
                return false;
            }

            // If one log file was found, return true and add it to known support files for the target
            if (logFiles.Length() == 1 && IsLogFile(logFiles[0]))
            {
                List<string> knownSupportFilePaths = TargetInformation.KnownSupportFiles.Select(x => x.File).ToList();

                if (knownSupportFilePaths.Contains(logFiles[0]))
                {
                    information.TargetSupportFiles.Add(LogFileKey, TargetInformation.KnownSupportFiles[knownSupportFilePaths.IndexOf(logFiles[0])]);
                }
                else
                {
                    LogFile logFile = new LogFile(logFiles[0]);
                    logFile.ExtractInformation();

                    TargetInformation.KnownSupportFiles.Add(logFile);
                    information.TargetSupportFiles.Add(LogFileKey, logFile);
                }

                return true;
            }

            foreach (string log in logFiles)
            {
                if (IsLogFile(log))
                {
                    LogFile logFile;
                    List<string> knownSupportFilePaths = TargetInformation.KnownSupportFiles.Select(x => x.File).ToList();

                    if (knownSupportFilePaths.Contains(log))
                    {
                        logFile = (LogFile)TargetInformation.KnownSupportFiles[knownSupportFilePaths.IndexOf(log)];
                    }
                    else
                    {
                        logFile = new LogFile(log);
                        logFile.ExtractInformation();
                        TargetInformation.KnownSupportFiles.Add(logFile);
                    }

                    // If more than one log file is found, we must check for the file name in the log file
                    // Won't apply to later firmware versions
                    foreach ((string recording, int line) in logFile.RecordingLogs)
                    {
                        if (recording.Contains(information.FileSystem.Path.GetFileName(information.Path)))
                        {
                            information.TargetSupportFiles.Add(LogFileKey, logFile);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool IsLogFile(string logFile)
        {
            using (StreamReader reader = new StreamReader(logFile))
            {
                for (int i = 0; i < 5; i++)
                {
                    if ((reader.ReadLine() ?? string.Empty).Contains(FrontierLabsLogString))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void ExtractInformation()
        {
            string[] lines = System.IO.File.ReadAllLines(this.File);

            int i = 0;

            for (; i < 8; i++)
            {
                if (lines[i].Contains(FirmwareString))
                {
                    string firmwareString = lines[i].Split(" ")[FirmwareTokenOffset];

                    if (firmwareString[0] == 'V')
                    {
                        this.Firmware = float.Parse(firmwareString.Substring(1));
                    }
                    else
                    {
                        this.Firmware = float.Parse(firmwareString);
                    }
                }
            }

            for (; i < lines.Length(); i++)
            {
                if (lines[i].Contains(SDCardString))
                {
                    MemoryCard memoryCard = new MemoryCard() with
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
                    };

                    this.MemoryCardsLogs.Add((memoryCard, i));
                }
                else if (lines[i].Contains(RecordingString))
                {
                    string recording = lines[i].Substring(RecordingOffset);

                    this.RecordingLogs.Add((recording, i));
                }
            }
        }
    }
}