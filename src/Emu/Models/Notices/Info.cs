// <copyright file="Info.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models.Notices
{
    public record Info(string Message, WellKnownProblem Problem = null) : Notice(Message, Problem);
}
