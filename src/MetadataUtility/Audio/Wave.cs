// <copyright file="Wave.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio
{
    using System.Buffers.Binary;
    using LanguageExt;
    using LanguageExt.Common;

    public static class Wave
    {
        public const string Mime = "audio/wave";

        public static readonly byte[] RiffMagicNumber = new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
        public static readonly byte[] WaveMagicNumber = new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' };
        public static readonly byte[] FormatBlockId = new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' };
        public static readonly byte[] DataBlockId = new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' };

        //public static readonly Error FileTooShort = Error.New("Error reading file: file is not long enough to have RIFF header");
        //public static readonly Error ChunkSpanTooShort = Error.New("Error reading file: chunk size too short");
        //public static readonly Error FormatChunkMissing = Error.New("Error reading file: format chunk missing from file data");
        //public static readonly Error ByteSpanTooShort2 = Error.New("Error reading file: bytes span must at least be 2 long");
        //public static readonly Error ByteSpanTooShort4 = Error.New("Error reading file: bytes span must at least be 4 long");
        //public static readonly Error FileNotWavePCM = Error.New("Error reading file: file must be wave format PCM to be processed");

        public static readonly Error FileTooShortWave = Error.New("Error reading file: file is not long enough to have a WAVE header");
        public static readonly Error InvalidFileData = Error.New("Error reading file: no valid file data was found");

        public static Fin<ulong> ReadWaveFileLength(FileStream stream)
        {
            Span<byte> waveData = stackalloc byte[4];

            stream.Seek(4, SeekOrigin.Begin);
            var read = stream.Read(waveData);

            ulong length = BinaryPrimitives.ReadUInt32LittleEndian(waveData);

            return length + 8;
        }

        public static byte[] GetWaveFormatChunk(FileStream stream)
        {
            int byteCount = 12;
            string chunkName = string.Empty;
            int fmtLength = 0;
            byte[] data = new byte[fmtLength];

            Span<byte> waveData = stackalloc byte[4];

            stream.Seek(byteCount, SeekOrigin.Begin);
            var read = stream.Read(waveData);

            while (byteCount < stream.Length)
            {
                stream.Seek(byteCount, SeekOrigin.Begin);
                read = stream.Read(waveData);

                Int32 name = BinaryPrimitives.ReadInt32BigEndian(waveData);

                if (name == BinaryPrimitives.ReadInt32BigEndian(FormatBlockId))
                {
                    int fmtLengthStart = byteCount + 4;

                    //Get value of format length
                    stream.Seek(fmtLengthStart, SeekOrigin.Begin);
                    read = stream.Read(waveData);

                    fmtLength = BinaryPrimitives.ReadInt32LittleEndian(waveData);

                    //Get byte[] from byteCount to the end of the format chunk

                    waveData = stackalloc byte[fmtLength];

                    fmtLengthStart += 4;

                    stream.Seek(fmtLengthStart, SeekOrigin.Begin);
                    read = stream.Read(waveData);

                    data = new byte[fmtLength]; //Empty memory with a length of the format chunk

                    //Now I need to fill the array
                    for (int i = 0; i < fmtLength; i++)
                    {
                        data[i] = waveData[i];
                    }

                    //return the chunk
                    return data;
                }

                int sizeStart = byteCount + 4;

                int chunkSize = BinaryPrimitives.ReadInt32LittleEndian(waveData[sizeStart..]); //Getting the length of the chunk. Format: little endian

                byteCount += chunkSize;
            }

            return data;
        }

        public static Fin<uint> ReadWaveSampleRate(byte[] fmtChunk)
        {
            byte[] data = new byte[4];

            data[0] = fmtChunk[4];
            data[1] = fmtChunk[5];
            data[2] = fmtChunk[6];
            data[3] = fmtChunk[7];

            uint sampleRate = BinaryPrimitives.ReadUInt32LittleEndian(data);

            return sampleRate;
        }

        public static Fin<byte> ReadWaveChannels(byte[] fmtChunk)
        {
            byte[] data = new byte[2];

            data[0] = fmtChunk[2];
            data[1] = fmtChunk[3];

            byte channels = data[0];

            return channels;
        }

        public static Fin<uint> ReadWaveBitsPerSecond(byte[] fmtChunk)
        {
            byte[] data = new byte[2];

            data[0] = fmtChunk[14];
            data[1] = fmtChunk[15];

            uint bitsPerSecond = BinaryPrimitives.ReadUInt16LittleEndian(data);

            return bitsPerSecond;
        }

        public static Fin<ulong> ReadTotalSamples(FileStream stream)
        {
            int byteCount = 12;
            string chunkName = string.Empty;
            ulong dataLength = 0;
            byte[] data = new byte[dataLength];

            Span<byte> waveData = stackalloc byte[4];

            stream.Seek(byteCount, SeekOrigin.Begin);
            var read = stream.Read(waveData);

            while (byteCount < stream.Length)
            {
                stream.Seek(byteCount, SeekOrigin.Begin);
                read = stream.Read(waveData);

                Int32 name = BinaryPrimitives.ReadInt32BigEndian(waveData);

                if (name == BinaryPrimitives.ReadInt32BigEndian(DataBlockId))
                {
                    int dataLengthStart = byteCount + 4;

                    //Get value of format length
                    stream.Seek(dataLengthStart, SeekOrigin.Begin);
                    read = stream.Read(waveData);

                    dataLength = BinaryPrimitives.ReadUInt32LittleEndian(waveData);

                    //return data length
                    return dataLength;
                }

                int sizeStart = byteCount + 4;
                byteCount += 8;

                stream.Seek(sizeStart, SeekOrigin.Begin);
                read = stream.Read(waveData);

                int chunkSize = BinaryPrimitives.ReadInt32LittleEndian(waveData); //Getting the length of the chunk. Format: little endian

                byteCount += chunkSize;
            }

            return InvalidFileData;
        }

        public static Fin<bool> IsWaveFilePCM(FileStream stream)
        {
            return true;

            Span<byte> waveData = stackalloc byte[(int)stream.Length];

            var read = stream.Read(waveData);

            int byteCount = 0;
            string chunkName = string.Empty;

            //Is file long enough to be a RIFF file?
            if (waveData.Length < RiffMagicNumber.Length)
            {
                return false;
            }

            //Do the proper bytes match "RIFF"
            if (!waveData.StartsWith(RiffMagicNumber))
            {
                return false;
            }

            //Is file long enough to be a WAVE file?
            if (waveData.Length < WaveMagicNumber.Length)
            {
                return false;
            }

            //Do the proper bytes match "WAVE"
            if (waveData[8] != WaveMagicNumber[0] || waveData[9] != WaveMagicNumber[1] || waveData[10] != WaveMagicNumber[2] || waveData[11] != WaveMagicNumber[3])
            {
                return false;
            }

            while (byteCount < waveData.Length)
            {
                //Get chunk name as a String
                byte[] temp = { waveData[byteCount], waveData[byteCount + 1], waveData[byteCount + 2], waveData[byteCount + 3] }; //Big endian
                chunkName = Convert.ToHexString(temp);

                if (chunkName == "fmt ")
                {
                    int x = byteCount + 6; //Skipping to the beginning of AudioFormat bytes
                    int audioFormat = (int)waveData[x + 1] + (int)waveData[x]; //Little endian

                    //Checks if format is PCM and if bytes 8 - 11 are "WAVE"
                    if (audioFormat == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    //Get the length of the current chunk and advance that far
                    int sizeStart = byteCount + 4;

                    //int chunkSize = waveData[sizeStart + 3] + (waveData[sizeStart + 2] << 8) + (waveData[sizeStart + 1] << 16) + (waveData[sizeStart] << 24); //Getting the length of the chunk. Format: little endian

                    int chunkSize = BinaryPrimitives.ReadInt32LittleEndian(waveData[sizeStart..]); //Getting the length of the chunk. Format: little endian

                    //sizeStart needs to be converted before used to identify waveData's position*****************************

                    byteCount += chunkSize;

                    continue;
                }
            }

            return false;
        }
    }
}
