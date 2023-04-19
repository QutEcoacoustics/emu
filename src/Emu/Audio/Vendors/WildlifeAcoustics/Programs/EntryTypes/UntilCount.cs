// <copyright file="UntilCount.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using static Emu.Utilities.BinaryHelpers;

    public record UntilCount : AdvancedScheduleEntry
    {
        public UntilCount()
        {
            this.Type = AdvancedScheduleEntryType.UNTCOUNT;
        }

        public byte Count
        {
            get
            {
                return (byte)ReadBitRange(this.Raw, 0, 8);
            }

            init
            {
                WriteBitRange(ref this.raw, 0, 8, value);
            }
        }
    }
}
