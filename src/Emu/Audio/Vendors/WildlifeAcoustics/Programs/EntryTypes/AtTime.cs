// <copyright file="AtTime.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using NodaTime;
    using static Emu.Utilities.BinaryHelpers;

    public record AtTime : AdvancedScheduleEntry
    {
        public AtTime()
        {
            this.Type = AdvancedScheduleEntryType.AT_TIME;
        }

        public LocalTime Time
        {
            get
            {
                return LocalTime.FromSecondsSinceMidnight((int)ReadBitRange(this.Raw, 0, 17));
            }

            init
            {
                var seconds = (value - LocalTime.Midnight).ToDuration().TotalSeconds;
                WriteBitRange(ref this.raw, 0, 17, (uint)seconds);
            }
        }
    }
}
