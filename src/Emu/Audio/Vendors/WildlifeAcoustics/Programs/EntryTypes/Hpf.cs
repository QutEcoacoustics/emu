// <copyright file="Hpf.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using static Emu.Utilities.BinaryHelpers;

    public record Hpf : AdvancedScheduleEntry
    {
        public Hpf()
        {
            this.Type = AdvancedScheduleEntryType.HPF;
        }

        public HighPassFilter Channel0
        {
            get
            {
                return (HighPassFilter)ReadBitRange(this.Raw, 4, 8);
            }

            init
            {
                WriteBitRange(ref this.raw, 4, 8, (uint)value);
            }
        }

        public HighPassFilter Channel1
        {
            get
            {
                return (HighPassFilter)ReadBitRange(this.Raw, 0, 4);
            }

            init
            {
                WriteBitRange(ref this.raw, 0, 4, (uint)value);
            }
        }
    }
}
