// <copyright file="Program.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro
{
    /// <summary>
    /// See WildlifeAcoustics\schedule_structure.md for more information.
    /// </summary>
    /// <remarks>
    /// This program and configuration format is distinctly different to previous Song Meter models.
    /// It is used in the Song Meter Mini and Song Meter Micro models.
    /// </remarks>
    public record Program
    {
        public Configuration Configuration { get; init; }

        public Schedule Schedule { get; init; }
    }
}
