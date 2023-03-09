// <copyright file="Wamd.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.WAMD
{
    using Emu.Models;
    using LanguageExt;
    using NodaTime;

    public record Wamd
    {
        public ushort Version { get; init; }

        public string DevModel { get; init; }

        public string DevSerialNum { get; init; }

        public string SwVersion { get; init; }

        public string DevName { get; init; }

        public Either<OffsetDateTime, LocalDateTime>? FileStartTime { get; init; }

        public Location GpsFirst { get; init; }

        public string GpsTrack { get; init; }

        public string Software { get; init; }

        public string LicenseId { get; init; }

        public string UserNotes { get; init; }

        public string AutoId { get; init; }

        public string ManualId { get; init; }

        public string VoiceNote { get; init; }

        public string AutoIdStats { get; init; }

        public ushort? TimeExpansion { get; init; }

        public string DevParams { get; init; }

        public string DevRunstate { get; init; }

        public string[] MicType { get; init; }

        public double[] MicSensitivity { get; init; }

        public Location PosLast { get; init; }

        public double? TempInt { get; init; }

        public double? TempExt { get; init; }

        public double? Humidity { get; init; }

        public double? Light { get; init; }
    }
}
