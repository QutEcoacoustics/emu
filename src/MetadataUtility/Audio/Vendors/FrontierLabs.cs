// <copyright file="FrontierLabs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio.Vendors
{
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using LanguageExt;
    using LanguageExt.Common;
    using MetadataUtility.Audio;
    using MetadataUtility.Extensions.System;
    using NodaTime;
    using NodaTime.Text;

    public static class FrontierLabs
    {
        public const string FirmwareCommentKey = "SensorFirmwareVersion";
        public const string RecordingStartCommentKey = "RecordingStart";
        public const string BatteryLevelCommentKey = "BatteryLevel";
        public const string LocationCommentKey = "SensorLocation";
        public const string LastSyncCommentKey = "LastTimeSync";
        public const string SensorIdCommentKey = "SensorUid";
        public const string RecordingEndCommentKey = "RecordingEnd";
        public const string SdCidCommentKey = "SdCardCid";
        public const string MicrophoneTypeCommentKey = "MicrophoneType";
        public const string MicrophoneUIDCommentKey = "MicrophoneUid";
        public const string MicrophoneBuildDateCommentKey = "MicrophoneBuildDate";
        public const string MicrophoneGainCommentKey = "ChannelGain";
        public const string UnknownMicrophoneString = "unknown";
        public const string LongitudeKey = "Longitude";
        public const string LatitudeKey = "Latitude";
        public const string MicrophonesKey = "Microphones";
        public const string MicrophoneKey = "Microphone";
        public const int DefaultFileStubLength = 44;
        public const int SeekLimit = 1024;
        public static readonly string[] DateFormats = { "yyyy'-'MM'-'dd'T'HH':'mm':'sso<+HHmm>", "yyyy'-'MM'-'dd'T'HH':'mm':'sso<+HH:mm>" };
        public static readonly byte[] VendorString = Encoding.ASCII.GetBytes("Frontier Labs");
        public static readonly Error VendorStringNotFound = Error.New("Error reading file: could not find vendor string Frontier Labs in file header");
        public static readonly Error FileTooShortFirmware = Error.New("Error reading file: file is not long enough to have a firmware comment");
        public static readonly Error FirmwareNotFound = Error.New("Frontier Labs firmware comment string not found");
        public static readonly Func<string, Error> FirmwareVersionInvalid = x => Error.New($"Frontier Labs firmware version `{x}` is invalid");
        public static readonly Func<string, Error> StartDateInvalid = x => Error.New($"Start date `{x}` is invalid");
        public static readonly Func<string, Error> EndDateInvalid = x => Error.New($"End date `{x}` is invalid");
        public static readonly Func<string, Error> LocationInvalid = x => Error.New($"Location `{x}` is invalid");
        public static readonly Func<string, Error> LastTimeSyncInvalid = x => Error.New($"Last time sync `{x}` is invalid");
        public static readonly Func<string, Error> CIDInvalid = x => Error.New($"CID `{x}` is invalid");

        public static async ValueTask<Fin<FirmwareRecord>> ReadFirmwareAsync(FileStream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            var buffer = new byte[SeekLimit];

            var count = await stream.ReadAsync(buffer);
            if (count != SeekLimit)
            {
                return FileTooShortFirmware;
            }

            // find the frontier labs vorbis vendor comment
            return FindInBufferFirmware(buffer);
        }

        public static Fin<FirmwareRecord> ParseFirmwareComment(string comment, Range offset)
        {
            // remove leading comment key and '=', then split by space
            var segments = comment[(FirmwareCommentKey.Length + 1)..].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 1)
            {
                return FirmwareVersionInvalid(comment);
            }

            var first = segments[0];
            var rest = segments[1..];
            if (first.Contains("Firmware:"))
            {
                // v3.08 has "Firmware: " prefix
                first = segments[1];
                rest = segments[2..];
            }

            // trim the leading "V" if present
            first = first.StartsWith("V") ? first[1..] : first;

            if (decimal.TryParse(first, out var version))
            {
                return new FirmwareRecord(comment, version, offset, rest);
            }
            else
            {
                return FirmwareVersionInvalid(comment);
            }
        }

        public static async ValueTask WriteFirmware(FileStream stream, FirmwareRecord original, string addendum)
        {
            var old = original.Comment;

            // there's trailing space at the end of the comment, verify there's enough space for our addendum
            var trimmed = old.TrimEnd();

            var newFirmware = Encoding.UTF8.GetBytes(trimmed + ' ' + addendum);
            if (newFirmware.Length > original.FoundAt.Length())
            {
                throw new ArgumentException("addendum must be short enough to fit within existing firmware header", nameof(addendum));
            }

            stream.Seek(original.FoundAt.Start.Value, SeekOrigin.Begin);
            await stream.WriteAsync(newFirmware);
        }

        public static async ValueTask<bool> IsDefaultStubRecording(FileStream stream)
        {
            var isLength = stream.Length == DefaultFileStubLength;

            if (!isLength)
            {
                return false;
            }

            var bytes = new byte[DefaultFileStubLength];
            await stream.ReadAsync(bytes);

            return Check(bytes);

            static bool Check(ReadOnlySpan<byte> buffer)
            {
                var dataBlockIndex = buffer.IndexOf(Wave.DataChunkId);
                if (dataBlockIndex >= 0)
                {
                    var dataChunkSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(dataBlockIndex + Wave.DataChunkId.Length));
                    return dataChunkSize == 0;
                }

                return false;
            }
        }

        public static Fin<bool> HasFrontierLabsVorbisComment(Stream stream)
        {
            long position = stream.Seek(0, SeekOrigin.Begin);
            Debug.Assert(position == 0, $"Expected stream.Seek position to return 0, instead returned {position}");

            var buffer = new byte[SeekLimit];

            var count = stream.Read(buffer);
            if (count != SeekLimit)
            {
                return FileTooShortFirmware;
            }

            Fin<(string Vendor, int LineNumber)> result = Flac.FindVendorString(buffer);

            return result.IsSucc && (((string Vendor, int LineNumber))result).Vendor.Equals(Encoding.UTF8.GetString(VendorString));
        }

        public static Fin<Dictionary<string, object>> ParseComments(Dictionary<string, string> comments)
        {
            Dictionary<string, Func<string, Fin<(string, object)>>> commentParsers = new Dictionary<string, Func<string, Fin<(string, object)>>>
            {
                { FirmwareCommentKey, FirmwareParser },
                { RecordingStartCommentKey, StartDateParser },
                { RecordingEndCommentKey, EndDateParser },
                { BatteryLevelCommentKey, BatteryLevelParser },
                { LocationCommentKey, LocationParser },
                { LastSyncCommentKey, LastSyncParser },
                { SensorIdCommentKey, SensorIdParser },
                { SdCidCommentKey, SdCidParser },
                { MicrophoneTypeCommentKey, MicrophoneTypeParser },
                { MicrophoneUIDCommentKey, MicrophoneUIDParser },
                { MicrophoneBuildDateCommentKey, MicrophoneBuildDateParser },
                { MicrophoneGainCommentKey, MicrophoneGainParser },
            };

            string[] microphoneKeys =
            {
                MicrophoneTypeCommentKey,
                MicrophoneUIDCommentKey,
                MicrophoneBuildDateCommentKey,
                MicrophoneGainCommentKey,
            };

            Dictionary<string, object> parsedResults = new Dictionary<string, object>();

            foreach (var comment in comments)
            {
                foreach (var commentParser in commentParsers)
                {
                    // Extract each comment using its corresponding parser function
                    if (comment.Key.Contains(commentParser.Key))
                    {
                        var result = commentParser.Value(comment.Value);

                        if (result.IsSucc)
                        {
                            (string Key, object Value) parsedResult = ((string, object))result;

                            if (microphoneKeys.Contains(commentParser.Key))
                            {
                                int micNumber = int.Parse(comment.Key.Substring(commentParser.Key.Length(), 1));

                                if (!parsedResults.ContainsKey(MicrophonesKey))
                                {
                                    parsedResults[MicrophonesKey] = new Dictionary<string, Dictionary<string, object>>();
                                }

                                Dictionary<string, Dictionary<string, object>> microphones = (Dictionary<string, Dictionary<string, object>>)parsedResults[MicrophonesKey];

                                if (!microphones.ContainsKey(MicrophoneKey + micNumber))
                                {
                                    Dictionary<string, object> microphoneInfo = new Dictionary<string, object>() { { parsedResult.Key, parsedResult.Value } };
                                    microphones[MicrophoneKey + micNumber] = microphoneInfo;
                                }
                                else
                                {
                                    microphones[MicrophoneKey + micNumber][parsedResult.Key] = parsedResult.Value;
                                }

                                break;
                            }

                            parsedResults[parsedResult.Key] = parsedResult.Value;
                        }
                    }
                }
            }

            return parsedResults;
        }

        public static Fin<(string Key, object Value)> FirmwareParser(string value)
        {
            var segments = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 1)
            {
                return FirmwareVersionInvalid(value);
            }

            string firmware = segments[0];

            if (firmware.Contains("Firmware:"))
            {
                firmware = segments[1];
            }

            firmware = firmware.StartsWith("V") ? firmware[1..] : firmware;

            return (FirmwareCommentKey, firmware);
        }

        public static Fin<(string Key, object Value)> StartDateParser(string value)
        {
            OffsetDateTime? startDate = null;

            foreach (string dateFormat in DateFormats)
            {
                try
                {
                    startDate = OffsetDateTimePattern.CreateWithInvariantCulture(dateFormat).Parse(value).Value;
                }
                catch (UnparsableValueException)
                {
                    continue;
                }
            }

            if (startDate == null)
            {
                return StartDateInvalid(value);
            }

            return (RecordingStartCommentKey, startDate);
        }

        public static Fin<(string Key, object Value)> EndDateParser(string value)
        {
            OffsetDateTime? endDate = null;

            foreach (string dateFormat in DateFormats)
            {
                try
                {
                    endDate = OffsetDateTimePattern.CreateWithInvariantCulture(dateFormat).Parse(value).Value;
                }
                catch (UnparsableValueException)
                {
                    continue;
                }
            }

            if (endDate == null)
            {
                return EndDateInvalid(value);
            }

            return (RecordingEndCommentKey, endDate);
        }

        public static Fin<(string Key, object Value)> BatteryLevelParser(string value) => (BatteryLevelCommentKey, value);

        public static Fin<(string Key, object Value)> LocationParser(string value)
        {
            Dictionary<string, double> location = new Dictionary<string, double>();

            try
            {
                value = new string(value.Where(c => char.IsDigit(c) || (new char[] { '+', '-', '.' }).Contains(c)).ToArray());

                int latLonDividingIndex = value.IndexOfAny(new char[] { '+', '-' }, 1);

                double latitude = double.Parse(value.Substring(0, latLonDividingIndex));
                double longitude = double.Parse(value.Substring(latLonDividingIndex));

                location[LatitudeKey] = latitude;
                location[LongitudeKey] = longitude;
            }
            catch (ArgumentOutOfRangeException)
            {
                return LocationInvalid(value);
            }

            return (LocationCommentKey, location);
        }

        public static Fin<(string Key, object Value)> LastSyncParser(string value)
        {
            OffsetDateTime? lastTimeSync = null;

            foreach (string dateFormat in DateFormats)
            {
                try
                {
                    lastTimeSync = OffsetDateTimePattern.CreateWithInvariantCulture(dateFormat).Parse(value).Value;
                }
                catch (UnparsableValueException)
                {
                    continue;
                }
            }

            if (lastTimeSync == null)
            {
                return LastTimeSyncInvalid(value);
            }

            return (LastSyncCommentKey, lastTimeSync);
        }

        public static Fin<(string Key, object Value)> SensorIdParser(string value) => (SensorIdCommentKey, value);

        public static Fin<(string Key, object Value)> SdCidParser(string value)
        {
            Models.SdCardCid cid = new Models.SdCardCid(value);
            Dictionary<string, object> cidInfo;

            try
            {
                cidInfo = cid.ExtractSdInfo();
            }
            catch (IndexOutOfRangeException)
            {
                return CIDInvalid(value);
            }

            return (SdCidCommentKey, cidInfo);
        }

        public static Fin<(string Key, object Value)> MicrophoneTypeParser(string value) => (MicrophoneTypeCommentKey, value);

        public static Fin<(string Key, object Value)> MicrophoneUIDParser(string value) => (MicrophoneUIDCommentKey, value);

        public static Fin<(string Key, object Value)> MicrophoneBuildDateParser(string value) => (MicrophoneBuildDateCommentKey, value);

        public static Fin<(string Key, object Value)> MicrophoneGainParser(string value) => (MicrophoneGainCommentKey, value);

        private static Fin<FirmwareRecord> FindInBufferFirmware(ReadOnlySpan<byte> buffer)
        {
            var vendorPosition = Flac.FindVendorString(buffer);

            if (vendorPosition.IsFail)
            {
                return VendorStringNotFound;
            }

            int offset = (((string Vendor, int Position))vendorPosition).Position;

            var commentCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
            offset += 4;

            // read each comment
            for (int i = 0; i < commentCount; i++)
            {
                var commentLength = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
                offset += 4;

                int commentStart = offset;

                // dangerous cast: but we're reading a 4096 size buffer, we'll never hit the overflow.
                int commentEnd = (int)(offset + commentLength);
                Range range = commentStart..commentEnd;

                var comment = Encoding.UTF8.GetString(buffer[range]);

                if (comment.Contains(FirmwareCommentKey))
                {
                    return ParseFirmwareComment(comment, range);
                }

                offset += (int)commentLength;
            }

            return FirmwareNotFound;
        }

        public record FirmwareRecord(string Comment, decimal Version, Range FoundAt, string[] Tags);
    }
}
