// <copyright file="NoteChunk.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.WAVE
{
    public readonly record struct NoteChunk(uint CuePointId, string Text) : ICueWithText;
}
