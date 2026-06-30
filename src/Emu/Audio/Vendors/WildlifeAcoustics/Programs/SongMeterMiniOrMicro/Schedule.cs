// <copyright file="Schedule.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro
{
    using LanguageExt;
    using NodaTime;

    public partial record Schedule
    {
        public bool ScheduleFromRecording { get; init; }

        public LocalDateTime? DelayStart { get; init; }

        public byte SchedulesCount { get; init; }

        public Arr<ScheduleEntry> Entries { get; init; }

        public byte DefaultAlwaysOnSchedule { get; init; }
    }
}
