// <copyright file="FrequencyMinimum.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using static Emu.Utilities.BinaryHelpers;

    public record FrequencyMinimum : AdvancedScheduleEntry
    {
        public FrequencyMinimum()
        {
            this.Type = AdvancedScheduleEntryType.FREQMIN;
        }

        public uint Channel1
        {
            get
            {
                return ReadBitRange(this.Raw, 0, 8) * 1000u;
            }

            init
            {
                WriteBitRange(ref this.raw, 0, 8, value / 1000u);
            }
        }

        public uint Channel0
        {
            get
            {
                return ReadBitRange(this.Raw, 8, 16) * 1000u;
            }

            init
            {
                WriteBitRange(ref this.raw, 8, 16, value / 1000u);
            }
        }
    }
}
