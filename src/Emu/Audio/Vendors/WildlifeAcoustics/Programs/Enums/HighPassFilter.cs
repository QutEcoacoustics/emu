// <copyright file="HighPassFilter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums
{
    using System.Runtime.Serialization;

    public enum HighPassFilter
    {
        [EnumMember(Value = "Off")]
        Off = 0,

        [EnumMember(Value = "220 Hz")]
        On220Hz = 1,

        [EnumMember(Value = "1000 Hz")]
        On1000Hz = 2,

        [EnumMember(Value = "16 kHz")]
        On16000Hz = 3,
    }
}
