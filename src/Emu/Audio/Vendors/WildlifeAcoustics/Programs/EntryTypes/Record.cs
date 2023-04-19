// <copyright file="Record.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using NodaTime;
    using static Emu.Utilities.BinaryHelpers;

    public record Record : AdvancedScheduleEntry
    {
        public Record()
        {
            this.Type = AdvancedScheduleEntryType.RECORD;
        }

        public Duration Duration
        {
            get
            {
                return Duration.FromSeconds(ReadBitRange(this.Raw, 0, 17));
            }

            init
            {
                WriteBitRange(ref this.raw, 0, 17, (uint)value.TotalSeconds);
            }
        }
    }
}
