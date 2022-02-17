// <copyright file="Flac.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio
{
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

        public static Fin<ulong> ReadTotalSamples(FileStream stream)
        {
            stream.Seek(FlacSamplesOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[5];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            // flac files have an unsigned 36-bit integer for total samples!
            return BinaryHelpers.Read36BitUnsignedBigEndianIgnoringFirstOctet(buffer);
        }

        public static Fin<uint> ReadSampleRate(FileStream stream)
        {
            stream.Seek(SampleRateOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[3];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return BinaryHelpers.Read20BitUnsignedBigEndianIgnoringLastOctet(buffer);
        }

        public static Fin<byte> ReadNumChannels(FileStream stream)
        {
            stream.Seek(ChannelOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[1];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            return (byte)(BinaryHelpers.Read3BitUnsignedBigEndianIgnoringFirstFourAndLastBit(buffer) + 1);
        }

        public static Fin<byte> ReadBitRate(FileStream stream)
        {
            stream.Seek(ChannelOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[2];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            // flac files have an unsigned 36-bit integer for total sample duration!
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
            BinaryHelpers.Write36BitUnsignedBigEndianIgnoringFirstOctet(buffer, sampleCount);

            stream.Seek(FlacSamplesOffset, SeekOrigin.Begin);
            stream.Write(buffer);

            return Unit.Default;
        }

        public static Fin<bool> IsFlacFile(FileStream stream)
        {
            Span<byte> buffer = stackalloc byte[4];

            stream.Seek(0, SeekOrigin.Begin);
            var read = stream.Read(buffer);

            if (read != FlacMagicNumber.Length)
            {
                return FileTooShortFlac;
            }

            return buffer.StartsWith(FlacMagicNumber);
        }
    }
}
