// <copyright file="AtDate.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using NodaTime;
    using static Emu.Utilities.BinaryHelpers;

    public record AtDate : AdvancedScheduleEntry
    {
        public AtDate()
        {
            this.Type = AdvancedScheduleEntryType.AT_DATE;
        }

        public LocalDate Date
        {
            get
            {
                return new(
                    (int)ReadBitRange(this.Raw, 9, 16) + ProgramParser.WildlifeAcousticsEpoch.Year,
                    (int)ReadBitRange(this.Raw, 5, 9),
                    (int)ReadBitRange(this.Raw, 0, 5));
            }

            init
            {
                WriteBitRange(
                    ref this.raw,
                    9,
                    16,
                    (byte)(value.Year - ProgramParser.WildlifeAcousticsEpoch.Year));
                WriteBitRange(ref this.raw, 5, 9, (byte)value.Month);
                WriteBitRange(ref this.raw, 0, 5, (byte)value.Day);
            }
        }
    }
}
