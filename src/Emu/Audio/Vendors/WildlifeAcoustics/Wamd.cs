// <copyright file="Wamd.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio
{
    using System;
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Text;
    using Emu.Audio.WAVE;
    using LanguageExt;
    using LanguageExt.Common;
    using NodaTime;
    using NodaTime.Text;

    public class Wamd
    {
        public const int ModelNameChunkId = 0x01;
        public const int ModelSerialNumberChunkId = 0x02;
        public const int FirmwareChunkId = 0x03;
        public const int StartDateChunkId = 0x05;
        public const int MicrophoneTypeChunkId = 0x12;
        public const int MicrophoneSensitivityChunkId = 0x13;
        public const int LocationChunkId = 0x14;
        public const int TemperatureChunkId = 0x15;
        public const string LatitudeKey = "Latitude";
        public const string LongitudeKey = "Longitude";
        public const string AltitudeKey = "Altitude";
        public static readonly byte[] WamdChunkId = new byte[] { (byte)'w', (byte)'a', (byte)'m', (byte)'d' };
        public static readonly Error WamdVersionError = Error.New("Error reading wamd version");
        public static readonly OffsetDateTimePattern OffsetDatePattern = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd' 'HH':'mm':'sso<m>");
        public static readonly LocalDateTimePattern LocalDatePattern = LocalDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
        public static readonly Dictionary<int, Action<Wamd, string>> Setters = new()
        {
            { ModelNameChunkId, (wamdData, value) => wamdData.Name = value },
            { ModelSerialNumberChunkId, (wamdData, value) => wamdData.SerialNumber = value },
            { FirmwareChunkId, (wamdData, value) => wamdData.Firmware = value },
            { StartDateChunkId, (wamdData, value) => wamdData.StartDate = DateParser(value) },
            { MicrophoneTypeChunkId, (wamdData, value) => wamdData.MicrophoneType = value.Split(",") },
            { MicrophoneSensitivityChunkId, (wamdData, value) => wamdData.MicrophoneSensitivity = Array.ConvertAll(value.Split(","), double.Parse) },
            {
                LocationChunkId, (wamdData, value) =>
                {
                    Dictionary<string, double> location = LocationParser(value);
                    wamdData.Latitude = location[LatitudeKey];
                    wamdData.Longitude = location[LongitudeKey];
                    wamdData.Altitude = location.ContainsKey(AltitudeKey) ? location[AltitudeKey] : null;
                }
            },
            { TemperatureChunkId, (wamdData, value) => wamdData.Temperature = double.Parse(value.Where(c => char.IsDigit(c) || (new char[] { '.', '-' }).Contains(c)).ToArray()) },
        };

        public string Name { get; set; }

        public string SerialNumber { get; set; }

        public string Firmware { get; set; }

        public double? Temperature { get; set; }

        public Either<OffsetDateTime?, LocalDateTime?> StartDate { get; set; }

        public string[] MicrophoneType { get; set; }

        public double[] MicrophoneSensitivity { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public double? Altitude { get; set; }

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
            var wamdChunk = waveChunk.Bind(w => Wave.ScanForChunk(stream, w, WamdChunkId, false));

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
        public static Either<OffsetDateTime?, LocalDateTime?> DateParser(string value)
        {
            var offsetDate = OffsetDatePattern.Parse(value);
            var localDate = LocalDatePattern.Parse(value);

            if (offsetDate.Success)
            {
                return offsetDate.Value;
            }
            else if (localDate.Success)
            {
                return localDate.Value;
            }

            throw offsetDate.Exception;
        }

        /// <summary>
        /// Parses location data.
        /// Expected format according to wamd documentation: WGS84,nn.nnnnn,N,mmm.mmmmm,W[,alt].
        /// </summary>
        /// <param name="value">The location to parse.</param>
        /// <returns>Dictionary representing the parsed location data.</returns>
        public static Dictionary<string, double> LocationParser(string value)
        {
            Dictionary<string, double> location = new Dictionary<string, double>();

            string[] locationInfo = value.Split(",");

            // First element (WGS84) is assumed to be empty, if not location format could be unpredictable
            Debug.Assert(string.IsNullOrEmpty(locationInfo[0]), $"Expected empty WGS84, instead found {locationInfo[0]}");

            double latitude = double.Parse(locationInfo[1]);
            string latitudeDirection = locationInfo[2];

            double longitude = double.Parse(locationInfo[3]);
            string longitudeDirection = locationInfo[4];

            location[LatitudeKey] = latitudeDirection.Equals("N") ? latitude : latitude * -1;
            location[LongitudeKey] = latitudeDirection.Equals("E") ? longitude : longitude * -1;

            // If location contains an altitude information, parse that as well
            if (locationInfo.Length > 5)
            {
                location[AltitudeKey] = double.Parse(locationInfo[5]);
            }

            return location;
        }

        /// <summary>
        /// Extracts metadata from a wamd chunk.
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <returns>Wamd metadata.</returns>
        public static Fin<Wamd> ExtractMetadata(Stream stream)
        {
            var wamdChunk = Wamd.GetWamdChunk(stream);

            if (wamdChunk.IsFail)
            {
                return (Error)wamdChunk;
            }

            var wamdSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)wamdChunk);

            Wamd wamdData = new Wamd();

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

                // TODO: May need to selectively parse some values with utf-8
                value = Encoding.ASCII.GetString(wamdSpan[start..end]);

                if (Setters.ContainsKey(subChunkId))
                {
                    Setters[subChunkId](wamdData, value);
                }

                wamdOffset += (int)length;
            }

            return wamdData;
        }
    }
}
