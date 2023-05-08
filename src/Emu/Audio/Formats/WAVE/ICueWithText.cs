// <copyright file="ICueWithText.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.WAVE
{
    public interface ICueWithText
    {
        public uint CuePointId { get; }

        public string Text { get; }
    }
}
