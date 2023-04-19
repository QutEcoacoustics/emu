// <copyright file="ScheduleMode.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums
{
    using System.Runtime.Serialization;

    public enum ScheduleMode : byte
    {
        [EnumMember(Value = "Simple")]
        Daily = 0,

        [EnumMember(Value = "Advanced")]
        Advanced = 1,
    }
}
