// <copyright file="DurationMinimum.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using static Emu.Utilities.BinaryHelpers;

    public record DurationMinimum : AdvancedScheduleEntry
    {
        public DurationMinimum()
        {
            this.Type = AdvancedScheduleEntryType.DMIN;
        }

        public float Channel1
        {
            get
            {
                return Convert(ReadBitRange(this.Raw, 0, 13));
            }

            init
            {
                WriteBitRange(ref this.raw, 0, 13, ConvertBack(value));
            }
        }

        public float Channel0
        {
            get
            {
                return Convert(ReadBitRange(this.Raw, 13, 26));
            }

            init
            {
                WriteBitRange(ref this.raw, 13, 26, ConvertBack(value));
            }
        }

        // divide by 10 to get value as milliseconds, divide by 1000 to get seconds
        internal static float Convert(uint value) => value / 10f / 1000f;

        private static uint ConvertBack(float value) => (uint)MathF.Round(value * 1000 * 10);
    }
}
