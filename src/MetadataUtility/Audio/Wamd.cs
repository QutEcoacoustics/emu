// <copyright file="Wamd.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio
{
    using System;
    using System.Buffers.Binary;
    using System.Text;
    using LanguageExt;
    using LanguageExt.Common;
    using NodaTime;
    using NodaTime.Text;

    public class Wamd
    {
        public static readonly byte[] WamdChunkId = new byte[] { (byte)'w', (byte)'a', (byte)'m', (byte)'d' };

        public static readonly Error WamdVersionError = Error.New("Error reading wamd version");

        private string Name { get; set; }

        private string SerialNumber { get; set; }

        private string Firmware { get; set; }

        private string Temperature { get; set; }

        private OffsetDateTime? StartDate { get; set; }

        private string MicrophoneType { get; set; }

        private string MicrophoneSensitivity { get; set; }

        private double Longitude { get; set; }

        private double Latitude { get; set; }

        /// <summary>
        /// Check if file has version 1 wamd chunk.
        /// Wamd chunk contains metadata for wildlife acoustics sensors.
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <returns>Boolean indicating weather the wamd chunk is present.</returns>
        public static Fin<bool> HasVersion1WamdChunk(Stream stream)
        {
            var wamdChunk = GetWamdChunk(stream);

            if (wamdChunk.IsFail)
            {
                return (Error)wamdChunk;
            }

            var wamdSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)wamdChunk);

            var version = GetVersion(wamdSpan);

            if (version.IsFail)
            {
                return (Error)version;
            }

            return version == 1;
        }

        /// <summary>
        /// Scans for the position of the wamd chunk in the file stream.
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <returns>Range representing the position of the wamd chunk.</returns>
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

        /// <summary>
        /// Extracts the wamd chunk version.
        /// Wamd version has a sub chunk ID of 0, and is always first in the wamd chunk.
        /// </summary>
        /// <param name="wamdSpan">The wamd chunk.</param>
        /// <returns>Wamd chunk version.</returns>
        public static Fin<ushort> GetVersion(ReadOnlySpan<byte> wamdSpan)
        {
            int wamdOffset = 0;

            ushort subChunkId = BinaryPrimitives.ReadUInt16LittleEndian(wamdSpan[wamdOffset..]);
            wamdOffset += 2;

            uint length = BinaryPrimitives.ReadUInt32LittleEndian(wamdSpan[wamdOffset..]);
            wamdOffset += 4;

            if (subChunkId != 0 || length != 2)
            {
                return WamdVersionError;
            }

            ushort version = BinaryPrimitives.ReadUInt16LittleEndian(wamdSpan[wamdOffset..]);

            return version;
        }

        /// <summary>
        /// Parses a date into Nodatime's OffsetDateTime.
        /// </summary>
        /// <param name="value">The date to parse.</param>
        /// <returns>The parsed date.</returns>
        public static OffsetDateTime? DateParser(string value)
        {
            OffsetDateTime? date = null;

            try
            {
                date = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd' 'HH':'mm':'sso<m>").Parse(value).Value;
            }
            catch (UnparsableValueException)
            {
                return null;
            }

            return date;
        }

        /// <summary>
        /// Parses a location's longitude and latitude.
        /// Assigns the location data to a given wamd object.
        /// </summary>
        /// <param name="value">The location to parse.</param>
        /// <param name="wamdData">The wamd object.</param>
        public static void SetLocation(string value, Wamd wamdData)
        {
            string[] locationInfo = value.Split(",", StringSplitOptions.RemoveEmptyEntries);

            double latitude = double.Parse(locationInfo[0]);
            string latitudeDirection = locationInfo[1];

            double longitude = double.Parse(locationInfo[2]);
            string longitudeDirection = locationInfo[3];

            wamdData.Latitude = latitudeDirection.Equals("N") ? latitude : latitude * -1;
            wamdData.Longitude = latitudeDirection.Equals("E") ? longitude : longitude * -1;
        }

        /// <summary>
        /// Extracts metadata from a wamd chunk.
        /// </summary>
        /// <param name="wamdSpan">The wamd chunk.</param>
        public static Wamd ExtractMetadata(ReadOnlySpan<byte> wamdSpan)
        {
            Wamd wamdData = new Wamd();

            // Links metadata set functions to their corresponding subchunk ID
            Dictionary<int, Action<string>> setters = new Dictionary<int, Action<string>>
            {
                { 1, value => wamdData.Name = value },
                { 2, value => wamdData.SerialNumber = value },
                { 3, value => wamdData.Firmware = value },
                { 5, value => wamdData.StartDate = DateParser(value) },
                { 18, value => wamdData.MicrophoneType = value },
                { 19, value => wamdData.MicrophoneSensitivity = value },
                { 20, value => SetLocation(value, wamdData) },
                { 21, value => wamdData.Temperature = value },
            };

            int wamdOffset = 0;
            ushort subChunkId;
            uint length;
            string value;

            // Parse each piece of metadata in the wamd chunk
            while (wamdOffset < wamdSpan.Length)
            {
                subChunkId = BinaryPrimitives.ReadUInt16LittleEndian(wamdSpan[wamdOffset..]);
                wamdOffset += 2;

                length = BinaryPrimitives.ReadUInt32LittleEndian(wamdSpan[wamdOffset..]);
                wamdOffset += 4;

                int start = wamdOffset;
                int end = wamdOffset + (int)length;

                value = Encoding.ASCII.GetString(wamdSpan[start..end]);

                if (setters.ContainsKey(subChunkId))
                {
                    setters[subChunkId](value);
                }

                wamdOffset += (int)length;
            }

            return wamdData;
        }
    }
}
