// <copyright file="FroniterLabs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio.Vendors
{
    using System.Buffers.Binary;
    using System.Text;
    using LanguageExt;
    using LanguageExt.Common;
    using MetadataUtility.Extensions.System;

    public static class FrontierLabs
    {
        public record FirmwareRecord(string Comment, decimal Version, Range FoundAt, string[] Tags);

        public const string FirmwareCommentKey = "SensorFirmwareVersion";
        public const int DefaultFileStubLength = 44;

        public static readonly byte[] VendorString = Encoding.ASCII.GetBytes("Frontier Labs");
        public static readonly Error VendorStringNotFound = Error.New("Error reading file: could not find vendor string Frontier Labs in file header");

        public static readonly Error FileTooShortFirmware = Error.New("Error reading file: file is not long enough to have a firmware comment");
        public static readonly Error FirmwareNotFound = Error.New("Frontier Labs firmware comment string not found");
        public static readonly Func<string, Error> FirmwareVersionInvalid = x => Error.New($"Frontier Labs firmware version `{x}` is invlaid");

        public static async ValueTask<Fin<FirmwareRecord>> ReadFirmwareAsync(FileStream stream)
        {
            const int SeekLimit = 1024;

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
    }
}
