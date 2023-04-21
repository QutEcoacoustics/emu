// <copyright file="Gain.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using LanguageExt;
    using static Emu.Utilities.BinaryHelpers;

    public record Gain : AdvancedScheduleEntry
    {
        public Gain()
        {
            this.Type = AdvancedScheduleEntryType.GAIN;
        }

        public Either<Mode, float> Channel0
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

        public Either<Mode, float> Channel1
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

        internal static Either<Mode, float> Convert(uint value) => value switch
        {
            0xFF => Mode.Automatic,
            _ => value / 2f,
        };

        private static uint ConvertBack(Either<Mode, float> value)
        {
            return value.Match(
                v => (uint)(v * 2),
                v => v switch
                {
                    Mode.Automatic => 0xFFu,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid value"),
                });
        }
    }
}
