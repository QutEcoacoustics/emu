// <copyright file="AtSunset.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    public record AtSunset : AtSunrise
    {
        public AtSunset()
        {
            this.Type = AdvancedScheduleEntryType.AT_SSET;
        }
    }
}
