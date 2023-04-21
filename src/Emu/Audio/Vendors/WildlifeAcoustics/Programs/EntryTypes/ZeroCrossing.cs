// <copyright file="ZeroCrossing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using static Emu.Utilities.BinaryHelpers;

    public record ZeroCrossing : AdvancedScheduleEntry
    {
        public ZeroCrossing()
        {
            this.Type = AdvancedScheduleEntryType.ZC;
        }

        public SM3DivisionRatio ZeroCrossingMode
        {
            get
            {
                return (SM3DivisionRatio)ReadBitRange(this.Raw, 0, 2);
            }

            init
            {
                WriteBitRange(ref this.raw, 0, 2, (uint)value);
            }
        }

        public Channel Channel
        {
            get
            {
                return (Channel)ReadBitRange(this.Raw, 20, 23);
            }

            init
            {
                WriteBitRange(ref this.raw, 20, 23, (uint)value);
            }
        }
    }
}
