// <copyright file="CueResult.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Cues
{
    using Emu.Audio.WAVE;
    using Rationals;

    public record CueResult(string File, Rational Position, Cue Cue);
}
