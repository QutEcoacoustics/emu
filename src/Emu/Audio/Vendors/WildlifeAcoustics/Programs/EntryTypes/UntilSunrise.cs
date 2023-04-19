// <copyright file="UntilSunrise.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    public record UntilSunrise : AtSunrise
    {
        public UntilSunrise()
        {
            this.Type = AdvancedScheduleEntryType.UNTSRIS;
        }
    }
}
