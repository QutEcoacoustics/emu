// <copyright file="LogFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.SupportFiles.FrontierLabs
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Emu.Models;
    using LanguageExt;
    using NodaTime;
    using NodaTime.Text;

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
        public const string EndRecordingSection = "--------";
        public static readonly string[] PowerTokens = new[] { "Ext-power", "Solar-power" };
        public static readonly Regex LogFileRegex = new(@".*logfile.*txt");
        public static readonly Regex FirmwareRegex = new(@"V?\d+");
        public static readonly Regex BatteryParsingRegex = new(@"[%V()]");

        public LogFile(string filePath)
        {
            this.FilePath = filePath;
        }

        public List<(MemoryCard MemoryCard, int Line)> MemoryCardLogs { get; } = new List<(MemoryCard MemoryCard, int Line)>();

        public List<RecordingRecord> RecordingLogs { get; } = new List<RecordingRecord>();

        public Sensor Sensor { get; set; }

        public Location Location { get; set; }

        /// <summary>
        /// Searches each potential support file for a log file that correlates with the given recording.
        /// </summary>
        /// <param name="information">The target recording information.</param>
        /// <param name="supportFiles">List of all potential support files for this target.</param>
        public static void FindLogFile(TargetInformation information, IEnumerable<string> supportFiles)
        {
            IEnumerable<string> logFiles = supportFiles.Where(x => LogFileRegex.IsMatch(x));

            int length = logFiles.Length();

            string first = logFiles.FirstOrDefault();

            LogFile logFile = null;

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

            // If log file is found, correlate it to the target
            if (logFile != null)
            {
                information.TargetSupportFiles.Add(LogFileKey, logFile);
            }
        }

        /// <summary>
        /// Retrieves log file information in one of two ways.
        /// If the given log file has already been cached, find and return it.
        /// If this is an unseen log file, parse all of it's data and return it.
        /// </summary>
        /// <param name="log">The log file name.</param>
        /// <returns>
        /// A parsed log file object.
        /// </returns>
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

        /// <summary>
        /// Parses a memory card from a log file.
        /// </summary>
        /// <param name="reader">Log file stream reader.</param>
        /// <returns>
        /// A parsed memory card object.
        /// </returns>
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
                Capacity = ulong.Parse(reader.ReadLine()!.Split().Last()),
                WrCurrentVmin = uint.Parse(reader.ReadLine()!.Split().Last()),
                WrCurrentVmax = uint.Parse(reader.ReadLine()!.Split().Last()),
                WriteBlSize = uint.Parse(reader.ReadLine()!.Split().Last()),
                EraseBlSize = uint.Parse(reader.ReadLine()!.Split().Last()),
            };
        }

        /// <summary>
        /// Parses a location from a log file.
        /// Example location format: <c>[+43.70588-065.95160]</c>.
        /// </summary>
        /// <param name="value">The unparsed location value.</param>
        /// <returns>
        /// A parsed location object.
        /// </returns>
        public static Location LocationParser(string value)
        {
            double? longitude, latitude;

            // remove []
            value = value.Replace("[", string.Empty).Replace("]", string.Empty).Split(" ", StringSplitOptions.RemoveEmptyEntries).First();

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

        /// <summary>
        /// Parses battery data from a log file.
        /// Example battery data format: <c>94% ( 4.13 V )</c>.
        /// </summary>
        /// <param name="value">The unparsed battery data.</param>
        /// <returns>
        /// Parsed battery data.
        /// </returns>
        public static (double? BatteryLevel, double? Voltage) BatteryParser(string value)
        {
            value = BatteryParsingRegex.Replace(value, string.Empty);
            string[] batteryValues = value.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            double? batteryLevel = double.Parse(batteryValues[0]) / 100;
            double? batteryVoltage = double.Parse(batteryValues[1]);

            return (batteryLevel, batteryVoltage);
        }

        /// <summary>
        /// Parses a microphone from a log file.
        /// Example microphone data format: <c>Ch A: 006277 "STD AUDIO MIC" ( 16/01/2020 )</c>.
        /// </summary>
        /// <param name="value">The unparsed microphone value.</param>
        /// <returns>
        /// A parsed microphone object.
        /// </returns>
        public static Microphone MicrophoneParser(string value)
        {
            // Microphone data may be unknown
            if (value.Contains("Unknown"))
            {
                return null;
            }

            char? channelName = null;
            int? channel = null;

            // Parse channel data if it exists
            if (value.Contains("Ch"))
            {
                channelName = value.Split(':').First().ToArray().Last();
                channel = (int)channelName - 64;
            }

            // Split string into individual microphone values
            string[] micValues = value.Split("\"").Select(x => x.Trim()).ToArray();

            // Parse each value
            string uid = micValues[0].Split(" ").Last();
            string type = micValues[1];
            string stringBuildDate = micValues[2].Replace("(", string.Empty).Replace(")", string.Empty).Split(" ", StringSplitOptions.RemoveEmptyEntries).First();
            LocalDate buildDate = LocalDatePattern.CreateWithInvariantCulture("dd'/'MM'/'yyyy")
                .Parse(stringBuildDate).Value;

            return new Microphone() with
            {
                Channel = channel,
                ChannelName = channelName?.ToString(),
                UID = uid,
                Type = type,
                BuildDate = buildDate,
            };
        }

        /// <summary>
        /// Parses data from a log file header.
        /// </summary>
        /// <param name="reader">Log file stream reader.</param>
        /// <returns>
        /// A parsed file header record.
        /// </returns>
        public static FileHeader HeaderParser(StreamReader reader)
        {
            string line, firmware = null, serialNumber = null, powerSource = null;
            bool isLogFile = false;

            // Header data will be in first 8 lines of a log file
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
                    var tokens = line.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    // allow for more specific extraction
                    var matchedKnownTokens = tokens.Filter(t => PowerTokens.Contains(t));
                    if (matchedKnownTokens.Any())
                    {
                        powerSource = string.Join(", ", matchedKnownTokens);
                    }
                    else
                    {
                        powerSource = tokens.Last();
                    }
                }
            }

            return new FileHeader(isLogFile, firmware, serialNumber, powerSource);
        }

        /// <summary>
        /// Parses data specific to one recording from a log file.
        /// </summary>
        /// <param name="reader">Log file stream reader.</param>
        /// <param name="name">Recording name.</param>
        /// <param name="itemNumber">Item number of this recording relative to other objects in the log file.</param>
        /// <returns>
        /// A parsed file header record.
        /// </returns>
        public static RecordingRecord RecordingParser(StreamReader reader, string name, int itemNumber)
        {
            string line;

            (double?, double?)? batteryData = null;
            List<Microphone> microphones = null;

            while ((line = reader.ReadLine()) != null && !line.Contains(EndRecordingSection))
            {
                if (line.Contains(BatteryString))
                {
                    batteryData = BatteryParser(line.Split(BatteryString).Last());
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

        /// <summary>
        /// Extracts all information from a log file.
        /// </summary>
        /// <returns>
        /// A boolean representing whether the data extraction was succesful.
        /// </returns>
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
