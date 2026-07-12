// <copyright file="AtSunrise.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using System;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using NodaTime;
    using static Emu.Utilities.BinaryHelpers;

    public record AtSunrise : AdvancedScheduleEntry
    {
        private const uint MagnitudeMask = (1u << 18) - 1u;

        public AtSunrise()
        {
            this.Type = AdvancedScheduleEntryType.AT_SRIS;
        }

        public Duration Offset
        {
            get
            {
                var sign = ReadBitRange(this.Raw, 18, 19) == 1 ? 1 : -1;
                var magnitude = (int)ReadBitRange(this.Raw, 0, 18);
                var seconds = sign == 1
                    ? magnitude + 1
                    : (int)MagnitudeMask - magnitude;

                return Duration.FromSeconds(sign * seconds);
            }

            init
            {
                var totalSeconds = (long)value.TotalSeconds;
                if (totalSeconds == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Offset must not be zero (must be at least +1 second or at most -1 second).");
                }

                var sign = totalSeconds < 0 ? 0u : 1u;
                var seconds = (uint)Math.Abs(totalSeconds);
                var magnitude = sign == 1
                    ? seconds - 1
                    : MagnitudeMask - seconds;

                WriteBitRange(ref this.raw, 18, 19, sign);
                WriteBitRange(ref this.raw, 0, 18, magnitude);
            }
        }
    }
}
