// <copyright file="EventType.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums
{
    using System.Runtime.Serialization;

    public enum EventType
    {
        [EnumMember(Value = "Time of Day")]
        TimeOfDay = 0,
        [EnumMember(Value = "Sunrise")]
        Sunrise = 1,
        [EnumMember(Value = "Sunset")]
        Sunset = 2,
    }
}
