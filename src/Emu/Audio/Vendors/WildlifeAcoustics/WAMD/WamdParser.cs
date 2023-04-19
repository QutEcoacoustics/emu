// <copyright file="WamdParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.WAMD
{
    using System;
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Text;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using Emu.Audio.WAVE;
    using Emu.Models;
    using LanguageExt;
    using LanguageExt.Common;
    using Newtonsoft.Json.Converters;
    using NodaTime;
    using NodaTime.Text;
    using UnitsNet;
    using UnitsNet.NumberExtensions.NumberToLength;
    using UnitsNet.NumberExtensions.NumberToLuminousFlux;
    using UnitsNet.NumberExtensions.NumberToLuminousIntensity;
    using static SubChunkId;
    using Error = LanguageExt.Common.Error;

    public static class WamdParser
    {
        public const string LatitudeKey = "Latitude";
        public const string LongitudeKey = "Longitude";
        public const string AltitudeKey = "Altitude";
        public static readonly byte[] WamdChunkId = "wamd"u8.ToArray();
        public static readonly Error WamdVersionError = Error.New("Error reading wamd version");
        public static readonly OffsetDateTimePattern OffsetDatePattern = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFo<m>");
        public static readonly LocalDateTimePattern LocalDatePattern = LocalDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd' 'HH':'mm':'ss");

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
        public static Either<OffsetDateTime, LocalDateTime> DateParser(string value)
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
        public static Location LocationParser(string value)
        {
            string[] locationInfo = value.Split(",");

            // First element (WGS84) is assumed to be empty, if not location format could be unpredictable
            if (!string.IsNullOrEmpty(locationInfo[0]))
            {
              throw new NotSupportedException($"Expected empty WGS84, instead found {locationInfo[0]}" + Meta.CallToAction);
            }

            string latitude = locationInfo[1];
            string latitudeDirection = locationInfo[2];

            string longitude = locationInfo[3];
            string longitudeDirection = locationInfo[4];

            // If location contains an altitude information, parse that as well
            return new Location(
                    latitudeDirection + latitude,
                    longitudeDirection + longitude,
                    locationInfo.Length > 5 ? locationInfo[5] : null,
                    locationInfo[0]);
        }

        /// <summary>
        /// Reads the light meter value as candela.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The luminosity in candela.</returns>
        public static double LightParser(string value)
        {
            return double.Parse(value).Lumens().Candela().Value;
        }

        public static double TemperatureParser(string value)
        {
            if (value.EndsWith("C"))
            {
                return double.Parse(value[0..^1]);
            }

            throw new NotSupportedException("Found temperature not in Celsius. " + Meta.CallToAction);
        }

        /// <summary>
        /// Extracts metadata from a wamd chunk.
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <returns>Wamd metadata.</returns>
        public static Fin<Wamd> ExtractMetadata(Stream stream)
        {
            var wamdChunk = GetWamdChunk(stream);

            if (wamdChunk.IsFail)
            {
                return (Error)wamdChunk;
            }

            var wamdSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)wamdChunk);

            Wamd wamdData = new Wamd();

            int wamdOffset = 0;
            ushort subChunkId;
            uint length;

            // Parse each piece of metadata in the wamd chunk
            while (wamdOffset < wamdSpan.Length)
            {
                subChunkId = BinaryPrimitives.ReadUInt16LittleEndian(wamdSpan[wamdOffset..]);
                wamdOffset += 2;

                length = BinaryPrimitives.ReadUInt32LittleEndian(wamdSpan[wamdOffset..]);
                wamdOffset += 4;

                int start = wamdOffset;
                int end = wamdOffset + (int)length;
                var value = wamdSpan[start..end];

                wamdData = ParseSubChunk(wamdData, subChunkId, value);
                wamdOffset += (int)length;
            }

            return wamdData;
        }

        public static Wamd ParseSubChunk(Wamd wamd, ushort subChunkId, ReadOnlySpan<byte> value)
        {
            return (SubChunkId)subChunkId switch
            {
                SubChunkId.Version => wamd with { Version = BinaryPrimitives.ReadUInt16LittleEndian(value) },
                DevModel => wamd with { DevModel = GetString(value) },
                DevSerialNum => wamd with { DevSerialNum = GetString(value) },
                SwVersion => wamd with { SwVersion = GetString(value) },
                DevName => wamd with { DevName = GetString(value) },
                FileStartTime => wamd with { FileStartTime = DateParser(GetString(value)) },
                GpsFirst => wamd with { GpsFirst = LocationParser(GetString(value)) },
                GpsTrack => throw new NotImplementedException("WA says GPS Track is not yet implemented. " + Meta.CallToAction),
                Software => wamd with { Software = GetString(value) },
                LicenseId => wamd with { LicenseId = GetString(value) },
                UserNotes => wamd with { UserNotes = GetString(value) },
                AutoId => wamd with { AutoId = GetString(value) },
                ManualId => wamd with { ManualId = GetString(value) },
                VoiceNote => wamd with { VoiceNote = "<<Voice Notes detected but EMU does not support parsing them>>" },
                AutoIdStats => wamd with { AutoIdStats = GetString(value) },
                TimeExpansion => wamd with { TimeExpansion = BinaryPrimitives.ReadUInt16LittleEndian(value) },

                // we don't know how to parse these two, their description is opaque

                //DevRunstate => wamd with { DevRunstate = Encoding.UTF8.GetBytes(value).ToHexString() },
                DevParams => wamd with { DevParams = ProgramParser.Parse(value).ThrowIfFail() },
                DevRunstate => wamd with { DevRunstate = "<<Dev Runstate detected but EMU does not support parsing it>>" },

                MicType => wamd with { MicType = ParseList<string>(GetString(value)) },
                MicSensitivity => wamd with { MicSensitivity = ParseList(GetString(value), double.Parse) },
                PosLast => wamd with { PosLast = LocationParser(GetString(value)) },
                TempInt => wamd with { TempInt = TemperatureParser(GetString(value)) },
                TempExt => wamd with { TempExt = TemperatureParser(GetString(value)) },
                Humidity => wamd with { Humidity = double.Parse(GetString(value)) },
                Light => wamd with { Light = LightParser(GetString(value)) },

                // ignore padding
                Padding => wamd,

                _ => throw new NotImplementedException(
                    "Unexpected WAMD sub chunk. Don't know how to process: "
                    + Encoding.ASCII.GetString(BitConverter.GetBytes(subChunkId))),
            };

            // TODO: May need to selectively parse some values with utf-8
            static string GetString(ReadOnlySpan<byte> value) => Encoding.ASCII.GetString(value);
        }

        private static T[] ParseList<T>(string value, Func<string, T> parser = null)
        {
            var split = value.Split(",");
            if (parser is not null)
            {
                return split.Select(parser).ToArray();
            }
            else
            {
                return split.Cast<T>().ToArray();
            }
        }
    }
}
