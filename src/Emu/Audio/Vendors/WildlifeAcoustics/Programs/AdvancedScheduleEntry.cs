// <copyright file="AdvancedScheduleEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs
{
    using System;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes;
    using static AdvancedScheduleEntryType;
    using static Emu.Utilities.BinaryHelpers;

    public abstract record AdvancedScheduleEntry
    {
        private static readonly Dictionary<AdvancedScheduleEntryType, Type> Mapping = new()
        {
            { HPF, typeof(Hpf) },
            { GAIN, typeof(Gain) },
            { FS, typeof(FullSpectrum) },
            { ZC, typeof(ZeroCrossing) },
            { FREQMIN, typeof(FrequencyMinimum) },
            { FREQMAX, typeof(FrequencyMaximum) },
            { DMIN, typeof(DurationMinimum) },
            { DMAX, typeof(DurationMaximum) },
            { TRGLVL, typeof(TriggerLevel) },
            { TRGWIN, typeof(TriggerWindow) },
            { TRGMAX, typeof(TriggerMaximum) },
            { NAP, typeof(Nap) },
            { AT_DATE, typeof(AtDate) },
            { AT_TIME, typeof(AtTime) },
            { AT_SRIS, typeof(AtSunrise) },
            { AT_SSET, typeof(AtSunset) },
            { REPEAT, typeof(Repeat) },
            { UNTDATE, typeof(UntilDate) },
            { UNTTIME, typeof(UntilTime) },
            { UNTSRIS, typeof(UntilSunrise) },
            { UNTSSET, typeof(UntilSunset) },
            { UNTCOUNT, typeof(UntilCount) },
            { RECORD, typeof(Record) },
            { PAUSE, typeof(Pause) },
            { PLAY, typeof(Play) },
            { FEATURE, typeof(Feature) },
        };

#pragma warning disable SA1401 // Fields should be private - needs to modified by reference in derived classes
        protected uint raw;
#pragma warning restore SA1401 // Fields should be private

        protected AdvancedScheduleEntry()
        {
        }

        protected uint Raw { get => this.raw; private init => this.raw = value; }

        public AdvancedScheduleEntryType Type
        {
            get
            {
                return (AdvancedScheduleEntryType)ReadHighest6Bits(this.Raw);
            }

            protected init
            {
                WriteBitRange(ref this.raw, 26, 32, (uint)value);
            }
        }

        public static AdvancedScheduleEntry Create(uint raw)
        {
            var type = (AdvancedScheduleEntryType)ReadHighest6Bits(raw);
            if (!Mapping.TryGetValue(type, out var instanceType))
            {
                throw new NotSupportedException($"Schedule entry of type `{type}` is not recognized");
            }

            return (AdvancedScheduleEntry)Activator.CreateInstance(instanceType)! with { Raw = raw };
        }
    }
}
