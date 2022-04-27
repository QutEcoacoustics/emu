// <copyright file="Wamd.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio
{
    using System.Buffers.Binary;
    using LanguageExt;
    using LanguageExt.Common;

    public class Wamd
    {
        public static readonly byte[] WamdChunkId = new byte[] { (byte)'w', (byte)'a', (byte)'m', (byte)'d' };

        public static readonly Error InvalidFileData = Error.New("Error reading file: no valid file data was found");

        public static Fin<bool> HasVersion1WamdChunk(Stream stream)
        {
            var wamdChunk = GetWamdChunk(stream);

            if (wamdChunk.IsFail)
            {
                return (Error)wamdChunk;
            }

            var wamdSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)wamdChunk);

            throw new NotImplementedException();

            // int version = getVersion(wamdSpan);

            // return version == 1;
        }

        public static Fin<RangeHelper.Range> GetWamdChunk(Stream stream)
        {
            var riffChunk = Wave.FindRiffChunk(stream);
            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var wamdChunk = waveChunk.Bind(w => Wave.ScanForChunk(stream, w, WamdChunkId));

            if (wamdChunk.IsFail)
            {
                return (Error)wamdChunk;
            }

            return wamdChunk;
        }

        public static ushort GetVersion(ReadOnlySpan<byte> wamdChunk)
        {
            const int versionDataOffset = 6;
            ushort versionData = BinaryPrimitives.ReadUInt16LittleEndian(wamdChunk[versionDataOffset..]);

            return versionData;
        }

        public static ushort GetSubchunkId(ReadOnlySpan<byte> wamdChunk)
        {
            const int idOffset = 0;
            ushort id = BinaryPrimitives.ReadUInt16LittleEndian(wamdChunk[idOffset..]);

            return id;
        }

        public static string GetDeviceModel(ReadOnlySpan<byte> wamdChunk)
        {
            const int lengthOffset = 2;
            const int dataOffset = 6;
            string deviceModel = "";
            uint length = BinaryPrimitives.ReadUInt32LittleEndian(wamdChunk[lengthOffset..]);

            for (int i = 0; i < length; i++)
            {
                throw new NotImplementedException();
            }

            return deviceModel;
        }

        public static Wamd ExtractMetadata(ReadOnlySpan<byte> wamdSpan)
        {
            int wamdOffset = 0;
            ushort subChunkId;

            // while (wamdOffset < wamdSpan.Length)
            // {
            // Read in subchunk ID
            // Read in length

            // Assign using dictionary (set methods?)

            // Increment wamdOffset
            // }

            throw new NotImplementedException();
        }
    }
}
