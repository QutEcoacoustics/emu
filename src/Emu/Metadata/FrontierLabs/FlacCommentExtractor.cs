// <copyright file="FlacCommentExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.FrontierLabs
{
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Audio.Vendors;
    using Emu.Models;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using MoreLinq;
    using NodaTime;

    public class FlacCommentExtractor : IRawMetadataOperation
    {
        private readonly ILogger<FlacCommentExtractor> logger;

        public FlacCommentExtractor(ILogger<FlacCommentExtractor> logger)
        {
            this.logger = logger;
        }

        public string Name => "FL_FLAC_COMMENTS";

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.IsFlacFile() && information.HasFrontierLabsVorbisComment();

            return ValueTask.FromResult(result);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var tryComments = Flac.ExtractComments(information.FileStream);

            if (tryComments.IsSucc)
            {
                Dictionary<string, string> comments = (Dictionary<string, string>)tryComments;

                Location location = (Location)this.ParseComment(FrontierLabs.LocationCommentKey, comments);
                MemoryCard card = new SdCardCid((string)this.ParseComment(FrontierLabs.SdCidCommentKey, comments)).ExtractSdInfo().IfFail(null);

                List<Microphone> microphones = new List<Microphone>();
                int micNumber = 1;

                // Extract all microphone information
                while (comments.ContainsKey(FrontierLabs.MicrophoneTypeCommentKey + micNumber))
                {
                    // FL uses channel A, B to refer to microphones 0 and 1 respectively
                    // Converts to channel name using ASCII value offset of 64
                    string channelName = ((char)(micNumber + 64)).ToString();

                    Microphone microphone = new Microphone() with
                    {
                        Type = (string)this.ParseComment(FrontierLabs.MicrophoneTypeCommentKey + micNumber, comments),
                        UID = (string)this.ParseComment(FrontierLabs.MicrophoneUIDCommentKey + micNumber, comments),
                        BuildDate = (LocalDate?)this.ParseComment(FrontierLabs.MicrophoneBuildDateCommentKey + micNumber, comments),
                        Gain = (double?)this.ParseComment(FrontierLabs.MicrophoneGainCommentKey + micNumber, comments),

                        // 0 based channels
                        Channel = micNumber - 1,
                        ChannelName = channelName,
                    };

                    if (microphone.Type != null)
                    {
                        microphones.Add(microphone);
                    }

                    micNumber++;
                }

                // Extract battery related values
                // Two batter values are extracted from the same comment as a tuple
                var batteryValues = ((double?, double?)?)this.ParseComment(FrontierLabs.BatteryLevelCommentKey, comments);
                double? batteryLevel = null, voltage = null;

                if (batteryValues != null)
                {
                    batteryLevel = batteryValues?.Item1;

                    batteryLevel = batteryLevel == null ? null : batteryLevel / 100;

                    voltage = batteryValues?.Item2;
                }

                // Update recording information with parsed comments
                recording = recording with
                {
                    Sensor = (recording.Sensor ?? new Sensor()) with
                    {
                        Make = Vendor.FrontierLabs.ToNiceName(),
                        Firmware = recording.Sensor?.Firmware ?? (string)this.ParseComment(FrontierLabs.FirmwareCommentKey, comments),
                        BatteryLevel = recording.Sensor?.BatteryLevel ?? batteryLevel,
                        Voltage = recording.Sensor?.Voltage ?? voltage,
                        LastTimeSync = recording.Sensor?.LastTimeSync ?? (OffsetDateTime?)this.ParseComment(FrontierLabs.LastSyncCommentKey, comments),
                        SerialNumber = recording.Sensor?.SerialNumber ?? (string)this.ParseComment(FrontierLabs.SensorIdCommentKey, comments),
                        Microphones = recording.Sensor?.Microphones ?? new Microphone[microphones.Length()],
                    },

                    // TODO: 3.08 firmware includes a local time (no offset). In this case we just discard it here
                    // TrueStartDate is the ideal field to update here, but if we're missing the date in the filename, update normal start date as well
                    StartDate = recording.StartDate ?? this.ParseComment(FrontierLabs.RecordingStartCommentKey, comments) as OffsetDateTime?,

                    // From FL:
                    // The info in the FLAC header is when the first buffer is being written to the file, so it’s the “more accurate” figure.
                    // The filename timestamp is the basically what the schedule says the start time should be.
                    // People said they preferred that as it made more sense to them and they didn’t care too much about the “absolute time”
                    // or seconds differences since critters aren’t much for schedules and the clocks can drift over a long deployment
                    // anyway and the.The AAO boxes update their clocks once a day but everything is still +/ -1 second accuracy
                    // anyway(unless you’re running the acoustic localisation firmware).
                    TrueStartDate = recording.StartDate ?? this.ParseComment(FrontierLabs.RecordingStartCommentKey, comments) as OffsetDateTime?,
                    TrueEndDate = recording.TrueEndDate ?? this.ParseComment(FrontierLabs.RecordingEndCommentKey, comments) as OffsetDateTime?,
                    Location = recording.Location ?? location,
                    MemoryCard = recording.MemoryCard is null ? card : recording.MemoryCard with
                    {
                        ManufacturerID = card?.ManufacturerID,
                        OEMID = card?.OEMID,
                        ProductName = card?.ProductName,
                        ProductRevision = card?.ProductRevision,
                        SerialNumber = card?.SerialNumber,
                        ManufactureDate = card?.ManufactureDate,
                    },
                };

                // Update recording microphone information
                for (int i = 0; i < recording.Sensor.Microphones.Length(); i++)
                {
                    recording.Sensor.Microphones[i] = recording.Sensor.Microphones[i] ?? microphones[i];
                }
            }
            else
            {
                this.logger.LogError("Error extracting comments: {error}", (LanguageExt.Common.Error)tryComments);
            }

            return ValueTask.FromResult(recording);
        }

        public ValueTask<object> ProcessFileAsync(TargetInformation information)
        {
            var tryComments = Flac.ExtractComments(information.FileStream);

            if (tryComments.IsSucc)
            {
                var raw = tryComments.ThrowIfFail();
                var dictionary = raw
                    .Keys
                    .Select(key => KeyValuePair.Create(key, this.ParseComment(key, raw)))
                    .ToDictionary();
                return ValueTask.FromResult<object>(dictionary);
            }

            return ValueTask.FromResult<object>(null);
        }

        /// <summary>
        /// Parses a comment from a FLAC file vorbis comment block.
        /// </summary>
        /// <param name="key">The comment key.</param>
        /// <param name="comments">Each comment extracted from the file (key and value).</param>
        /// <returns>The parsed comment, or null if something went wrong.</returns>
        public object ParseComment(string key, Dictionary<string, string> comments)
        {
            if (comments.ContainsKey(key) && !comments[key].Contains(FrontierLabs.UnknownValueString))
            {
                string value = comments[key];

                // Strip the microphone number if it exists so the comment parser is properly identified
                key = char.IsDigit(key.Last()) ? key[..(key.Length() - 1)] : key;

                var parsedValue = FrontierLabs.CommentParsers[key](value.Trim());

                if (parsedValue.IsSucc)
                {
                    return parsedValue.ThrowIfFail();
                }
                else
                {
                    // If a parsing error occured, log it
                    // AT 2023: most code consuming this should be handling nulls as errors...
                    //   the log as an ERROR level was obtuse.
                    this.logger.LogDebug("Error parsing comment: {error}", (LanguageExt.Common.Error)parsedValue);
                }
            }

            // Return null if an error occured or if the comment key wasn't extracted
            return null;
        }
    }
}
