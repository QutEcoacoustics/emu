// <copyright file="RecordingFormat.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro
{
    using System.Runtime.Serialization;

    public enum RecordingFormat : byte
    {
        [EnumMember(Value = "Full Spectrum")]
        FullSpectrum = 1,
        [EnumMember(Value = "Zero Crossing")]
        ZeroCrossing = 2,

        [EnumMember(Value = "ZC & FS")]
        ZeroCrossingAndFullSpectrum = 3,
    }
}
