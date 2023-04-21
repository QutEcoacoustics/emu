// <copyright file="TriggerLevel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using System;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using LanguageExt;
    using static Emu.Utilities.BinaryHelpers;

    public record TriggerLevel : AdvancedScheduleEntry
    {
        public TriggerLevel()
        {
            this.Type = AdvancedScheduleEntryType.TRGLVL;
        }

        public Either<Mode, sbyte> Channel1
        {
            get
            {
                return Convert(ReadBitRange(this.Raw, 0, 8));
            }

            init
            {
                WriteBitRange(ref this.raw, 0, 8, ConvertBack(value));
            }
        }

        public Either<Mode, sbyte> Channel0
        {
            get
            {
                return Convert(ReadBitRange(this.Raw, 8, 16));
            }

            init
            {
                WriteBitRange(ref this.raw, 8, 16, ConvertBack(value));
            }
        }

        private static Either<Mode, sbyte> Convert(uint value)
        {
            const byte mask = 0b1000_0000;

            return value switch
            {
                (uint)Mode.Off => Mode.Off,
                (uint)Mode.Automatic => Mode.Automatic,
                0 => 1,
                uint p when (p & mask) == mask => (sbyte)(((sbyte)((byte)p & ~mask)) + 1),
                uint n when (n & mask) == 0 => (sbyte)((127 - (sbyte)n) * -1),
                _ => throw new NotSupportedException(),
            };
        }

        private static uint ConvertBack(Either<Mode, sbyte> value)
        {
            return value.Case switch
            {
                0 => throw new NotSupportedException("0 dB is not supported"),
                Mode m => (uint)m,
                sbyte n when n < 0 => (byte)(127 - (n * -1)),
                sbyte p when p > 0 => (uint)((byte)(p - 1) | 0b1000_0000),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
