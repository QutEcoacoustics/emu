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
    using NodaTime;

    public class FlacCommentExtractor : IMetadataOperation
    {
        private readonly ILogger<FlacCommentExtractor> logger;

        public FlacCommentExtractor(ILogger<FlacCommentExtractor> logger)
        {
            this.logger = logger;
        }

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

                Dictionary<string, double> location = (Dictionary<string, double>)this.ParseComment(FrontierLabs.LocationCommentKey, comments);
                Dictionary<string, object> sdCid = (Dictionary<string, object>)this.ParseComment(FrontierLabs.SdCidCommentKey, comments);

                List<Microphone> microphones = new List<Microphone>();
                int micNumber = 1;

                // Extract all microphone information
                while (comments.ContainsKey(FrontierLabs.MicrophoneTypeCommentKey + micNumber))
                {
                    // FL uses channel A, B to refer to microphones 1 and 2 respectively
                    // Converts to channel name using ASCII value offset of 64
                    string channelName = ((char)(micNumber + 64)).ToString();

                    Microphone microphone = new Microphone() with
                    {
                        Type = (string)this.ParseComment(FrontierLabs.MicrophoneTypeCommentKey + micNumber, comments),
                        UID = (string)this.ParseComment(FrontierLabs.MicrophoneUIDCommentKey + micNumber, comments),
                        BuildDate = (LocalDate?)this.ParseComment(FrontierLabs.MicrophoneBuildDateCommentKey + micNumber, comments),
                        Gain = (double?)this.ParseComment(FrontierLabs.MicrophoneGainCommentKey + micNumber, comments),
                        Channel = micNumber,
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
                        Firmware = recording.Sensor?.Firmware ?? (string)this.ParseComment(FrontierLabs.FirmwareCommentKey, comments),
                        BatteryLevel = recording.Sensor?.BatteryLevel ?? batteryLevel,
                        Voltage = recording.Sensor?.Voltage ?? voltage,
                        LastTimeSync = recording.Sensor?.LastTimeSync ?? (OffsetDateTime?)this.ParseComment(FrontierLabs.LastSyncCommentKey, comments),
                        SerialNumber = recording.Sensor?.SerialNumber ?? (string)this.ParseComment(FrontierLabs.SensorIdCommentKey, comments),
                        Microphones = recording.Sensor?.Microphones ?? new Microphone[microphones.Length()],
                    },
                    StartDate = recording.StartDate ?? (OffsetDateTime?)this.ParseComment(FrontierLabs.RecordingStartCommentKey, comments),
                    EndDate = recording.EndDate ?? (OffsetDateTime?)this.ParseComment(FrontierLabs.RecordingEndCommentKey, comments),
                    Location = (recording.Location ?? new Location()) with
                    {
                        Longitude = recording.Location?.Longitude ?? (location != null ? location[FrontierLabs.LongitudeKey] : null),
                        Latitude = recording.Location?.Latitude ?? (location != null ? location[FrontierLabs.LatitudeKey] : null),
                    },
                    MemoryCard = (recording.MemoryCard ?? new MemoryCard()) with
                    {
                        ManufacturerID = recording.MemoryCard?.ManufacturerID ?? (sdCid != null ? (byte)sdCid[SdCardCid.ManufacturerIDKey] : null),
                        OEMID = recording.MemoryCard?.OEMID ?? (sdCid != null ? (string)sdCid[SdCardCid.OEMIDKey] : null),
                        ProductName = recording.MemoryCard?.ProductName ?? (sdCid != null ? (string)sdCid[SdCardCid.ProductNameKey] : null),
                        ProductRevision = recording.MemoryCard?.ProductRevision ?? (sdCid != null ? (float)sdCid[SdCardCid.ProductRevisionKey] : null),
                        SerialNumber = recording.MemoryCard?.SerialNumber ?? (sdCid != null ? (uint)sdCid[SdCardCid.SerialNumberKey] : null),
                        ManufactureDate = recording.MemoryCard?.ManufactureDate ?? (sdCid != null ? (string)sdCid[SdCardCid.ManufactureDateKey] : null),
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

        /// <summary>
        /// Parses a comment from a FLAC file vorbis comment block.
        /// </summary>
        /// <param name="key">The comment key.</param>
        /// <param name="comments">Each comment extracted from the file (key and value).</param>
        /// <returns>The parsed comment, or null if something went wrong.</returns>
        public Fin<object>? ParseComment(string key, Dictionary<string, string> comments)
        {
            if (comments.ContainsKey(key) && !comments[key].Contains(FrontierLabs.UnknownValueString))
            {
                string value = comments[key];

                // Strip the microphone number if it exists so the comment parser is properly identified
                key = char.IsDigit(key.Last()) ? key.Substring(0, key.Length() - 1) : key;

                var parsedValue = FrontierLabs.CommentParsers[key](value.Trim());

                if (parsedValue.IsSucc)
                {
                    return parsedValue;
                }
                else
                {
                    // If a parsing error occured, log it
                    this.logger.LogError("Error parsing comment: {error}", (LanguageExt.Common.Error)parsedValue);
                }
            }

            // Return null if an error occured or if the comment key wasn't extracted
            return null;
        }
    }
}
