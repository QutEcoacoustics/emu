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
    using MetadataUtility.Extensions.System;
    using MetadataUtility.Models;
    using MetadataUtility.Utilities;
    using NodaTime.Text;

    public static class FrontierLabs
    {
        public const string FirmwareCommentKey = "SensorFirmwareVersion";
        public const string RecordingStartCommentKey = "RecordingStart";
        public const string BatteryLevelCommentKey = "BatteryLevel";
        public const string LocationCommentKey = "SensorLocation";
        public const string LastSyncCommentKey = "LastTimeSync";
        public const string SensorIdKey = "SensorUid";
        public const string RecordingEndCommentKey = "RecordingEnd";
        public const string UnknownMicrophoneString = "unknown";
        public const string EmptyGainString = "0dB";
        public const string DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'sso<+HHmm>";
        public const int DefaultFileStubLength = 44;
        public const int BlockTypeOffset = 4;
        public const int SeekLimit = 1024;
        public static readonly (string, string)[] MicrophoneKeys = { ("MicrophoneType", "Type"), ("MicrophoneUid", "UID"), ("MicrophoneBuildDate", "BuildDate"), ("ChannelGain", "Gain") };
        public static readonly byte[] VendorString = Encoding.ASCII.GetBytes("Frontier Labs");
        public static readonly LanguageExt.Common.Error VendorStringNotFound = LanguageExt.Common.Error.New("Error reading file: could not find vendor string Frontier Labs in file header");

        public static readonly LanguageExt.Common.Error FileTooShortFirmware = LanguageExt.Common.Error.New("Error reading file: file is not long enough to have a firmware comment");
        public static readonly LanguageExt.Common.Error FirmwareNotFound = LanguageExt.Common.Error.New("Frontier Labs firmware comment string not found");
        public static readonly Func<string, LanguageExt.Common.Error> FirmwareVersionInvalid = x => LanguageExt.Common.Error.New($"Frontier Labs firmware version `{x}` is invlaid");

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
                var dataBlockIndex = buffer.IndexOf(Wave.DataBlockId);
                if (dataBlockIndex >= 0)
                {
                    var dataChunkSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(dataBlockIndex + Wave.DataBlockId.Length));
                    return dataChunkSize == 0;
                }

                return false;
            }
        }

        public static Fin<bool> HasFrontierLabsVorbisComment(Stream stream)
        {
            Fin<long> result = FindVendorStringPosition(stream);

            return result.IsSucc;
        }

        public static void ExtractVorbisCommentMetadata(Stream stream, ref Recording recording)
        {
            long startPosition = (long)FindVendorStringPosition(stream);

            long position = stream.Seek(startPosition, SeekOrigin.Begin);
            Debug.Assert(position == startPosition, $"Expected stream.Seek position to return {startPosition}, instead returned {position}");

            Span<byte> buffer = stackalloc byte[SeekLimit];
            stream.Read(buffer);
            int offset = 0;

            uint commentListLength = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            offset += 4;

            uint commentLength;
            string comment, value;
            List<Microphone> microphones = new List<Microphone>();

            // Extract each comment one by one
            for (int i = 0; i < commentListLength; i++)
            {
                commentLength = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
                offset += 4;

                int commentStart = offset;
                int commentEnd = (int)(offset + commentLength);
                Range range = commentStart..commentEnd;
                offset += (int)commentLength;

                comment = Encoding.UTF8.GetString(buffer[range]);
                value = comment.Split("=")[1].Trim();

                // Extract firmware version
                if (comment.Contains(FirmwareCommentKey))
                {
                    var segments = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string firmware = segments[0];

                    if (firmware.Contains("Firmware:"))
                    {
                        firmware = segments[1];
                    }

                    firmware = firmware.StartsWith("V") ? firmware[1..] : firmware;

                    recording = recording with
                    {
                        Sensor = (recording.Sensor ?? new Sensor()) with
                        {
                            Firmware = firmware,
                        },
                    };
                }

                // Extract recording start
                else if (comment.Contains(RecordingStartCommentKey))
                {
                    recording = recording with
                    {
                        StartDate = OffsetDateTimePattern.CreateWithInvariantCulture(DateFormat).Parse(value).Value,
                    };
                }

                // Extract recording end
                else if (comment.Contains(RecordingEndCommentKey))
                {
                    recording = recording with
                    {
                        EndDate = OffsetDateTimePattern.CreateWithInvariantCulture(DateFormat).Parse(value).Value,
                    };
                }

                // Extract batter level
                else if (comment.Contains(BatteryLevelCommentKey))
                {
                    recording = recording with
                    {
                        Sensor = (recording.Sensor ?? new Sensor()) with
                        {
                            BatteryLevel = value,
                        },
                    };
                }

                //Extract coordinates
                else if (comment.Contains(LocationCommentKey))
                {
                    try
                    {
                        value = new string(value.Where(c => char.IsDigit(c) || (new char[] { '+', '-', '.' }).Contains(c)).ToArray());

                        int latLonDividingIndex = value.IndexOfAny(new char[] { '+', '-' }, 1);

                        double latitude = double.Parse(value.Substring(0, latLonDividingIndex));
                        double longitude = double.Parse(value.Substring(latLonDividingIndex));

                        recording = recording with
                        {
                            Location = (recording.Location ?? new Location()) with
                            {
                                Latitude = latitude,
                                Longitude = longitude,
                            },
                        };
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        continue;
                    }
                }

                // Extract last sync time
                else if (comment.Contains(LastSyncCommentKey))
                {
                    recording = recording with
                    {
                        Sensor = (recording.Sensor ?? new Sensor()) with
                        {
                            LastTimeSync = OffsetDateTimePattern.CreateWithInvariantCulture(DateFormat).Parse(value).Value,
                        },
                    };
                }

                // Extract sensor ID
                else if (comment.Contains(SensorIdKey))
                {
                    recording = recording with
                    {
                        Sensor = (recording.Sensor ?? new Sensor()) with
                        {
                            SerialNumber = value,
                        },
                    };
                }

                // Extract microphone information
                else
                {
                    foreach ((string CommentKey, string ModelKey) microphoneKey in MicrophoneKeys)
                    {
                        if (comment.Contains(microphoneKey.CommentKey))
                        {
                            UpdateMicrophones(comment, value, microphones, microphoneKey);
                        }
                    }
                }
            }

            recording = recording with
            {
                Sensor = (recording.Sensor ?? new Sensor()) with
                {
                    Microphones = recording.Sensor!.Microphones ?? new List<Microphone>(),
                },
            };

            bool hasMicrophone;

            // Update recording with new microphone(s)
            // First verify this microphone has not been added from a different source
            foreach (Microphone newMicrophone in microphones)
            {
                hasMicrophone = false;

                foreach (Microphone microphone in recording.Sensor.Microphones)
                {
                    if (microphone.UID.Equals(newMicrophone.UID))
                    {
                        hasMicrophone = true;
                    }
                }

                if (!hasMicrophone)
                {
                    recording.Sensor.Microphones.Add(new Microphone() with
                    {
                        UID = newMicrophone.UID,
                        BuildDate = newMicrophone.BuildDate,
                        Type = newMicrophone.Type,
                        Gain = newMicrophone.Gain,
                    });
                }
            }
        }

        private static void UpdateMicrophones(string comment, string value, List<Microphone> microphones, (string CommentKey, string ModelKey) microphoneKey)
        {
            if (value.Contains(UnknownMicrophoneString) || value.Equals(EmptyGainString))
            {
                return;
            }

            // Parse the microphone number & see if it has already been identified
            int micNumber = int.Parse(comment.Substring(microphoneKey.CommentKey.Length(), 1));
            int micIndex = -1;
            for (int i = 0; i < microphones.Length(); i++)
            {
                if (microphones[i].Number == micNumber)
                {
                    micIndex = i;
                    break;
                }
            }

            // If this mic hasn't been created yet, create it
            if (micIndex == -1)
            {
                microphones.Add(new Microphone() with
                {
                    Number = micNumber,
                });

                micIndex = microphones.Length() - 1;
            }

            // Update the given value of the microphone using Type.InvokeMember
            microphones[micIndex].GetType().InvokeMember(
                microphoneKey.ModelKey,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                Type.DefaultBinder,
                microphones[micIndex],
                new string[] { value });
        }

        /// <summary>
        /// Frontier Labs vendor string is found within the file stream.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The position of the vendor string in the file stream.</returns>
        private static Fin<long> FindVendorStringPosition(Stream stream)
        {
            const int VorbisCommentBlockNumber = 4, MaxIteration = 20;

            long position = stream.Seek(BlockTypeOffset, SeekOrigin.Begin);
            Debug.Assert(position == 4, $"Expected stream.Seek position to return 4, instead returned {position}");

            Span<byte> blockTypeBuffer = stackalloc byte[1];
            Span<byte> blockLengthBuffer = stackalloc byte[3];

            uint length = 0, i = 0, blockType;

            do
            {
                stream.Seek(length, SeekOrigin.Current);

                stream.Read(blockTypeBuffer);
                stream.Read(blockLengthBuffer);

                blockType = BinaryHelpers.Read7BitUnsignedBigEndianIgnoringFirstBit(blockTypeBuffer);
                length = BinaryHelpers.Read24bitUnsignedBigEndian(blockLengthBuffer);

                i++;
            }
            while (blockType != VorbisCommentBlockNumber && (blockTypeBuffer[0] >> 7) != 1 && i < MaxIteration);

            Span<byte> vendorLengthBuffer = stackalloc byte[4];

            if (blockType == 4)
            {
                stream.Read(vendorLengthBuffer);
                uint vendorLength = BinaryPrimitives.ReadUInt32LittleEndian(vendorLengthBuffer);

                Span<byte> vendorBuffer = stackalloc byte[(int)vendorLength];
                stream.Read(vendorBuffer);

                if (vendorBuffer.SequenceEqual(VendorString.AsSpan()))
                {
                    return stream.Position;
                }
            }

            return VendorStringNotFound;
        }

        private static Fin<FirmwareRecord> FindInBufferFirmware(ReadOnlySpan<byte> buffer)
        {
            // beginning of file
            int offset = 0;
            var index = buffer.IndexOf(VendorString);
            if (index < 0)
            {
                return VendorStringNotFound;
            }

            // next read the number of comments
            offset += index + VendorString.Length;

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
