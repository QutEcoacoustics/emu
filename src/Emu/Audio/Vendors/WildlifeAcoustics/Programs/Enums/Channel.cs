// <copyright file="Channel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums
{
    public enum Channel
    {
        Left = 0,
        Right = 1,
        Stereo = 2,

        // bottom two only used on SM3s
        Off = 6,
        Auto = 7,
    }
}
