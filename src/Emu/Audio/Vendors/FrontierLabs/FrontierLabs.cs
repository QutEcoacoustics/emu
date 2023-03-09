// <copyright file="FrontierLabs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors
{
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using Emu.Audio;
    using Emu.Audio.WAVE;
    using Emu.Extensions.System;
    using Emu.Fixes.FrontierLabs;
    using Emu.Utilities;
    using LanguageExt;
    using LanguageExt.Common;
    using NodaTime;
    using NodaTime.Text;
    using static LanguageExt.Prelude;

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

        public const int DefaultStubDataLength = 44;

        // based on a statistical sample this is the most common stub length for flac filees
        public const int DefaultFileStubLength2 = 153;

        public static readonly OffsetDateTimePattern[] DatePatterns =
        {
            OffsetDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd'T'HH':'mm':'sso<M>"),
            OffsetDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd'T'HH':'mm':'sso<m>"),
        };

        public static readonly LocalDateTimePattern[] LocalDatePatterns =
        {
            // V3.08 firmware used a space as a separator
            LocalDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
        };

        public static readonly byte[] VendorString = Encoding.ASCII.GetBytes("Frontier Labs");
        public static readonly Dictionary<string, Func<string, Fin<object>>> CommentParsers = new()
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
        public static readonly Error EmptyError = Error.New($"File is empty");

        public static async ValueTask<Fin<FirmwareRecord>> ReadFirmwareAsync(Stream stream)
        {
            var isFlac = Flac.IsFlacFile(stream);
            if (!isFlac.IfFail(false))
            {
                return isFlac.IsFail ? (Error)isFlac : Flac.NotFlac;
            }

            var vorbisChunk = Flac.ScanForChunk(stream, Flac.MetadataBlockType.VorbisComment);

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
                return new FirmwareRecord(comment, version, offset, Seq(rest));
            }
            else
            {
                return FirmwareVersionInvalid(comment);
            }
        }

        public static async ValueTask WriteFirmware(Stream stream, FirmwareRecord original)
        {
            var old = original.Comment;

            // there's trailing space at the end of the comment, verify there's enough space for our addendum
            var trimmed = old.TrimEnd();

            var newFirmware = Encoding.UTF8.GetBytes(trimmed + ' ' + string.Join(" ", original.Tags));
            if (newFirmware.Length > original.FoundAt.Length())
            {
                throw new ArgumentException(
                    "tags must be short enough to fit within existing firmware header",
                    nameof(original));
            }

            stream.Seek(original.FoundAt.Start.Value, SeekOrigin.Begin);
            await stream.WriteAsync(newFirmware);
        }

        /// <summary>
        /// Determines whether a given file stream exhibits behaviour of a preallocated header file.
        /// These files act like wave files, but don't have any meaningful data.
        /// There are certain traits that identify this problem, not every preallocated header file has them all.
        /// A scoring system is used to determine whether a file fits the criteria.
        /// If three or more faults are found, the file is deemed to have the problem.
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <param name="path">The path to the current target.</param>
        /// <returns>
        /// True for a preallocated header file, false if not.
        /// </returns>
        public static bool IsPreallocatedHeader(Stream stream, string path)
        {
            int faults = 0;

            // an empty file is a distinct an different error
            if (stream.Length == 0)
            {
                return false;
            }

            if (stream.Length is DefaultFileStubLength or DefaultFileStubLength2)
            {
                faults += 2;
            }
            else if (stream.Length < 200)
            {
                // If there are less than 200 bytes in the file, increment faults
                faults++;
            }

            var riffChunk = Wave.FindRiffChunk(stream);

            // If there is a riff chunk but a flac extension, increment faults
            if (Path.GetExtension(path).Equals(Flac.Extension) && riffChunk.IsSucc)
            {
                faults++;
            }

            if (riffChunk.IsFail)
            {
                // some edge cases have no RIFF magic number
                stream.Position = 0;
                Span<byte> buffer = stackalloc byte[sizeof(uint)];
                stream.Read(buffer);
                if (BinaryPrimitives.ReadUInt32LittleEndian(buffer) == 0)
                {
                    faults++;
                }

                return Judgement();
            }

            // If the riff chunk size is incorrect, increment faults
            if (((RangeHelper.Range)riffChunk).End != stream.Length)
            {
                faults++;
            }

            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var dataChunk = waveChunk.Bind(w => Wave.FindDataChunk(stream, w));
            if (dataChunk.IsFail)
            {
                return Judgement();
            }

            long dataStart = ((RangeHelper.Range)dataChunk).Start;
            long dataEnd = ((RangeHelper.Range)dataChunk).End;

            // If the data is less than 4 bytes or the first 4 bytes are 0, increment faults
            if (dataEnd - dataStart < 4)
            {
                faults++;
            }
            else
            {
                Span<byte> dataBuffer = stackalloc byte[4];
                long position = stream.Seek(dataStart, SeekOrigin.Begin);

                if (position == dataStart)
                {
                    stream.Read(dataBuffer);

                    if (BinaryPrimitives.ReadInt32BigEndian(dataBuffer) == 0)
                    {
                        faults++;
                    }
                }
            }

            // If the data section is longer than the stream, increment faults
            if (dataEnd > stream.Length)
            {
                faults++;
            }

            return Judgement();

            bool Judgement() => faults >= 3;
        }

        /// <summary>
        /// Check if a file is pre-allocated and wholly empty.
        /// This scenario happens when FL allocate space for a file but never write any samples into it.
        /// This is a slightly different situation than a pre-allocated header (<see cref="IsPreallocatedHeader"/>)
        /// which is one of these pre-allocated files that did not finish writing the file.
        /// Works only for RIFF WAVE files.
        /// </summary>
        /// <param name="stream">The file stream to search for.</param>
        /// <param name="utilities">An instance of <see cref="FileUtilities"/> to use.</param>
        /// <returns>True if the file matches the criteria.</returns>
        public static async Task<Fin<bool>> IsPreallocatedFile(Stream stream, FileUtilities utilities)
        {
            var riffChunk = Wave.FindRiffChunk(stream);
            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));

            var dataChunk = waveChunk.Bind(w => Wave.FindDataChunk(stream, w, allowOutOfBounds: true));

            if (dataChunk.Case is RangeHelper.Range r)
            {
                if (r.Length != DefaultStubDataLength)
                {
                    return false;
                }

                // scan the rest of the file to see if it's full of null bytes
                return await utilities.CheckForContinuousValue(stream, r.Start);
            }
            else
            {
                return (Error)dataChunk;
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

            var vorbisChunk = Flac.ScanForChunk(stream, Flac.MetadataBlockType.VorbisComment);

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
            // Try parsing the date in each known date format (varies depending on firmware version)
            foreach (OffsetDateTimePattern datePattern in DatePatterns)
            {
                if (datePattern.Parse(value) is { Success: true } d)
                {
                    return d.Value;
                }
            }

            foreach (var pattern in LocalDatePatterns)
            {
                if (pattern.Parse(value) is { Success: true } d)
                {
                    return d.Value;
                }
            }

            return DateInvalid(value);
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
                double latitude = double.Parse(parsedValue[..latLonDividingIndex]);
                double longitude = double.Parse(parsedValue[latLonDividingIndex..]);

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
            if (buffer.Length < 12)
            {
                return FirmwareNotFound;
            }

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

        public record FirmwareRecord(string Comment, decimal Version, Range FoundAt, Seq<string> Tags);
    }
}
