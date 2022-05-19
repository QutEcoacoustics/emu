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
        public const string LocationString = "GPS position lock acquired [";
        public const string BatteryString = "Battery:";
        public const string MicrophoneString = "Microphone";
        public const string EndSection = "--------";
        public static readonly Regex LogFileRegex = new Regex(@".*logfile.*txt");
        public static readonly Regex FirmwareRegex = new Regex(@"V?\d+");

        public LogFile(string filePath)
        {
            this.FilePath = filePath;
        }

        public List<(MemoryCard MemoryCard, int Line)> MemoryCardLogs { get; } = new List<(MemoryCard MemoryCard, int Line)>();

        public List<RecordingRecord> RecordingLogs { get; } = new List<RecordingRecord>();

        public Sensor Sensor { get; set; }

        public Location Location { get; set; }

        public static void FindLogFile(TargetInformation information, IEnumerable<string> supportFiles)
        {
            IEnumerable<string> logFiles = supportFiles.Where(x => LogFileRegex.IsMatch(x));

            int length = logFiles.Length();

            string first = logFiles.FirstOrDefault();

            LogFile logFile = null;

            // If one log file was found, return true and add it to known support files for the target
            if (length == 1)
            {
                logFile = GetLogFile(first);
            }
            else if (length > 1)
            {
                foreach (string log in logFiles)
                {
                    var file = GetLogFile(log);

                    if (file == null)
                    {
                        continue;
                    }

                    // If more than one log file is found, we must check for the file name in the log file
                    // Won't apply to later firmware versions
                    foreach (RecordingRecord record in file.RecordingLogs)
                    {
                        if (record.Name.Contains(information.FileSystem.Path.GetFileName(information.Path)))
                        {
                            logFile = file;
                            break;
                        }
                    }
                }
            }

            if (logFile != null)
            {
                information.TargetSupportFiles.Add(LogFileKey, logFile);
            }
        }

        public static LogFile GetLogFile(string log)
        {
            IEnumerable<string> knownSupportFilePaths = TargetInformation.KnownSupportFiles.Select(x => x.FilePath);

            if (knownSupportFilePaths.Contains(log))
            {
                var file = TargetInformation.KnownSupportFiles[knownSupportFilePaths.ToList().IndexOf(log)];

                if (file is LogFile)
                {
                    return (LogFile)file;
                }
            }
            else
            {
                var file = new LogFile(log);

                if (file.ExtractInformation())
                {
                    TargetInformation.KnownSupportFiles.Add(file);
                    return file;
                }
            }

            return null;
        }

        public static MemoryCard MemoryCardParser(StreamReader reader)
        {
            return new MemoryCard() with
            {
                FormatType = reader.ReadLine()?.Split().Last(),
                ManufacturerID = byte.Parse(reader.ReadLine()!.Split().Last()),
                OEMID = reader.ReadLine()?.Split().Last(),
                ProductName = reader.ReadLine()?.Split().Last(),
                ProductRevision = float.Parse(reader.ReadLine()!.Split().Last()),
                SerialNumber = uint.Parse(reader.ReadLine()!.Split().Last()),
                ManufactureDate = reader.ReadLine()?.Split().Last().Replace('/', '-'),
                Speed = uint.Parse(reader.ReadLine()!.Split().Last()),
                Capacity = uint.Parse(reader.ReadLine()!.Split().Last()),
                WrCurrentVmin = uint.Parse(reader.ReadLine()!.Split().Last()),
                WrCurrentVmax = uint.Parse(reader.ReadLine()!.Split().Last()),
                WriteBlSize = uint.Parse(reader.ReadLine()!.Split().Last()),
                EraseBlSize = uint.Parse(reader.ReadLine()!.Split().Last()),
            };
        }

        public static Location LocationParser(string value)
        {
            double? longitude, latitude;

            value = NumericParser(value);

            // Find index dividing lat and lon
            int latLonDividingIndex = value.IndexOfAny(new char[] { '+', '-' }, 1);

            // Parse lat and lon
            latitude = double.Parse(value.Substring(0, latLonDividingIndex));
            longitude = double.Parse(value.Substring(latLonDividingIndex));

            return new Location() with
            {
                Latitude = latitude,
                Longitude = longitude,
            };
        }

        public static (double? BatteryLevel, double? Voltage) BatteryParser(string value)
        {
            string[] batteryValues = value.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            double? batteryLevel = double.Parse(NumericParser(batteryValues[0])) / 100;
            double? batteryVoltage = double.Parse(NumericParser(batteryValues[1]));

            return (batteryLevel, batteryVoltage);
        }

        public static Microphone MicrophoneParser(string value)
        {
            if (value.Contains("Unknown"))
            {
                return null;
            }

            char? channelName = null;
            int? channel = null;

            if (value.Contains("Ch"))
            {
                channelName = value.Split(':').First().ToArray().Last();
                channel = (int)channelName - 64;
            }

            // Split string into individual microphone values
            string[] micValues = value.Split("\"").Select(x => x.Trim()).ToArray();

            string uid = micValues[0].Split(" ").Last();
            string type = micValues[1];
            string buildDate = string.Join(
                '-',
                new string(micValues[2].Where(c => !(new char[] { ' ', '(', ')' }).Contains(c)).ToArray())
                .Split('/').Reverse().ToArray());

            return new Microphone() with
            {
                Channel = channel,
                ChannelName = channelName?.ToString(),
                UID = uid,
                Type = type,
                BuildDate = buildDate,
            };
        }

        public static string NumericParser(string value) => new string(value.Where(c => char.IsDigit(c) || (new char[] { '.', '-', '+' }).Contains(c)).ToArray());

        public static FileHeader HeaderParser(StreamReader reader)
        {
            string line, firmware = null, serialNumber = null, powerSource = null;
            bool isLogFile = false;

            for (int i = 0; i < 8; i++)
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

            return new FileHeader(isLogFile, firmware, serialNumber, powerSource);
        }

        public static RecordingRecord RecordingParser(StreamReader reader, string name, int itemNumber)
        {
            string line;

            (double?, double?)? batteryData = null;
            List<Microphone> microphones = null;

            while ((line = reader.ReadLine()) != null && !line.Contains(EndSection))
            {
                if (line.Contains(BatteryString))
                {
                    batteryData = BatteryParser(new string(line.Split(BatteryString).Last().Where(c => c != '(' && c != ')').ToArray()));
                }

                if (line.Contains(MicrophoneString))
                {
                    var microphone = MicrophoneParser(line.Split(MicrophoneString).Last());

                    if (microphone != null)
                    {
                        if (microphones == null)
                        {
                            microphones = new List<Microphone>();
                        }

                        microphones.Add(microphone);
                    }
                }
            }

            return new RecordingRecord(name, itemNumber, batteryData?.Item1, batteryData?.Item2, microphones?.ToArray());
        }

        public override bool ExtractInformation()
        {
            using (StreamReader reader = new StreamReader(this.FilePath))
            {
                var headerData = HeaderParser(reader);

                if (!headerData.IsLogFile)
                {
                    return false;
                }

                this.Sensor = new Sensor() with
                {
                    Firmware = headerData.Firmware,
                    SerialNumber = headerData.SerialNumber,
                    PowerSource = headerData.PowerSource,
                };

                string line;
                int itemNumber = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(SDCardString))
                    {
                        this.MemoryCardLogs.Add((MemoryCardParser(reader), itemNumber));
                        itemNumber++;
                    }
                    else if (line.Contains(RecordingString))
                    {
                        this.RecordingLogs.Add(RecordingParser(reader, line, itemNumber));
                        itemNumber++;
                    }
                    else if (line.Contains(LocationString))
                    {
                        this.Location = LocationParser(line.Split(LocationString).Last());
                    }
                }
            }

            return true;
        }

        public record RecordingRecord(string Name, int ItemNumber, double? BatteryLevel, double? Voltage, Microphone[] Microphones);

        public record FileHeader(bool IsLogFile, string Firmware, string SerialNumber, string PowerSource);
    }
}
