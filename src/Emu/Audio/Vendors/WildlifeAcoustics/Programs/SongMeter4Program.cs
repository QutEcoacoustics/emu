// <copyright file="SongMeter4Program.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using Emu.Models;
    using LanguageExt;
    using NodaTime;
    using Duration = NodaTime.Duration;

    /// <summary>
    /// See WildlifeAcoustics\schedule_structure.md for more information.
    /// </summary>
    public record SongMeter4Program : SongMeterProgram
    {
        internal const ushort PreampLeftFlag = 0b1000_0000_0000_0000;
        internal const ushort PreampRightFlag = 0b0100_0000_0000_0000;

        public override int Version => 4;

        public override string Model { get; init; } = Models.SM4;

        public override Location Position { get; init; } = new Location()
        {
            Latitude = 0,
            Longitude = 0,
            LatitudePrecision = 5,
            LongitudePrecision = 5,
        };

        public LocalDate DelayStart { get; init; } = ProgramParser.WildlifeAcousticsEpoch;

        public bool DelayStartEnabled { get; init; }

        public LedSettings LedSettings { get; init; } = LedSettings.LedAlways;

        public float TriggerWindow { get; init; } = 3f;

        public Channel Channels { get; init; } = Channel.Stereo;

        public double GainLeft { get; init; } = 16;

        public double GainRight { get; init; } = 16;

        public HighPassFilter HighPassFilterLeft { get; init; }

        public HighPassFilter HighPassFilterRight { get; init; }

        public uint SampleRate { get; init; } = 24_000;

        public ushort DivisionRatio { get; init; } = 8;

        public Preamp PreampLeft { get; init; } = Preamp.On26dB;

        public Preamp PreampRight { get; init; } = Preamp.On26dB;

        public float MinDuration { get; init; }

        public float MaxDuration { get; init; }

        public uint MinTriggerFrequency { get; init; } = 16_000;

        public ushort Unknown574 { get; init; } = 16;

        public ushort Unknown578 { get; init; }

        public ushort Unknown582 { get; init; }

        public short TriggerLevel { get; init; }

        public ushort Unknown586 { get; init; }

        public ushort MaxTriggerTime { get; init; } = 15;

        public Duration MaxLength { get; init; } = Duration.FromHours(1);

        public Compression Compression { get; init; }

        public override SongMeterMicrophone ScenarioMicrophone0 { get; init; } = SongMeterMicrophone.Internal;

        public override SongMeterMicrophone ScenarioMicrophone1 { get; init; } = SongMeterMicrophone.Internal;

        public ScheduleMode ScheduleMode { get; init; } = ScheduleMode.Daily;

        public Arr<SimpleScheduleEntry> Schedule { get; init; } = Arr.create<SimpleScheduleEntry>(new SimpleScheduleEntry(0));

        public Arr<Range> Bitmap1 { get; init; } = Arr.create<Range>(new Range(0, 86400));

        public Arr<Range> Bitmap2 { get; init; } = Arr.create<Range>(new Range(0, 86400));
    }
}
