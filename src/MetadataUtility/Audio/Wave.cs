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

        //public const int WaveFileTextRIFF = 0;
        //public const int WaveFileLengthOffset = 4;

        public static readonly byte[] RiffMagicNumber = new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
        public static readonly byte[] WaveMagicNumber = new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' };
        public static readonly byte[] DataBlockId = new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' };

        //public static readonly Error FileTooShort = Error.New("Error reading file: file is not long enough to have RIFF header");
        //public static readonly Error ChunkSpanTooShort = Error.New("Error reading file: chunk size too short");
        //public static readonly Error FormatChunkMissing = Error.New("Error reading file: format chunk missing from file data");
        //public static readonly Error ByteSpanTooShort2 = Error.New("Error reading file: bytes span must at least be 2 long");
        //public static readonly Error ByteSpanTooShort4 = Error.New("Error reading file: bytes span must at least be 4 long");
        //public static readonly Error FileNotWavePCM = Error.New("Error reading file: file must be wave format PCM to be processed");

        public static readonly Error FileTooShortWave = Error.New("Error reading file: file is not long enough to have a WAVE header");
        public static readonly Error InvalidFileData = Error.New("Error reading file: no valid file data was found");

        public static Span<byte> ScanWaveFile(ReadOnlySpan<byte> waveData, string chunkName, string dataType)
        {
            int byteCount = 0;

            while (byteCount < waveData.Length)
            {
                //Get chunk name as a String
                byte[] temp = { waveData[byteCount], waveData[byteCount + 1], waveData[byteCount + 2], waveData[byteCount + 3] }; //Big endian
                chunkName = Convert.ToBase64String(temp);

                //Processing the chunk
                switch (chunkName)
                {
                    case "fmt ":
                        {
                            switch (dataType)
                            {
                                case "Channels":
                                    {
                                        int start = byteCount + 10;

                                        Span<byte> metadata = new byte[2];
                                        metadata[0] = waveData[start + 1];
                                        metadata[1] = waveData[start];

                                        return metadata;
                                    }

                                case "SampleRate":
                                    {
                                        int start = byteCount + 12;

                                        Span<byte> metadata = new byte[4];
                                        metadata[0] = waveData[start + 3];
                                        metadata[1] = waveData[start + 2];
                                        metadata[2] = waveData[start + 1];
                                        metadata[3] = waveData[start];

                                        return metadata;
                                    }

                                case "BitsPerSecond":
                                    {
                                        int start = byteCount + 22;

                                        Span<byte> metadata = new byte[2];
                                        metadata[0] = waveData[start + 1];
                                        metadata[1] = waveData[start];

                                        return metadata;
                                    }

                                default:
                                    {
                                        break;
                                    }
                            }

                            break;
                        }

                    case "data":
                        {
                            if (dataType == "TotalSamples")
                            {
                                int start = byteCount + 4;

                                Span<byte> metadata = new byte[4];
                                metadata[0] = waveData[start + 3];
                                metadata[1] = waveData[start + 2];
                                metadata[2] = waveData[start + 1];
                                metadata[3] = waveData[start];

                                return metadata;
                            }

                            break;
                        }

                    default:
                        {
                            //Get the length of the current chunk and advance that far
                            int sizeStart = byteCount + 4;
                            int chunkSize = waveData[sizeStart + 3] + waveData[sizeStart + 2] + waveData[sizeStart + 1] + waveData[sizeStart]; //Little endian

                            byteCount += chunkSize; //Skip this chunk

                            continue;
                        }
                }
            }

            Span<byte> data = new byte[0];

            return data;
        }

        //INCOMPLETE
        public static Fin<bool> IsWaveFile(FileStream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            Span<byte> riffBuffer = stackalloc byte[4];
            var readRiff = stream.Read(riffBuffer);

            if (readRiff != WaveMagicNumber.Length)
            {
                return FileTooShortWave;
            }

            return FileTooShortWave; //Changed. Use another version to get accurate function without errors
        }

        //Finished Functions**************************************************************************************************************

        public static Fin<uint> ReadWaveSampleRate(FileStream stream)
        {
            //Stream.Seek(0, SeekOrigin.Begin);
            Span<byte> waveData = stackalloc byte[(int)stream.Length];

            string chunkName = "fmt ";
            string dataType = "SampleRate";

            Span<byte> data = ScanWaveFile(waveData, chunkName, dataType);

            if (data.Length == 0)
            {
                return InvalidFileData;
            }

            uint sampleRate = BinaryPrimitives.ReadUInt32LittleEndian(data);

            return sampleRate;
        }

        public static Fin<byte> ReadWaveChannels(FileStream stream)
        {
            Span<byte> waveData = stackalloc byte[(int)stream.Length];

            string chunkName = "fmt ";
            string dataType = "Channels";

            Span<byte> data = ScanWaveFile(waveData, chunkName, dataType);

            if (data.Length == 0)
            {
                return InvalidFileData;
            }

            byte channels = data[0];

            return channels;
        }

        public static Fin<uint> ReadWaveBitsPerSecond(FileStream stream)
        {
            Span<byte> waveData = stackalloc byte[(int)stream.Length];

            string chunkName = "fmt ";
            string dataType = "BitsPerSecond";

            Span<byte> data = ScanWaveFile(waveData, chunkName, dataType);

            if (data.Length == 0)
            {
                return InvalidFileData;
            }

            uint bitsPerSecond = BinaryPrimitives.ReadUInt32LittleEndian(data);

            return bitsPerSecond;
        }

        public static Fin<ulong> ReadTotalSamples(FileStream stream)
        {
            Span<byte> waveData = stackalloc byte[(int)stream.Length];

            string chunkName = "data";
            string dataType = "TotalSamples";

            Span<byte> data = ScanWaveFile(waveData, chunkName, dataType);

            if (data.Length == 0)
            {
                return InvalidFileData;
            }

            ulong totalSamples = BinaryPrimitives.ReadUInt64LittleEndian(data);

            return totalSamples;
        }

        public static Fin<ulong> ReadWaveFileLength(FileStream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            Span<byte> waveData = stackalloc byte[(int)stream.Length];

            Span<byte> buffer = new byte[4];
            buffer[0] = waveData[4];
            buffer[1] = waveData[5];
            buffer[2] = waveData[6];
            buffer[3] = waveData[7];

            ulong dest = BinaryPrimitives.ReadUInt64LittleEndian(buffer);

            return dest + 8;
        }

        public static Fin<bool> IsWaveFilePCM(FileStream stream)
        {
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
                    int chunkSize = waveData[sizeStart + 3] + (waveData[sizeStart + 2] << 8) + (waveData[sizeStart + 1] << 16) + (waveData[sizeStart] << 24); //Getting the length of the chunk. Format: little endian

                    //sizeStart needs to be converted before used to identify waveData's position*****************************

                    byteCount += chunkSize;

                    continue;
                }
            }

            return false;
        }
    }
}
