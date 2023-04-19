// <copyright file="SongMeterMicrophone.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums
{
    using System.Runtime.Serialization;

    public enum SongMeterMicrophone
    {
        [EnumMember(Value = "unknown/internal (SM3?)")]
        Unknown = 0,

        [EnumMember(Value = "SMM-A1/SM3-A1")]
        SMM_A1 = 1,

        [EnumMember(Value = "SMM-A2")]
        SMM_A2 = 2,

        [EnumMember(Value = "SMM-U1/SM3-U1")]
        SMM_U1 = 3,

        [EnumMember(Value = "SMM-U2")]
        SMM_U2 = 4,

        [EnumMember(Value = "SMM-H1")]
        SMM_H1 = 5,

        [EnumMember(Value = "SMM-H2")]
        SMM_H2 = 6,

        [EnumMember(Value = "Internal")]
        Internal = 7,
    }
}
