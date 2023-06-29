// <copyright file="GainSetting.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.OpenAcousticDevices
{
    using System.Runtime.Serialization;

    public enum GainSetting
    {
        [EnumMember(Value = "Low")]
        Low = 0,
        [EnumMember(Value = "Low-Medium")]
        LowMedium = 1,
        [EnumMember(Value = "Medium")]
        Medium = 2,
        [EnumMember(Value = "Medium-High")]
        MediumHigh = 3,
        [EnumMember(Value = "High")]
        High = 4,
    }
}
