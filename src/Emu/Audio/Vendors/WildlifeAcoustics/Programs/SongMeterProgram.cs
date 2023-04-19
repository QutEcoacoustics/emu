// <copyright file="SongMeterProgram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using Emu.Models;
    using LanguageExt;
    using NodaTime;
    using UnitsNet.NumberExtensions.NumberToEnergy;

    /// <summary>
    /// See WildlifeAcoustics\schedule_structure.md for more information.
    /// </summary>
    public abstract record SongMeterProgram
    {
        public abstract int Version { get; }

        public string Prefix { get; init; } = string.Empty;

        public bool PrefixEnabled { get; init; }

        public Offset Timezone { get; init; }

        public bool TimezoneEnabled { get; init; }

        public abstract Location Position { get; init; }

        public bool PositionEnabled { get; init; }

        public SolarMode SolarMode { get; init; } = SolarMode.Actual;

        public float BatteryCutoffVoltage { get; init; }

        public bool BatteryCutoffVoltageEnabled { get; init; }

        public float SensitivityLeft { get; init; }

        public float SensitivityRight { get; init; }

        public bool SensitivityEnabled { get; init; }

        public abstract string Model { get; init; }

        public Arr<AdvancedScheduleEntry> AdvancedSchedule { get; init; }

        public ulong ScenarioMemoryCardA { get; init; }

        public ulong ScenarioMemoryCardB { get; init; }

        public virtual SongMeterMicrophone ScenarioMicrophone0 { get; init; }

        public virtual SongMeterMicrophone ScenarioMicrophone1 { get; init; }

        public float ScenarioTriggerRatio { get; init; } = 0.10f;

        public double ScenarioBatteryEnergy { get; init; } = 0x48.WattHours().Joules;

        public LocalDateTime ScenarioStart { get; init; }
    }
}
