// <copyright file="FlacCommentExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.FrontierLabs
{
    using System.Threading.Tasks;
    using MetadataUtility.Audio;
    using MetadataUtility.Audio.Vendors;
    using MetadataUtility.Models;
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
            var comments = Flac.ExtractComments(information.FileStream);

            if (comments.IsSucc)
            {
                var tryParsedComments = FrontierLabs.ParseComments((Dictionary<string, string>)comments);

                if (tryParsedComments.IsSucc)
                {
                    Dictionary<string, object> parsedComments = (Dictionary<string, object>)tryParsedComments;

                    recording = recording with
                    {
                        Sensor = (recording.Sensor ?? new Sensor()) with
                        {
                            Firmware = (string)parsedComments[FrontierLabs.FirmwareCommentKey] ?? null,
                            BatteryLevel = (string)parsedComments[FrontierLabs.BatteryLevelCommentKey] ?? null,
                            LastTimeSync = (OffsetDateTime?)parsedComments[FrontierLabs.LastSyncCommentKey] ?? null,
                            SerialNumber = (string)parsedComments[FrontierLabs.SensorIdCommentKey],
                            Microphones = recording.Sensor?.Microphones ?? new Microphone[2],
                        },
                        StartDate = (OffsetDateTime)parsedComments[FrontierLabs.RecordingStartCommentKey],
                        EndDate = (OffsetDateTime)parsedComments[FrontierLabs.RecordingEndCommentKey],
                        Location = parsedComments.ContainsKey(FrontierLabs.LocationCommentKey) ? (recording.Location ?? new Location()) with
                        {
                            Longitude = ((Dictionary<string, double>)parsedComments[FrontierLabs.LocationCommentKey])[FrontierLabs.LongitudeKey],
                            Latitude = ((Dictionary<string, double>)parsedComments[FrontierLabs.LocationCommentKey])[FrontierLabs.LatitudeKey],
                        } : null,
                        MemoryCard = (recording.MemoryCard ?? new MemoryCard()) with
                        {
                            ManufacturerID = (byte)((Dictionary<string, object>)parsedComments[FrontierLabs.SdCidCommentKey])[SdCardCid.ManufacturerIDKey],
                            OEMID = (string)((Dictionary<string, object>)parsedComments[FrontierLabs.SdCidCommentKey])[SdCardCid.OEMIDKey],
                            ProductName = (string)((Dictionary<string, object>)parsedComments[FrontierLabs.SdCidCommentKey])[SdCardCid.ProductNameKey],
                            ProductRevision = (float)((Dictionary<string, object>)parsedComments[FrontierLabs.SdCidCommentKey])[SdCardCid.ProductRevisionKey],
                            SerialNumber = (uint)((Dictionary<string, object>)parsedComments[FrontierLabs.SdCidCommentKey])[SdCardCid.SerialNumberKey],
                            ManufactureDate = (string)((Dictionary<string, object>)parsedComments[FrontierLabs.SdCidCommentKey])[SdCardCid.ManufactureDateKey],
                        },
                    };

                    foreach (Dictionary<string, object> newMicrophone in ((Dictionary<string, Dictionary<string, object>>)parsedComments[FrontierLabs.MicrophonesKey]).Values)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (recording.Sensor.Microphones[i] == null || recording.Sensor.Microphones[i].UID.Equals((string)newMicrophone[FrontierLabs.MicrophoneUIDCommentKey]))
                            {
                                if (!((string)newMicrophone[FrontierLabs.MicrophoneUIDCommentKey]).Equals(FrontierLabs.UnknownMicrophoneString))
                                {
                                    recording.Sensor.Microphones[i] = (recording.Sensor.Microphones[i] ?? new Microphone()) with
                                    {
                                        Type = (string)newMicrophone[FrontierLabs.MicrophoneTypeCommentKey],
                                        UID = (string)newMicrophone[FrontierLabs.MicrophoneUIDCommentKey],
                                        BuildDate = (string)newMicrophone[FrontierLabs.MicrophoneBuildDateCommentKey],
                                        Gain = (string)newMicrophone[FrontierLabs.MicrophoneGainCommentKey],
                                    };
                                }

                                break;
                            }
                        }
                    }
                }
            }

            return ValueTask.FromResult(recording);
        }
    }
}
