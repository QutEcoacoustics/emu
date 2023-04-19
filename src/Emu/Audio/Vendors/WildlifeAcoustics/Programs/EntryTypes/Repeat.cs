// <copyright file="Repeat.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;

    public record Repeat : AdvancedScheduleEntry
    {
        public Repeat()
        {
            this.Type = AdvancedScheduleEntryType.REPEAT;
        }
    }
}
