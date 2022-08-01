// <copyright file="LabelChunk.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.WAVE
{
    public readonly record struct LabelChunk(uint CuePointId, string Text) : ICueWithText;
}
