// <copyright file="Vendor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio
{
    public static class VendorExtensions
    {
        public static string ToNiceName(this Vendor vendor)
        {
            return vendor switch
            {
                Vendor.FrontierLabs => "Frontier Labs",
                Vendor.WildlifeAcoustics => "Wildlife Acoustics",
                Vendor.OpenAcoustics => "Open Acoustics",
                _ => "Unknown",
            };
        }
    }
}
