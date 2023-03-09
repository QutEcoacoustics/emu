// <copyright file="LogFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.SupportFiles.FrontierLabs
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Emu.Models;
    using LanguageExt;
    using LanguageExt.ClassInstances;
    using LanguageExt.TypeClasses;
    using NodaTime;
    using NodaTime.Text;
    using static LanguageExt.Prelude;

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
        public static readonly LocalDatePattern MicrophoneBuildDateParser = LocalDatePattern.CreateWithInvariantCulture("dd'/'MM'/'yyyy");
        public static readonly LocalDateTimePattern[] DatePatterns =
        {
            LocalDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
            LocalDateTimePattern.CreateWithInvariantCulture("dd'/'MM'/'yyyy' 'HH':'mm':'ss"),

            // some log files emit single digit days, e.g.
            // 1/01/2010 00:00:22
            LocalDateTimePattern.CreateWithInvariantCulture("d'/'MM'/'yyyy' 'HH':'mm':'ss"),
        };

        public static readonly Regex[] DateMatchers =
        {
            new("\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2}"),

            // some log files emit single digit days, e.g.
            // 1/01/2010 00:00:22
            new("\\d{1,2}/\\d{2}/\\d{4} \\d{2}:\\d{2}:\\d{2}"),
        };

        public static readonly string[] PowerTokens = new[] { "Ext-power", "Solar-power" };
        public static readonly Regex LogFileRegex = new(@".*logfile.*txt");
        public static readonly Regex FirmwareRegex = new(@"V?\d+");
        public static readonly Regex BatteryParsingRegex = new(@"[%V()]");
        private static readonly Regex GainRegex = new("(\\d+)dB on channel ([A-Z])");
        private static readonly Regex GainRegexOlder = new("with a gain of (\\d+)dB with");

        // matches: Microphone: 001958 "STD AUDIO MIC" ( 05/07/2019 )
        private static readonly Regex MicrophoneLineRegex = new(@": (\d+) ""(.*)"" \( ([\d/]+) \)");

        public LogFile(string filePath)
        {
            this.FilePath = filePath;
        }

        public List<RecordingRecord> RecordingLogs { get; set; } = new List<RecordingRecord>();

        public List<DataRecord<MemoryCard>> MemoryCardLogs { get; set; } = new List<DataRecord<MemoryCard>>();

        public List<DataRecord<Sensor>> SensorLogs { get; set; } = new List<DataRecord<Sensor>>();

        public List<DataRecord<Location>> LocationLogs { get; set; } = new List<DataRecord<Location>>();

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
        /// Finds the next date stamp in the log file.
        /// </summary>
        /// <param name="reader">Log file stream reader.</param>
        /// <returns>
        /// The date string.
        /// </returns>
        public static string FindNextDate(StreamReader reader)
        {
            string line, date;

            while ((line = reader.ReadLine()) != null)
            {
                foreach (Regex matcher in DateMatchers)
                {
                    date = string.Join(" ", line.Split(" ").Take(2));

                    if (matcher.IsMatch(date))
                    {
                        return date;
                    }
                }
            }

            throw new Exception("Can't find date stamp in log file");
        }

        public static LocalDateTime ParseDate(string dateTime)
        {
            foreach (LocalDateTimePattern datePattern in DatePatterns)
            {
                if (datePattern.Parse(dateTime) is { Success: true } d)
                {
                    return d.Value;
                }
            }

            throw new UnparsableValueException($"Can't parse date {dateTime}");
        }

        /// <summary>
        /// Parses a memory card from a log file.
        /// </summary>
        /// <param name="reader">Log file stream reader.</param>
        /// <param name="line">Line from log file.</param>
        /// <returns>
        /// A parsed memory card object.
        /// </returns>
        public static DataRecord<MemoryCard> MemoryCardParser(StreamReader reader, string line)
        {
            string dateTime = line.Split(SDCardString).First().Trim();
            LocalDateTime timeStamp = ParseDate(dateTime);

            // determine which date format is used in this log
            var dateFormat = DateMatchers.Single(regex => regex.Match(dateTime).Success);
            var indentMatch = new Regex($"{dateFormat}   ");

            // Formats:
            // - Version 3.00, note the weird double line (looks like a one time fault), and the trailing format type
            //
            // 02/10/2022 07:53:56   Manufacturer ID  3
            // 02/10/2022 07:53:56   OEM ID           SD
            // 02/10/2022 07:53:56   Product Name     ACLCF
            // 02/10/2022 07:53:56   Product Revision 8.0
            // 02/10/2022 07:53:56   Serial Number    15435216
            // 02/10/2022 07:53:56   Manufacture Date 2017 / 03
            // 02/10/2022 07:53:56   Speed            50
            // 02/10/2022 07:53:56   Capacity         124868608
            // 02/10/2022 07:53:56   Wr Current Vmin  5
            // 02/10/2022 07:53:56   Wr Current Vmax  502/10/2022 07:53:56   Write Bl Size    512
            // 02/10/2022 07:53:56   Erase Bl Size    65536
            // 02/10/2022 07:53:56 SD Card format type exFAT

            // - Version 3.30
            // 2022-03-31 09:48:39 SD Card :
            // 2022-03-31 09:48:39   Format type      exFAT
            // 2022-03-31 09:48:39   Manufacturer ID  3
            // 2022-03-31 09:48:39   OEM ID           SD
            // 2022-03-31 09:48:39   Product Name     SD128
            // 2022-03-31 09:48:39   Product Revision 8.5
            // 2022-03-31 09:48:39   Serial Number    8041328
            // 2022-03-31 09:48:39   Manufacture Date 2020/08
            // 2022-03-31 09:48:39   Speed            50
            // 2022-03-31 09:48:39   Capacity         124868608
            // 2022-03-31 09:48:39   Wr Current Vmin  5
            // 2022-03-31 09:48:39   Wr Current Vmax  5
            // 2022-03-31 09:48:39   Write Bl Size    512
            // 2022-03-31 09:48:39   Erase Bl Size    65536

            // read lines until deindent detected
            // also remove date prefixes and wrap lines that have a faulty line break;
            var lines = new List<string>(20);
            while (true)
            {
                line = reader.ReadLine();
                var shouldContinue = CleanLine(line);

                if (!shouldContinue)
                {
                    break;
                }
            }

            var format = Parse("Format type");
            var manufacturer = parseByte(Parse("Manufacturer ID"));
            var oem = Parse("OEM ID");
            var product = Parse("Product Name");
            var revision = parseFloat(Parse("Product Revision"));
            var serial = parseUInt(Parse("Serial Number"));
            var date = Parse("Manufacture Date")?.Replace('/', '-');
            var speed = parseUInt(Parse("Speed")).Map(x => x * MemoryCard.MegabyteConversion);
            var capacity = parseULong(Parse("Capacity")).Map(x => x * MemoryCard.KilobyteConversion);
            var vmin = parseUInt(Parse("Wr Current Vmin"));
            var vmax = parseUInt(Parse("Wr Current Vmax"));
            var write = parseUInt(Parse("Write Bl Size"));
            var erase = parseUInt(Parse("Erase Bl Size"));

            // the format follows the block in the v3.00 format
            if (line?.Contains("SD Card format type") ?? false)
            {
                format = line?.Split(' ').Last();
            }

            MemoryCard memoryCard = new MemoryCard() with
            {
                FormatType = format,
                ManufacturerID = manufacturer.ToNullable(),
                OEMID = oem,
                ProductName = product,
                ProductRevision = revision.ToNullable(),
                SerialNumber = serial.ToNullable(),
                ManufactureDate = date,
                Speed = speed.ToNullable(),
                Capacity = capacity.ToNullable(),
                WrCurrentVmin = vmin.ToNullable(),
                WrCurrentVmax = vmax.ToNullable(),
                WriteBlSize = write.ToNullable(),
                EraseBlSize = erase.ToNullable(),
            };

            return new DataRecord<MemoryCard>(memoryCard, timeStamp);

            string Parse(string key)
            {
                foreach (var line in lines)
                {
                    if (line.StartsWith(key))
                    {
                        return line[key.Length..].Trim();
                    }
                }

                return null;
            }

            bool CleanLine(string line)
            {
                if (line is null)
                {
                    return false;
                }

                var match = indentMatch.Match(line);

                if (match.Success)
                {
                    var matchEnd = match.Index + match.Length;
                    var trimmed = line[matchEnd..];

                    // check if there's a date stamp within the line,
                    // and split the line if so
                    var secondDate = dateFormat.Match(trimmed);
                    if (secondDate.Success)
                    {
                        var secondMatchEnd = secondDate.Index + secondDate.Length;
                        var secondLine = trimmed[secondMatchEnd..].Trim();

                        lines.Add(secondLine);

                        trimmed = trimmed[0..secondDate.Index];
                    }

                    lines.Add(trimmed);
                }

                return match.Success;
            }
        }

        /// <summary>
        /// Parses a location from a log file.
        /// Example location format: <c>[+43.70588-065.95160]</c>.
        /// </summary>
        /// <param name="line">Line from log file.</param>
        /// <returns>
        /// A parsed location object.
        /// </returns>
        public static DataRecord<Location> LocationParser(string line)
        {
            double? longitude, latitude;

            var locationValues = line.Split(LocationString);

            string locationData = locationValues.Last();
            string dateTime = locationValues.First().Trim();

            LocalDateTime timeStamp = ParseDate(dateTime);

            // remove []
            locationData = locationData.Replace("[", string.Empty).Replace("]", string.Empty).Split(" ", StringSplitOptions.RemoveEmptyEntries).First();

            // Find index dividing lat and lon
            int latLonDividingIndex = locationData.IndexOfAny(new char[] { '+', '-' }, 1);

            // Parse lat and lon
            latitude = double.Parse(locationData.Substring(0, latLonDividingIndex));
            longitude = double.Parse(locationData.Substring(latLonDividingIndex));

            Location location = new Location() with
            {
                Latitude = latitude,
                Longitude = longitude,
            };

            return new DataRecord<Location>(location, timeStamp);
        }

        /// <summary>
        /// Parses battery data from a log file.
        /// Example battery data format: <c>94% ( 4.13 V )</c>.
        /// </summary>
        /// <param name="batteryData">The unparsed battery data.</param>
        /// <returns>
        /// Parsed battery data.
        /// </returns>
        public static (double? BatteryLevel, double? Voltage) BatteryParser(string batteryData)
        {
            batteryData = BatteryParsingRegex.Replace(batteryData, string.Empty);
            string[] batteryValues = batteryData.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            double? batteryLevel = double.Parse(batteryValues[0]) / 100;
            double? batteryVoltage = double.Parse(batteryValues[1]);

            return (batteryLevel, batteryVoltage);
        }

        /// <summary>
        /// Parses a microphone from a log file.
        /// Example microphone data format: <c>Ch A: 006277 "STD AUDIO MIC" ( 16/01/2020 )</c>.
        /// </summary>
        /// <param name="microphoneData">The unparsed microphone data.</param>
        /// <param name="arrowLine">The log line containing the `-->` token.</param>
        /// <returns>
        /// A parsed microphone object.
        /// </returns>
        public static Microphone MicrophoneParser(string microphoneData, string arrowLine)
        {
            // Microphone data may be unknown
            if (microphoneData.Contains("Unknown"))
            {
                return null;
            }

            char? channelName = null;
            int? channel = null;

            // Parse channel data if it exists
            if (microphoneData.Contains("Ch"))
            {
                channelName = microphoneData.Split(':').First().ToArray().Last();
                channel = (int)channelName - 64 - 1;
            }

            // Split string into individual microphone values
            string[] micValues = microphoneData.Split("\"").Select(x => x.Trim()).ToArray();

            // Parse each value
            string uid;
            string type;
            string stringBuildDate;

            var microphoneLineMatch = MicrophoneLineRegex.Match(microphoneData);
            if (microphoneLineMatch.Success)
            {
                uid = microphoneLineMatch.Groups[1].Value;
                type = microphoneLineMatch.Groups[2].Value;
                stringBuildDate = microphoneLineMatch.Groups[3].Value;
            }
            else
            {
                uid = micValues[0].Split(" ").Last();
                type = micValues[1];
                stringBuildDate = micValues[2]
                    .Replace("(", string.Empty).Replace(")", string.Empty)
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries).First();
            }

            var buildDate = MicrophoneBuildDateParser.Parse(stringBuildDate).Value;

            double? gain = null;
            if (arrowLine is not null)
            {
                // gain is contained in the arrow line for some versions
                var matches = GainRegex.Matches(arrowLine);
                if (matches.Any())
                {
                    foreach (Match match in matches)
                    {
                        if (match.Success)
                        {
                            var gainText = match.Groups[1].Value;
                            var channelText = match.Groups[2].Value;

                            if (channelName.ToString() == channelText)
                            {
                                gain = double.Parse(gainText);
                            }
                        }
                    }
                }
                else
                {
                    var oldGainMatch = GainRegexOlder.Match(arrowLine);
                    if (oldGainMatch.Success)
                    {
                        gain = double.Parse(oldGainMatch.Groups[1].Value);

                        // this case only happens with one mic
                        channelName = 'A';
                        channel = 0;
                    }
                }
            }

            return new Microphone() with
            {
                Channel = channel,
                ChannelName = channelName?.ToString(),
                UID = uid,
                Type = type,
                BuildDate = buildDate,
                Gain = gain,
            };
        }

        /// <summary>
        /// Parses data from a log file header.
        /// </summary>
        /// <param name="reader">Log file stream reader.</param>
        /// <returns>
        /// A parsed file header record.
        /// </returns>
        public static DataRecord<Sensor> SensorParser(StreamReader reader)
        {
            string line, firmware = null, serialNumber = null, powerSource = null;

            while ((line = reader.ReadLine()) != null && !line.Contains(EndSection))
            {
                if (line.Contains(FirmwareString))
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
                    else if (tokens.Length > 2)
                    {
                        powerSource = tokens.Last();
                    }
                }
            }

            string dateTime = FindNextDate(reader);
            LocalDateTime timeStamp = ParseDate(dateTime);

            Sensor sensor = new Sensor() with
            {
                Firmware = firmware,
                SerialNumber = serialNumber,
                PowerSource = powerSource,
            };

            return new DataRecord<Sensor>(sensor, timeStamp);
        }

        /// <summary>
        /// Parses data specific to one recording from a log file.
        /// </summary>
        /// <param name="reader">Log file stream reader.</param>
        /// <param name="line">Line from log file.</param>
        /// <returns>
        /// A parsed file header record.
        /// </returns>
        public static RecordingRecord RecordingParser(StreamReader reader, string line)
        {
            var recordingValues = line.Split("|");

            string dateTime = recordingValues[0].Trim();
            LocalDateTime timeStamp = ParseDate(dateTime);

            string name = string.Join("|", recordingValues[1..]);

            (double?, double?)? batteryData = null;
            List<Microphone> microphones = null;
            string arrowString = null;

            while ((line = reader.ReadLine()) != null && !line.Contains(EndSection))
            {
                if (line.Contains("-->"))
                {
                    // extra information sometimes contained in this string
                    arrowString = line;
                }

                if (line.Contains(BatteryString))
                {
                    batteryData = BatteryParser(line.Split(BatteryString).Last());
                }

                if (line.Contains(MicrophoneString))
                {
                    var microphone = MicrophoneParser(line.Split(MicrophoneString).Last(), arrowString);

                    if (microphone != null)
                    {
                        microphones ??= new List<Microphone>();

                        microphones.Add(microphone);
                    }
                }
            }

            return new RecordingRecord(name, batteryData?.Item1, batteryData?.Item2, microphones?.ToArray(), timeStamp);
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
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(FrontierLabsLogString))
                    {
                        this.SensorLogs.Add(SensorParser(reader));
                    }
                    else if (line.Contains(SDCardString))
                    {
                        this.MemoryCardLogs.Add(MemoryCardParser(reader, line));
                    }
                    else if (line.Contains(RecordingString))
                    {
                        this.RecordingLogs.Add(RecordingParser(reader, line));
                    }
                    else if (line.Contains(LocationString))
                    {
                        this.LocationLogs.Add(LocationParser(line));
                    }
                }
            }

            this.SensorLogs = this.SensorLogs.OrderBy(log => log.TimeStamp).ToList();
            this.MemoryCardLogs = this.MemoryCardLogs.OrderBy(log => log.TimeStamp).ToList();
            this.RecordingLogs = this.RecordingLogs.OrderBy(log => log.TimeStamp).ToList();
            this.LocationLogs = this.LocationLogs.OrderBy(log => log.TimeStamp).ToList();

            return true;
        }

        public record RecordingRecord(string Name, double? BatteryLevel, double? Voltage, Microphone[] Microphones, LocalDateTime TimeStamp);

        public record DataRecord<T>(T Data, LocalDateTime TimeStamp);
    }
}
