// <copyright file="AtSunrise.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using NodaTime;
    using static Emu.Utilities.BinaryHelpers;

    public record AtSunrise : AdvancedScheduleEntry
    {
        public AtSunrise()
        {
            this.Type = AdvancedScheduleEntryType.AT_SRIS;
        }

        public Duration Offset
        {
            get
            {
                var sign = ReadBitRange(this.Raw, 18, 19) == 1 ? 1 : -1;
                var magnitude = (int)ReadBitRange(this.Raw, 0, 18) + 1;
                return Duration.FromSeconds(sign * magnitude);
            }

            init
            {
                var sign = value < Duration.Zero ? 0u : 1u;
                var magnitude = (uint)value.TotalSeconds - 1;

                WriteBitRange(ref this.raw, 18, 19, sign);
                WriteBitRange(ref this.raw, 0, 18, magnitude);
            }
        }
    }
}
