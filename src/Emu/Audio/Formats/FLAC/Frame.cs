// <copyright file="Frame.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio
{
    public record Frame(uint Index, long Offset, FrameHeader Header);
}
