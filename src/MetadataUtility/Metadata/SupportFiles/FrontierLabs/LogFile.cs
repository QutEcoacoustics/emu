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
        public const string SDCardString = "SD Card :";
        public const string RecordingString = "New recording started:";
        public static readonly Regex LogFileRegex = new Regex(@".*logfile.*txt");

        public LogFile(string filePath)
        {
            this.FilePath = filePath;
        }

        public List<(MemoryCard MemoryCard, int Line)> MemoryCardLogs { get; } = new List<(MemoryCard MemoryCard, int Line)>();

        public List<(string Recording, int Line)> RecordingLogs { get; } = new List<(string Recording, int Line)>();

        public float Firmware { get; set; }

        public static void FindLogFile(TargetInformation information, IEnumerable<string> supportFiles)
        {
            IEnumerable<string> logFiles = supportFiles.Where(x => LogFileRegex.IsMatch(x));

            int length = logFiles.Length();

            string first = logFiles.FirstOrDefault();

            // If one log file was found, return true and add it to known support files for the target
            if (length == 1 && IsLogFile(first))
            {
                IEnumerable<string> knownSupportFilePaths = TargetInformation.KnownSupportFiles.Select(x => x.FilePath);

                if (knownSupportFilePaths.Contains(first))
                {
                    information.TargetSupportFiles.Add(LogFileKey, TargetInformation.KnownSupportFiles[knownSupportFilePaths.ToList().IndexOf(first)]);
                }
                else
                {
                    LogFile logFile = new LogFile(first);
                    logFile.ExtractInformation();

                    TargetInformation.KnownSupportFiles.Add(logFile);
                    information.TargetSupportFiles.Add(LogFileKey, logFile);
                }
            }
            else if (length > 1)
            {
                foreach (string log in logFiles)
                {
                    if (IsLogFile(log))
                    {
                        LogFile logFile;
                        IEnumerable<string> knownSupportFilePaths = TargetInformation.KnownSupportFiles.Select(x => x.FilePath);

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
            }

            return;
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
            string[] lines = System.IO.File.ReadAllLines(this.FilePath);

            int i = 0;

            for (; i < 8; i++)
            {
                if (lines[i].Contains(FirmwareString))
                {
                    string firmwareString = lines[i].Split().Where(x => new Regex(@"V?\d+").IsMatch(x)).FirstOrDefault();

                    if (firmwareString != null)
                    {
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
            }

            for (; i < lines.Length(); i++)
            {
                if (lines[i].Contains(SDCardString))
                {
                    MemoryCard memoryCard = new MemoryCard() with
                    {
                        FormatType = lines[++i].Split().Last(),
                        ManufacturerID = uint.Parse(lines[++i].Split().Last()),
                        OEMID = lines[++i].Split().Last(),
                        ProductName = lines[++i].Split().Last(),
                        ProductRevision = float.Parse(lines[++i].Split().Last()),
                        SerialNumber = uint.Parse(lines[++i].Split().Last()),
                        ManufactureDate = lines[++i].Split().Last(),
                        Speed = uint.Parse(lines[++i].Split().Last()),
                        Capacity = uint.Parse(lines[++i].Split().Last()),
                        WrCurrentVmin = uint.Parse(lines[++i].Split().Last()),
                        WrCurrentVmax = uint.Parse(lines[++i].Split().Last()),
                        WriteB1Size = uint.Parse(lines[++i].Split().Last()),
                        EraseB1Size = uint.Parse(lines[++i].Split().Last()),
                    };

                    this.MemoryCardLogs.Add((memoryCard, i));
                }
                else if (lines[i].Contains(RecordingString))
                {
                    string recording = lines[i];

                    this.RecordingLogs.Add((recording, i));
                }
            }
        }
    }
}
