// <copyright file="FrontierLabs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio.Vendors
{
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
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
        public const string UnknownValueString = "unknown";
        public const string LongitudeKey = "Longitude";
        public const string LatitudeKey = "Latitude";
        public const int DefaultFileStubLength = 44;
        public static readonly OffsetDateTimePattern[] DatePatterns =
        {
            OffsetDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd'T'HH':'mm':'sso<M>"),
            OffsetDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd'T'HH':'mm':'sso<m>"),
        };

        public static readonly byte[] VendorString = Encoding.ASCII.GetBytes("Frontier Labs");
        public static readonly Dictionary<string, Func<string, Fin<object>>> CommentParsers = new Dictionary<string, Func<string, Fin<object>>>
        {
            { FirmwareCommentKey, FirmwareParser },
            { RecordingStartCommentKey, OffsetDateTimeParser },
            { RecordingEndCommentKey, OffsetDateTimeParser },
            { BatteryLevelCommentKey, BatteryParser },
            { LocationCommentKey, LocationParser },
            { LastSyncCommentKey, OffsetDateTimeParser },
            { SensorIdCommentKey, GenericParser },
            { SdCidCommentKey, SdCidParser },
            { MicrophoneTypeCommentKey, GenericParser },
            { MicrophoneUIDCommentKey, GenericParser },
            { MicrophoneBuildDateCommentKey, DateParser },
            { MicrophoneGainCommentKey, NumericParser },
        };

        public static readonly Error FirmwareNotFound = Error.New("Frontier Labs firmware comment string not found");
        public static readonly Func<string, Error> FirmwareVersionInvalid = x => Error.New($"Frontier Labs firmware version `{x}` can't be parsed");
        public static readonly Func<string, Error> DateInvalid = x => Error.New($"Date `{x}` can't be parsed");
        public static readonly Func<string, Error> LocationInvalid = x => Error.New($"Location `{x}` can't be parsed");
        public static readonly Func<string, Error> CIDInvalid = x => Error.New($"CID `{x}` can't be parsed");
        public static readonly Func<string, Error> BatteryValuesInvalid = x => Error.New($"Battery values '{x}' can't be parsed");
        public static readonly Func<string, Error> ParsingError = x => Error.New($"Value '{x}' cant' be parsed");

        public static async ValueTask<Fin<FirmwareRecord>> ReadFirmwareAsync(FileStream stream)
        {
            var vorbisChunk = Flac.ScanForChunk(stream, Flac.VorbisCommentBlockNumber);

            if (vorbisChunk.IsFail)
            {
                return (Error)vorbisChunk;
            }

            var vorbisSpan = await RangeHelper.ReadRangeAsync(stream, (RangeHelper.Range)vorbisChunk);

            // find the frontier labs vorbis vendor comment
            return FindInBufferFirmware(vorbisSpan, ((RangeHelper.Range)vorbisChunk).Start);
        }

        public static Fin<FirmwareRecord> ParseFirmwareComment(string comment, Range offset)
        {
            var firmware = FirmwareParser(comment[(FirmwareCommentKey.Length + 1)..]);

            var rest = comment[(comment.IndexOf((string)firmware) + ((string)firmware).Length)..].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (decimal.TryParse((string)firmware, out var version))
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

        /// <summary>
        /// Determines whether a FLAC file has a frontier labs vorbis comment block.
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <returns>Boolean indicating whether the file has the vorbis comment block.</returns>
        public static Fin<bool> HasFrontierLabsVorbisComment(Stream stream)
        {
            long position = stream.Seek(0, SeekOrigin.Begin);
            Debug.Assert(position == 0, $"Expected stream.Seek position to return 0, instead returned {position}");

            var vorbisChunk = Flac.ScanForChunk(stream, Flac.VorbisCommentBlockNumber);

            if (vorbisChunk.IsFail)
            {
                return (Error)vorbisChunk;
            }

            var vorbisSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)vorbisChunk);

            var vendorString = Flac.FindXiphVendorString(vorbisSpan);

            return vendorString.Equals(Encoding.UTF8.GetString(VendorString));
        }

        /// <summary>
        /// Generic Frontier Labs vorbis comment parser.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed value, in this case no parsing is really required.
        /// The encoded value itself is what we're looking for.</returns>
        public static Fin<object> GenericParser(string value) => value;

        /// <summary>
        /// Frontier Labs vorbis comment numeric parser.
        /// Strips all non numeric characters from the value.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed value.</returns>
        public static Fin<object> NumericParser(string value) => double.Parse(value.Where(c => char.IsDigit(c) || (new char[] { '.', '-' }).Contains(c)).ToArray());

        /// <summary>
        /// Frontier Labs vorbis comment firmware parser.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed firmware.</returns>
        public static Fin<object> FirmwareParser(string value)
        {
            var segments = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 1)
            {
                return FirmwareVersionInvalid(value);
            }

            string firmware = segments[0];

            // v3.08 has "Firmware: " prefix
            if (firmware.Contains("Firmware:"))
            {
                firmware = segments[1];
            }

            // trim the leading "V" if present
            firmware = firmware.StartsWith("V") ? firmware[1..] : firmware;

            return firmware;
        }

        /// <summary>
        /// Frontier Labs vorbis comment battery parser.
        /// Parses battery voltage and battery level.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed battery values.</returns>
        public static Fin<object> BatteryParser(string value)
        {
            string[] batteryValues = value.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (batteryValues.Length != 2)
            {
                return BatteryValuesInvalid(value);
            }

            double? batteryLevel = (double)NumericParser(batteryValues[0]);
            double? batteryVoltage = (double)NumericParser(batteryValues[1]);

            return (batteryLevel, batteryVoltage);
        }

        /// <summary>
        /// Frontier Labs vorbis comment offset date time parser.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed date.</returns>
        public static Fin<object> OffsetDateTimeParser(string value)
        {
            OffsetDateTime? date = null;

            // Try parsing the date in each known date format (varies depending on firmware version)
            foreach (OffsetDateTimePattern datePattern in DatePatterns)
            {
                try
                {
                    date = datePattern.Parse(value).Value;
                }
                catch (UnparsableValueException)
                {
                    continue;
                }
            }

            if (date == null)
            {
                return DateInvalid(value);
            }

            return date;
        }

        /// <summary>
        /// Frontier Labs vorbis comment date parser.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed date.</returns>
        public static Fin<object> DateParser(string value) => LocalDatePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd").Parse(value).Value;

        /// <summary>
        /// Frontier Labs vorbis comment location parser.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>A dictionary containing the coordinates.</returns>
        public static Fin<object> LocationParser(string value)
        {
            Dictionary<string, double> location = new Dictionary<string, double>();

            try
            {
                // Only keep characters relevant to coordinates
                var parsedValue = new string(value.Where(c => char.IsDigit(c) || (new char[] { '+', '-', '.' }).Contains(c)).ToArray());

                // Find index dividing lat and lon
                int latLonDividingIndex = parsedValue.IndexOfAny(new char[] { '+', '-' }, 1);

                // Parse lat and lon
                double latitude = double.Parse(parsedValue.Substring(0, latLonDividingIndex));
                double longitude = double.Parse(parsedValue.Substring(latLonDividingIndex));

                location[LatitudeKey] = latitude;
                location[LongitudeKey] = longitude;
            }
            catch (ArgumentOutOfRangeException)
            {
                return LocationInvalid(value);
            }

            return location;
        }

        /// <summary>
        /// Frontier Labs vorbis comment SD CID parser.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>A dictionary containing all CID values.</returns>
        public static Fin<object> SdCidParser(string value)
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

            return cidInfo;
        }

        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1008:OpeningParenthesisMustBeSpacedCorrectly", Justification = "Parentheses are valid when calculating absolute range.")]
        private static Fin<FirmwareRecord> FindInBufferFirmware(ReadOnlySpan<byte> buffer, long absoluteOffset)
        {
            int offset = 0;
            int vendorLength = BinaryPrimitives.ReadInt32LittleEndian(buffer);

            offset += 4;

            offset += vendorLength;

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

                Range absoluteRange = (commentStart + (int)absoluteOffset)..(commentEnd + (int)absoluteOffset);

                var comment = Encoding.UTF8.GetString(buffer[commentStart..commentEnd]);

                if (comment.Contains(FirmwareCommentKey))
                {
                    return ParseFirmwareComment(comment, absoluteRange);
                }

                offset += (int)commentLength;
            }

            return FirmwareNotFound;
        }

        public record FirmwareRecord(string Comment, decimal Version, Range FoundAt, string[] Tags);
    }
}
