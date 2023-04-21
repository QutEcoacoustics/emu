// <copyright file="TriggerWindow.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using static Emu.Utilities.BinaryHelpers;

    public record TriggerWindow : AdvancedScheduleEntry
    {
        public TriggerWindow()
        {
            this.Type = AdvancedScheduleEntryType.TRGWIN;
        }

        public float Channel1
        {
            get
            {
                return Convert(ReadBitRange(this.Raw, 0, 10));
            }

            init
            {
                WriteBitRange(ref this.raw, 0, 10, ConvertBack(value));
            }
        }

        public float Channel0
        {
            get
            {
                return Convert(ReadBitRange(this.Raw, 10, 20));
            }

            init
            {
                WriteBitRange(ref this.raw, 10, 20, ConvertBack(value));
            }
        }

        // divide by 10 to get value as seconds
        internal static float Convert(uint value) => value / 10f;

        private static uint ConvertBack(float value) => (uint)(value * 10);
    }
}
