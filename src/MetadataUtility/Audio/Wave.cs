// <copyright file="Wave.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio
{
    using LanguageExt;
    using LanguageExt.Common;
    using MetadataUtility.Utilities;

    public static class Wave
    {
        public const string Mime = "audio/wave";

        public const int WaveFileOffset = 7;
        public const int WaveSampleRateOffset = 24;
        public const int WaveChannelOffset = 22;
        public const int WaveBitsPerSampleOffset = 34;
        public const int WaveFileLengthOffset = 4;

        public static readonly byte[] WaveMagicNumber = new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' };

        public static readonly Error FileTooShort = Error.New("Error reading file: file is not long enough to have a duration header");

        public static readonly Error FileTooShortWave = Error.New("Error reading file: file is not long enough to have a WAVE header");

        public static readonly byte[] DataBlockId = new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' };

        public static Fin<ulong> ReadTotalSamples(FileStream stream)
        {
            stream.Seek(WaveFileOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[5];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            // flac files have an unsigned 36-bit integer for total sample duration!
            return BinaryHelpers.Read36BitUnsignedBigEndianIgnoringFirstOctet(buffer);
        }

        public static Fin<ulong> ReadWaveSampleRate(FileStream stream)
        {
            stream.Seek(WaveSampleRateOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[4];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            if (buffer.Length < 4)
            {
                throw new ArgumentException("bytes span must at least be 4 long", nameof(buffer));
            }

            ulong dest = (ulong)buffer[3] << 24;
            dest |= (ulong)buffer[2] << 16;
            dest |= (ulong)buffer[1] << 8;
            dest |= (ulong)buffer[0] << 0;

            return dest;
        }

        public static Fin<byte> ReadWaveChannels(FileStream stream)
        {
            stream.Seek(WaveChannelOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[2];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            if (buffer.Length < 2)
            {
                throw new ArgumentException("bytes span must at least be 2 long", nameof(buffer));
            }

            byte dest = (byte)(buffer[0]);

            return dest;
        }

        public static Fin<uint> ReadWaveBitsPerSecond(FileStream stream)
        {
            stream.Seek(WaveBitsPerSampleOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[2];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            if (buffer.Length < 2)
            {
                throw new ArgumentException("bytes span must at least be 2 long", nameof(buffer));
            }

            uint dest = (uint)buffer[1] << 8;
            dest |= (uint)buffer[0] << 0;

            return dest;
        }

        public static Fin<ulong> ReadWaveFileLength(FileStream stream)
        {
            stream.Seek(WaveFileLengthOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[4];
            int bytesRead = stream.Read(buffer);

            if (bytesRead != buffer.Length)
            {
                return FileTooShort;
            }

            if (buffer.Length < 4)
            {
                throw new ArgumentException("bytes span must at least be 4 long", nameof(buffer));
            }

            ulong dest = (ulong)buffer[3] << 24;
            dest |= (ulong)buffer[2] << 16;
            dest |= (ulong)buffer[1] << 8;
            dest |= (ulong)buffer[0] << 0;
            dest += 8;

            return dest;
        }

        public static Fin<bool> IsWaveFile(FileStream stream)
        {
            stream.Seek(WaveFileOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[4];

            var read = stream.Read(buffer);

            if (read != WaveMagicNumber.Length)
            {
                return FileTooShortWave;
            }

            return buffer.StartsWith(WaveMagicNumber);
        }
    }
}
