// <copyright file="SimpleScheduleEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using NodaTime;
    using static Emu.Utilities.BinaryHelpers;
    using Duration = NodaTime.Duration;

    /// <summary>
    /// See WildlifeAcoustics\schedule_structure.md for more information.
    /// </summary>
    public partial record SimpleScheduleEntry
    {
        private readonly ulong raw = 0;

        public SimpleScheduleEntry(ulong raw)
        {
            this.raw = raw;
        }

        public SimpleScheduleEntry(
            EventType startType,
            Duration start,
            EventType endType,
            Duration end,
            Duration off = default,
            Duration on = default)
        {
            this.StartType = startType;
            this.Start = start;
            this.EndType = endType;
            this.End = end;
            this.Off = off;
            this.On = on;
        }

        public SimpleScheduleEntry(
            LocalTime start,
            LocalTime end,
            Duration off = default,
            Duration on = default)
        {
            var midnight = LocalTime.Midnight;
            this.StartType = EventType.TimeOfDay;
            this.Start = (start - midnight).ToDuration();
            this.EndType = EventType.TimeOfDay;
            this.End = (end - midnight).ToDuration();
            this.Off = off;
            this.On = on;
        }

        public ulong Raw { get => this.raw; }

        public EventType StartType
        {
            get
            {
                return (EventType)ReadBitRange(this.Raw, 60, 62);
            }

            init
            {
                WriteBitRange(ref this.raw, 60, 62, (ulong)value);
            }
        }

        public Duration Start
        {
            get
            {
                var sign = ReadBitRange(this.Raw, 59, 60) == 1 ? -1 : 1;
                long mangitude = (long)ReadBitRange(this.Raw, 48, 59);

                return Duration.FromMinutes(sign * mangitude);
            }

            init
            {
                var minutes = (long)value.TotalMinutes;
                uint sign = minutes < 0 ? 1u : 0u;
                ulong magnitude = (ulong)(minutes < 0 ? minutes * -1 : minutes);

                WriteBitRange(ref this.raw, 59, 60, sign);
                WriteBitRange(ref this.raw, 48, 59, magnitude);
            }
        }

        public EventType EndType
        {
            get
            {
                return (EventType)ReadBitRange(this.Raw, 44, 46);
            }

            init
            {
                WriteBitRange(ref this.raw, 44, 46, (ulong)value);
            }
        }

        public Duration End
        {
            get
            {
                var sign = ReadBitRange(this.Raw, 43, 44) == 1 ? -1 : 1;
                long mangitude = (long)ReadBitRange(this.Raw, 32, 43);
                return Duration.FromMinutes(sign * mangitude);
            }

            init
            {
                var minutes = (long)value.TotalMinutes;
                uint sign = minutes < 0 ? 1u : 0u;
                ulong magnitude = (ulong)(minutes < 0 ? minutes * -1 : minutes);

                WriteBitRange(ref this.raw, 43, 44, sign);
                WriteBitRange(ref this.raw, 32, 43, magnitude);
            }
        }

        public Duty Duty => ReadBitRange(this.Raw, 0, 32) == 0 ? Duty.Always : Duty.Cycle;

        public Duration On
        {
            get
            {
                return Duration.FromMinutes(ReadBitRange(this.Raw, 11, 22));
            }

            init
            {
                WriteBitRange(ref this.raw, 11, 22, (ulong)value.TotalMinutes);
            }
        }

        public Duration Off
        {
            get
            {
                return Duration.FromMinutes(ReadBitRange(this.Raw, 0, 11));
            }

            init
            {
                WriteBitRange(ref this.raw, 0, 11, (ulong)value.TotalMinutes);
            }
        }
    }
}
