// <copyright file="Preamp.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums
{
    using System.Runtime.Serialization;

    public enum Preamp
    {
        [EnumMember(Value = "Off")]
        Off = 0,
        [EnumMember(Value = "On 26 dB")]
        On26dB = 26,
    }
}
