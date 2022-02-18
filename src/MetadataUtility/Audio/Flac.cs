// <copyright file="Flac.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio
{
    using System.Diagnostics;
    using LanguageExt;
    using LanguageExt.Common;
    using MetadataUtility.Utilities;

    public static class Flac
    {
        public const string Mime = "audio/flac";
        public const int FlacSamplesOffset = 21;
        public const int SampleRateOffset = 18;
        public const int ChannelOffset = 20;
        public static readonly byte[] FlacMagicNumber = new byte[] { (byte)'f', (byte)'L', (byte)'a', (byte)'C' };

        public static readonly Error FileTooShort = Error.New("Error reading file: file is not long enough to have a duration header");

        public static readonly Error FileTooShortFlac = Error.New("Error reading file: file is not long enough to have a fLaC header");

        /// <summary>
        /// The total samples in the stream are read from the flac header here.
        /// This can be extracted from bits 173-208 of the stream.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The total samples.</returns>
        public static Fin<ulong> ReadTotalSamples(Stream stream)
        {
            long position = stream.Seek(FlacSamplesOffset, SeekOrigin.Begin);
            Debug.Assert(position == 21, $"Expected stream.Seek position to return 21, instead returned {position}");

            Span<byte> buffer = stackalloc byte[5];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return BinaryHelpers.Read36BitUnsignedBigEndianIgnoringFirstNibble(buffer);
        }

        /// <summary>
        /// The sample rate is read from the flac header here.
        /// This can be extracted from bits 145-164 of the stream.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The sample rate.</returns>
        public static Fin<uint> ReadSampleRate(Stream stream)
        {
            long position = stream.Seek(SampleRateOffset, SeekOrigin.Begin);
            Debug.Assert(position == 18, $"Expected stream.Seek position to return 18, instead returned {position}");

            Span<byte> buffer = stackalloc byte[3];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return BinaryHelpers.Read20BitUnsignedBigEndianIgnoringLastNibble(buffer);
        }

        /// <summary>
        /// The number of channels is read from the flac header here.
        /// This can be extracted from bits 165-167 of the stream.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The number of channels.</returns>
        public static Fin<byte> ReadNumChannels(Stream stream)
        {
            long position = stream.Seek(ChannelOffset, SeekOrigin.Begin);
            Debug.Assert(position == 20, $"Expected stream.Seek position to return 20, instead returned {position}");

            Span<byte> buffer = stackalloc byte[1];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return (byte)(BinaryHelpers.Read3BitUnsignedBigEndianIgnoringFirstFourAndLastBit(buffer) + 1);
        }

        /// <summary>
        /// The bit depth is read from the flac header here.
        /// This can be extracted from bits 168-172 of the stream.
        /// More information: https://xiph.org/flac/format.html#metadata_block_streaminfo.
        /// </summary>
        /// <param name="stream">The flac file stream.</param>
        /// <returns>The bit depth.</returns>
        public static Fin<byte> ReadBitDepth(Stream stream)
        {
            long position = stream.Seek(ChannelOffset, SeekOrigin.Begin);
            Debug.Assert(position == 20, $"Expected stream.Seek position to return 20, instead returned {position}");

            stream.Seek(ChannelOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[2];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            // flac files have an unsigned 5-bit integer for bit rate
            return (byte)(BinaryHelpers.Read5BitUnsignedBigEndianIgnoringFirstSevenAndLastFourBits(buffer) + 1);
        }

        public static Fin<Unit> WriteTotalSamples(FileStream stream, ulong sampleCount)
        {
            stream.Seek(FlacSamplesOffset, SeekOrigin.Begin);

            // we're writing over the top of other bits, so read first
            Span<byte> buffer = stackalloc byte[5];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            // flac files have an unsigned 36-bit integer for total sample duration!
            BinaryHelpers.Write36BitUnsignedBigEndianIgnoringFirstNibble(buffer, sampleCount);

            stream.Seek(FlacSamplesOffset, SeekOrigin.Begin);
            stream.Write(buffer);

            return Unit.Default;
        }

        public static Fin<bool> IsValidFlacFile(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[4];

            stream.Seek(0, SeekOrigin.Begin);
            var read = stream.Read(buffer);

            if (read != FlacMagicNumber.Length)
            {
                return FileTooShortFlac;
            }

            return buffer.StartsWith(FlacMagicNumber) && stream.Length > 42;
        }
    }
}
