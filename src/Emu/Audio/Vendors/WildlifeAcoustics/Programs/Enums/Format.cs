// <copyright file="Format.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums
{
    using System.Runtime.Serialization;

    public enum Format
    {
        [EnumMember(Value = "WAVE")]
        Wave = 0,

        [EnumMember(Value = "WAC")]
        Wac = 1,
    }
}
