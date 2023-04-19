// <copyright file="FullSpectrum.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using static Emu.Utilities.BinaryHelpers;

    public record FullSpectrum : AdvancedScheduleEntry
    {
        public const int AutoSampleRate = 0x7FFFF;

        public FullSpectrum()
        {
            this.Type = AdvancedScheduleEntryType.FS;
        }

        public int SampleRate
        {
            get
            {
                return ReadBitRange(this.Raw, 0, 19) switch
                {
                    AutoSampleRate => -1,
                    uint u => (int)u,
                };
            }

            init
            {
                if (value == -1)
                {
                    WriteBitRange(ref this.raw, 0, 19, AutoSampleRate);
                }
                else if (value >= 0)
                {
                    WriteBitRange(ref this.raw, 0, 19, (uint)value);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        //public bool Channel1AutoRate
        //{
        //    get
        //    {
        //        return ReadBitRange(this.Raw, 17, 18) == 1;
        //    }

        //    init
        //    {
        //        WriteBitRange(ref this.raw, 17, 18, value ? 1u : 0);
        //    }
        //}

        public bool AutoRate
        {
            get
            {
                return ReadBitRange(this.Raw, 19, 20) == 1;
            }

            init
            {
                WriteBitRange(ref this.raw, 19, 20, value ? 1u : 0);
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

        public Format Format
        {
            get
            {
                return (Format)ReadBitRange(this.Raw, 23, 24);
            }

            init
            {
                WriteBitRange(ref this.raw, 23, 24, (uint)value);
            }
        }
    }
}
