// <copyright file="ScheduleEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro
{
    using System;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using static Emu.Utilities.BinaryHelpers;
    using Duration = NodaTime.Duration;

    public record ScheduleEntry
    {
        private const byte NegativeTime = 1;
        private const byte PositiveTime = 0;

        // the first 4 bytes
        private readonly uint main1;

        // the second 4 bytes
        private readonly uint main2;

        // the overflow section
        private readonly ushort overflow;

        public ScheduleEntry(uint main1, uint main2, ushort overflow)
        {
            this.main1 = main1;
            this.main2 = main2;
            this.overflow = overflow;
        }

        public ScheduleEntry()
        {
            // default to acoustic mode
            this.main2 = 0x80_00_00_00;
            this.main1 = 0;
            this.overflow = 0;
        }

        public ScheduleEntryMode Mode
        {
            get => (ScheduleEntryMode)ReadHighBit(this.main2);
            init => WriteHighBit(ref this.main2, (byte)value);
        }

        public EventType StartType
        {
            get => (EventType)ReadBitRange(this.main1, 28, 30);
            init => WriteBitRange(ref this.main1, 28, 30, (byte)value);
        }

        /// <remarks>
        /// This is a duration so it can be relative to <see cref="StartType"/>.
        /// </remarks>
        public Duration StartTime
        {
            get
            {
                var sign = ReadBitRange(this.main1, 27, 28) == NegativeTime ? -1 : 1;
                var magnitude = ReadBitRange(this.main1, 16, 27);
                return Duration.FromMinutes(sign * magnitude);
            }

            init
            {
                var totalMinutes = (long)value.TotalMinutes;
                byte sign = totalMinutes < 0 ? NegativeTime : PositiveTime;
                uint magnitude = (uint)Math.Abs(totalMinutes);

                WriteBitRange(ref this.main1, 27, 28, sign);
                WriteBitRange(ref this.main1, 16, 27, magnitude);
            }
        }

        public EventType EndType
        {
            get => (EventType)ReadBitRange(this.main1, 12, 14);
            init => WriteBitRange(ref this.main1, 12, 14, (byte)value);
        }

        /// <remarks>
        /// This is a duration so it can be relative to <see cref="EndType"/>.
        /// </remarks>
        public Duration EndTime
        {
            get
            {
                var sign = ReadBitRange(this.main1, 11, 12) == NegativeTime ? -1 : 1;
                var magnitude = ReadBitRange(this.main1, 0, 11);
                return Duration.FromMinutes(sign * magnitude);
            }

            init
            {
                var totalMinutes = (long)value.TotalMinutes;
                byte sign = totalMinutes < 0 ? NegativeTime : PositiveTime;
                uint magnitude = (uint)Math.Abs(totalMinutes);

                WriteBitRange(ref this.main1, 11, 12, sign);
                WriteBitRange(ref this.main1, 0, 11, magnitude);
            }
        }

        public Duration? DutyCycleOn
        {
            get
            {
                var minutes = ReadBitRange(this.main2, 11, 22);
                return minutes == 0 ? null : Duration.FromMinutes(minutes);
            }

            init
            {
                var minutes = (uint)(value?.TotalMinutes ?? 0);
                WriteBitRange(ref this.main2, 11, 22, minutes);
            }
        }

        public Duration? DutyCycleOff
        {
            get
            {
                var minutes = ReadBitRange(this.main2, 0, 11);
                return minutes == 0 ? null : Duration.FromMinutes(minutes);
            }

            init
            {
                var minutes = (uint)(value?.TotalMinutes ?? 0);
                WriteBitRange(ref this.main2, 0, 11, minutes);
            }
        }

        public byte? StartDateMonth
        {
            get
            {
                // this value is split into two parts
                var highBits = ReadBitRange(this.main1, 30, 32);
                var lowBits = ReadBitRange(this.main1, 14, 16);

                var month = (byte)((highBits << 2) | lowBits);
                return month == 0 ? null : month;
            }

            init
            {
                uint month = value ?? 0;
                var highBits = (month >> 2) & 0b11;
                var lowBits = month & 0b11;

                WriteBitRange(ref this.main1, 30, 32, highBits);
                WriteBitRange(ref this.main1, 14, 16, lowBits);
            }
        }

        public byte? StartDateDay
        {
            get
            {
                var day = ReadBitRange(this.overflow, 11, 16);
                return day == 0 ? null : (byte)day;
            }

            init
            {
                var day = value ?? 0;
                WriteBitRange(ref this.overflow, 11, 16, day);
            }
        }

        public byte? EndDateMonth
        {
            get
            {
                var month = ReadBitRange(this.main2, 27, 31);
                return month == 0 ? null : (byte)month;
            }

            init
            {
                var month = (uint)(value ?? 0);
                WriteBitRange(ref this.main2, 27, 31, month);
            }
        }

        public byte? EndDateDay
        {
            get
            {
                var day = ReadBitRange(this.main2, 22, 27);
                return day == 0 ? null : (byte)day;
            }

            init
            {
                var day = (uint)(value ?? 0);
                WriteBitRange(ref this.main2, 22, 27, day);
            }
        }

        public byte? DutyCycleDaysOn
        {
            get
            {
                var on = (byte)ReadBitRange(this.overflow, 6, 11);
                return on == 0 ? null : on;
            }

            init
            {
                ushort on = value ?? 0;
                WriteBitRange(ref this.overflow, 6, 11, on);
            }
        }

        public byte? DutyCycleDaysOff
        {
            get
            {
                var off = (byte)ReadBitRange(this.overflow, 1, 6);
                return off == 0 ? null : off;
            }

            init
            {
                byte off = value ?? 0;
                WriteBitRange(ref this.overflow, 1, 6, off);
            }
        }

        public byte UnknownBit
        {
            get => (byte)ReadBitRange(this.overflow, 0, 1);
            init => WriteBitRange(ref this.overflow, 0, 1, value);
        }
    }
}
