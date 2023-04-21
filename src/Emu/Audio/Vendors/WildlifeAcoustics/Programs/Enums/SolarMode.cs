// <copyright file="SolarMode.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums
{
    public enum SolarMode : byte
    {
        Actual = 0xFF,
        Civil = 0x00,
        Nautical = 0x01,
        Astronomical = 0x02,
    }
}
