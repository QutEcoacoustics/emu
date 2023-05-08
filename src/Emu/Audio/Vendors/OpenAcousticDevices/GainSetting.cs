// <copyright file="GainSetting.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.OpenAcousticDevices
{
    using System.Runtime.Serialization;

    public enum GainSetting
    {
        [EnumMember(Value = "low")]
        Low = 0,
        [EnumMember(Value = "low-medium")]
        LowMedium = 1,
        [EnumMember(Value = "medium")]
        Medium = 2,
        [EnumMember(Value = "medium-high")]
        MediumHigh = 3,
        [EnumMember(Value = "high")]
        High = 4,
    }
}
