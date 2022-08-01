// <copyright file="Cue.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.WAVE
{
    public record Cue(uint SamplePosition, string Label, string Note, string Text);
}
