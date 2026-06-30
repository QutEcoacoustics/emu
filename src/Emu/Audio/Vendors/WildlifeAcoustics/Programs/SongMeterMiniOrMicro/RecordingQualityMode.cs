// <copyright file="RecordingQualityMode.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Only relevant for the Song Meter Mini and and Mini 2 models.
    /// </summary>
    public enum RecordingQualityMode : byte
    {
        [EnumMember(Value = "High Quality")]
        HighQuality = 0,
        [EnumMember(Value = "Low Power")]
        LowPower = 1,
    }
}
