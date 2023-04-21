// <copyright file="Models.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics
{
    public static class Models
    {
        public const string SM3 = "SM3";
        public const string SM3BAT = "SM3BAT";
        public const string SM3M = "SM3M";
        public const string SM4 = "SM4";
        public const string SM4BATFS = "SM4BAT-FS";
        public const string SM4BATZC = "SM4BAT-ZC";

        public static bool IsSM4Variant(string variant) =>
            variant == SM4BATFS || variant == SM4 || variant == SM4BATZC;

        public static bool IsSM3Variant(string variant) =>
            variant == SM3 || variant == SM3BAT || variant == SM3M;
    }
}
