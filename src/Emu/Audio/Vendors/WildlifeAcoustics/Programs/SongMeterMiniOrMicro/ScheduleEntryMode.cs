// <copyright file="ScheduleEntryMode.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Only relevant for the Song Meter Mini and Micro schedule entries.
    /// </summary>
    public enum ScheduleEntryMode : byte
    {
        [EnumMember(Value = "Ultrasonic")]
        Ultrasonic = 0,
        [EnumMember(Value = "Acoustic")]
        Acoustic = 1,
    }
}
