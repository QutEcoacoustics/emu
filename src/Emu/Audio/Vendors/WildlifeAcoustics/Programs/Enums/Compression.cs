// <copyright file="Compression.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums
{
    using System.Runtime.Serialization;

    public enum Compression
    {
        [EnumMember(Value = "None")]

        None = 0,

        [EnumMember(Value = "W4V-8")]

        W4V8 = 8,

        [EnumMember(Value = "W4V-6")]

        W4V6 = 10,

        [EnumMember(Value = "W4V-4")]

        W4V4 = 12,
    }
}
