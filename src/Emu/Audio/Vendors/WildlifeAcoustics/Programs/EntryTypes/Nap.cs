// <copyright file="Nap.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using NodaTime;
    using static Emu.Utilities.BinaryHelpers;

    public record Nap : AdvancedScheduleEntry
    {
        public Nap()
        {
            this.Type = AdvancedScheduleEntryType.NAP;
        }

        // value is in minutes, convert to seconds
        public Duration Duration
        {
            get
            {
                return Duration.FromMinutes(ReadBitRange(this.Raw, 0, 8));
            }

            init
            {
                WriteBitRange(ref this.raw, 0, 8, (uint)value.TotalMinutes);
            }
        }
    }
}
