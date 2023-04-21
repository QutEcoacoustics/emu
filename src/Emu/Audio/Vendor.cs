// <copyright file="Vendor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio
{
    using System.Runtime.Serialization;

    public enum Vendor
    {
        [EnumMember(Value = "Unknown")]
        Unknown = 0,
        [EnumMember(Value = "Frontier Labs")]
        FrontierLabs = 1,
        [EnumMember(Value = "Wildlife Acoustics")]
        WildlifeAcoustics = 2,
        [EnumMember(Value = "Open Acoustics")]
        OpenAcoustics = 3,
    }
}
