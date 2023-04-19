// <copyright file="SongMeter3Program.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using Emu.Models;
    using NodaTime;
    using UnitsNet.NumberExtensions.NumberToEnergy;

    /// <summary>
    /// See WildlifeAcoustics\schedule_structure.md for more information.
    /// </summary>
    public record SongMeter3Program : SongMeterProgram
    {
        public override int Version => 3;

        public override Location Position { get; init; } = new Location()
        {
            Latitude = 0,
            Longitude = 0,
            LatitudePrecision = 2,
            LongitudePrecision = 2,
        };

        public bool SolarModeEnabled { get; init; }

        public ulong ScenarioMemoryCardC { get; init; }

        public ulong ScenarioMemoryCardD { get; init; }

        public ushort Unknown498 { get; init; }

        public override string Model { get; init; } = Models.SM3;
    }
}
