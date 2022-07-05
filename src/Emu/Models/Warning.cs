// <copyright file="Warning.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models
{
    /// <inheritdoc />
    public record Warning(WellKnownProblem Problem) : Notice(Problem);
}
